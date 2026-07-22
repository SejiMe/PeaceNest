using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;
using PeaceNest.Api.Common.FamilyRecovery;
using PeaceNest.Api.Tests.Integration.Support;
using CreateFamilyRequest = PeaceNest.Api.Features.Families.CreateFamily.Request;
using CreateFamilyResponse = PeaceNest.Api.Features.Families.CreateFamily.Response;
using CreateJoinRequest = PeaceNest.Api.Features.FamilyJoinRequests.CreateJoinRequest.Request;
using GenerateJoinCodeResponse = PeaceNest.Api.Features.FamilyJoinCodes.GenerateJoinCode.Response;
using LeaveFamilyResponse = PeaceNest.Api.Features.Families.LeaveFamily.Response;
using ListFamiliesResponse = PeaceNest.Api.Features.Families.ListFamilies.Response;
using RecoverFamilyRequest = PeaceNest.Api.Features.Families.RecoverFamily.Request;
using RecoverFamilyResponse = PeaceNest.Api.Features.Families.RecoverFamily.Response;

namespace PeaceNest.Api.Tests.Integration.Features.Families;

public sealed class FamilyDepartureAndRecoveryEndpointTests
{
    [Fact]
    public async Task NonOwner_CanLeaveWithoutDeactivatingWorkspace()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var ownerClient = CreateAuthenticatedClient(factory, "11000000-0000-0000-0000-000000000001", "owner@example.test");
        var family = await CreateFamilyAsync(ownerClient, "Shared Nest");
        var adultSubject = Guid.Parse("11000000-0000-0000-0000-000000000002");
        await AddMemberAsync(factory, family.Id, adultSubject, "adult@example.test", FamilyMemberRole.AdultMember);
        using var adultClient = CreateAuthenticatedClient(factory, adultSubject.ToString(), "adult@example.test");

        using var leaveResponse = await adultClient.PostAsync($"/families/{family.Id}/leave", null);
        var payload = await leaveResponse.Content.ReadFromJsonAsync<LeaveFamilyResponse>();

        Assert.Equal(HttpStatusCode.OK, leaveResponse.StatusCode);
        Assert.NotNull(payload);
        Assert.False(payload.RecoveryAvailable);
        Assert.Null(payload.RecoveryCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
        var adultMembership = await dbContext.FamilyMembers.SingleAsync(member => member.User.SupabaseUserId == adultSubject);
        Assert.Equal(FamilyMemberStatus.Removed, adultMembership.Status);
        Assert.NotNull(adultMembership.RemovedAt);
        Assert.Null((await dbContext.Families.SingleAsync()).DeletedAt);
    }

    [Fact]
    public async Task Owner_WithAnotherActiveMember_MustTransferOwnershipBeforeLeaving()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var ownerClient = CreateAuthenticatedClient(factory, "22000000-0000-0000-0000-000000000001", "owner@example.test");
        var family = await CreateFamilyAsync(ownerClient, "Guarded Nest");
        await AddMemberAsync(
            factory,
            family.Id,
            Guid.Parse("22000000-0000-0000-0000-000000000002"),
            "viewer@example.test",
            FamilyMemberRole.Viewer);

        using var leaveResponse = await ownerClient.PostAsync($"/families/{family.Id}/leave", null);

        Assert.Equal((HttpStatusCode)422, leaveResponse.StatusCode);
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
        Assert.Equal(2, await dbContext.FamilyMembers.CountAsync(member => member.Status == FamilyMemberStatus.Active));
        Assert.Null((await dbContext.Families.SingleAsync()).DeletedAt);
    }

    [Fact]
    public async Task SoleCreator_LeavesWithOneTimeRecoveryCodeAndCancelsAccessActivity()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var ownerClient = CreateAuthenticatedClient(factory, "33000000-0000-0000-0000-000000000001", "owner@example.test");
        var family = await CreateFamilyAsync(ownerClient, "Quiet Nest");
        var joinCode = await GenerateJoinCodeAsync(ownerClient, family.Id);
        using var requesterClient = CreateAuthenticatedClient(factory, "33000000-0000-0000-0000-000000000002", "requester@example.test");
        using var requestResponse = await requesterClient.PostAsJsonAsync("/family-join-requests", new CreateJoinRequest(joinCode.Code));
        Assert.Equal(HttpStatusCode.Created, requestResponse.StatusCode);

        using var leaveResponse = await ownerClient.PostAsync($"/families/{family.Id}/leave", null);
        var payload = await leaveResponse.Content.ReadFromJsonAsync<LeaveFamilyResponse>();

        Assert.Equal(HttpStatusCode.OK, leaveResponse.StatusCode);
        Assert.NotNull(payload);
        Assert.True(payload.RecoveryAvailable);
        Assert.Matches(
            "^[23456789ABCDEFGHJKLMNPQRSTUVWXYZ]{5}(-[23456789ABCDEFGHJKLMNPQRSTUVWXYZ]{5}){3}$",
            payload.RecoveryCode!);

        using var listResponse = await ownerClient.GetAsync("/families");
        var listed = await listResponse.Content.ReadFromJsonAsync<ListFamiliesResponse>();
        Assert.NotNull(listed);
        Assert.Empty(listed.Families);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
        var storedRecovery = await dbContext.FamilyRecoveryCodes.SingleAsync();
        var storedFamily = await dbContext.Families.IgnoreQueryFilters().SingleAsync();
        Assert.NotEqual(payload.RecoveryCode, storedRecovery.CodeHash);
        Assert.InRange(
            storedRecovery.ExpiresAt - storedRecovery.CreatedAt,
            TimeSpan.FromDays(30) - TimeSpan.FromSeconds(1),
            TimeSpan.FromDays(30));
        Assert.Equal(payload.RecoveryExpiresAt, storedRecovery.ExpiresAt);
        Assert.NotNull(storedFamily.DeletedAt);
        Assert.Equal(FamilyMemberStatus.Removed, (await dbContext.FamilyMembers.SingleAsync(member => member.User.Email == "owner@example.test")).Status);
        Assert.Equal(FamilyJoinCodeStatus.Revoked, (await dbContext.FamilyJoinCodes.SingleAsync()).Status);
        Assert.Equal(FamilyJoinRequestStatus.Cancelled, (await dbContext.FamilyJoinRequests.SingleAsync()).Status);
        Assert.All(await dbContext.Notifications.IgnoreQueryFilters().ToListAsync(), notification => Assert.NotNull(notification.DeletedAt));
    }

    [Fact]
    public async Task Recovery_RestoresOnlyTheCreatorAndConsumesCode()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var ownerClient = CreateAuthenticatedClient(factory, "44000000-0000-0000-0000-000000000001", "owner@example.test");
        var family = await CreateFamilyAsync(ownerClient, "Returning Nest");
        var recovery = await LeaveFamilyAsync(ownerClient, family.Id);
        using var outsiderClient = CreateAuthenticatedClient(factory, "44000000-0000-0000-0000-000000000002", "outsider@example.test");

        using var deniedResponse = await outsiderClient.PostAsJsonAsync("/families/recover", new RecoverFamilyRequest(recovery.RecoveryCode!));
        Assert.Equal(HttpStatusCode.Forbidden, deniedResponse.StatusCode);

        using var recoverResponse = await ownerClient.PostAsJsonAsync("/families/recover", new RecoverFamilyRequest(recovery.RecoveryCode!));
        var payload = await recoverResponse.Content.ReadFromJsonAsync<RecoverFamilyResponse>();
        Assert.Equal(HttpStatusCode.OK, recoverResponse.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(family.Id, payload.FamilyId);
        Assert.Equal(FamilyMemberRole.Owner, payload.Role);

        using var reusedResponse = await ownerClient.PostAsJsonAsync("/families/recover", new RecoverFamilyRequest(recovery.RecoveryCode!));
        Assert.Equal((HttpStatusCode)422, reusedResponse.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
        Assert.Null((await dbContext.Families.SingleAsync()).DeletedAt);
        Assert.Equal(FamilyMemberStatus.Active, (await dbContext.FamilyMembers.SingleAsync()).Status);
        Assert.Equal(FamilyRecoveryCodeStatus.Used, (await dbContext.FamilyRecoveryCodes.SingleAsync()).Status);
    }

    [Fact]
    public async Task ExpiredRecovery_AttemptPermanentlyPurgesFamilyOwnedData()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        const string ownerSubject = "55000000-0000-0000-0000-000000000001";
        using var ownerClient = CreateAuthenticatedClient(factory, ownerSubject, "owner@example.test");
        var family = await CreateFamilyAsync(ownerClient, "Closing Nest");
        await GenerateJoinCodeAsync(ownerClient, family.Id);
        var recovery = await LeaveFamilyAsync(ownerClient, family.Id);

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
            var storedRecovery = await dbContext.FamilyRecoveryCodes.SingleAsync();
            storedRecovery.ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1);
            await dbContext.SaveChangesAsync();
        }

        using var recoverResponse = await ownerClient.PostAsJsonAsync("/families/recover", new RecoverFamilyRequest(recovery.RecoveryCode!));
        Assert.Equal((HttpStatusCode)422, recoverResponse.StatusCode);

        using var verifyScope = factory.Services.CreateScope();
        var verifyDbContext = verifyScope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
        Assert.False(await verifyDbContext.Families.IgnoreQueryFilters().AnyAsync());
        Assert.False(await verifyDbContext.FamilyMembers.AnyAsync());
        Assert.False(await verifyDbContext.FamilyRecoveryCodes.AnyAsync());
        Assert.False(await verifyDbContext.FamilyJoinCodes.AnyAsync());
        Assert.True(await verifyDbContext.Users.AnyAsync(user => user.SupabaseUserId == Guid.Parse(ownerSubject)));
    }

    private static async Task<CreateFamilyResponse> CreateFamilyAsync(HttpClient client, string name)
    {
        using var response = await client.PostAsJsonAsync("/families", new CreateFamilyRequest(name, null));
        var payload = await response.Content.ReadFromJsonAsync<CreateFamilyResponse>();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(payload);
        return payload;
    }

    private static async Task<GenerateJoinCodeResponse> GenerateJoinCodeAsync(HttpClient client, Guid familyId)
    {
        using var response = await client.PostAsync($"/families/{familyId}/join-code", null);
        var payload = await response.Content.ReadFromJsonAsync<GenerateJoinCodeResponse>();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(payload);
        return payload;
    }

    private static async Task<LeaveFamilyResponse> LeaveFamilyAsync(HttpClient client, Guid familyId)
    {
        using var response = await client.PostAsync($"/families/{familyId}/leave", null);
        var payload = await response.Content.ReadFromJsonAsync<LeaveFamilyResponse>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        return payload;
    }

    private static async Task AddMemberAsync(
        TestingApiFactory factory,
        Guid familyId,
        Guid supabaseUserId,
        string email,
        FamilyMemberRole role)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
        var user = new User
        {
            Id = Guid.NewGuid(),
            SupabaseUserId = supabaseUserId,
            Email = email,
            DisplayName = email.Split('@')[0],
            CountryCode = "PH",
            OnboardingCompletedAt = DateTimeOffset.UtcNow
        };
        dbContext.Users.Add(user);
        dbContext.FamilyMembers.Add(new FamilyMember
        {
            Id = Guid.NewGuid(),
            FamilyId = familyId,
            UserId = user.Id,
            Role = role,
            Status = FamilyMemberStatus.Active,
            JoinedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();
    }

    private static HttpClient CreateAuthenticatedClient(TestingApiFactory factory, string subject, string email)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            TestJwtTokenFactory.CreateSupabaseAccessToken(subject: subject, email: email));
        return client;
    }
}
