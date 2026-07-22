using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;
using PeaceNest.Api.Common.JoinCodes;
using PeaceNest.Api.Features.FamilyJoinRequests;
using PeaceNest.Api.Tests.Integration.Support;
using ApproveRequest = PeaceNest.Api.Features.FamilyJoinRequests.ApproveJoinRequest.Request;
using ApproveResponse = PeaceNest.Api.Features.FamilyJoinRequests.ApproveJoinRequest.Response;
using CreateFamilyRequest = PeaceNest.Api.Features.Families.CreateFamily.Request;
using CreateFamilyResponse = PeaceNest.Api.Features.Families.CreateFamily.Response;
using CreateJoinRequest = PeaceNest.Api.Features.FamilyJoinRequests.CreateJoinRequest.Request;
using CreateJoinResponse = PeaceNest.Api.Features.FamilyJoinRequests.CreateJoinRequest.Response;
using GenerateJoinCodeResponse = PeaceNest.Api.Features.FamilyJoinCodes.GenerateJoinCode.Response;
using GetJoinCodeResponse = PeaceNest.Api.Features.FamilyJoinCodes.GetJoinCode.Response;
using ListFamilyRequestsResponse = PeaceNest.Api.Features.FamilyJoinRequests.ListFamilyJoinRequests.Response;
using ListNotificationsResponse = PeaceNest.Api.Features.Notifications.ListNotifications.Response;
using WithdrawResponse = PeaceNest.Api.Features.FamilyJoinRequests.WithdrawJoinRequest.Response;

namespace PeaceNest.Api.Tests.Integration.Features.FamilyJoinRequests;

public sealed class FamilyJoinRequestEndpointTests
{
    [Fact]
    public async Task AdultMember_CannotGenerateCodesOrReviewRequests()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var ownerClient = CreateAuthenticatedClient(factory, "10101010-aaaa-aaaa-aaaa-101010101010", "owner@example.test");
        var family = await CreateFamilyAsync(ownerClient, "Protected Nest");
        var adultSupabaseId = Guid.Parse("10101010-bbbb-bbbb-bbbb-101010101010");

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
            var adult = new User
            {
                Id = Guid.NewGuid(),
                SupabaseUserId = adultSupabaseId,
                Email = "adult@example.test",
                DisplayName = "adult",
                CountryCode = "PH",
                OnboardingCompletedAt = DateTimeOffset.UtcNow
            };
            dbContext.Users.Add(adult);
            dbContext.FamilyMembers.Add(new FamilyMember
            {
                Id = Guid.NewGuid(),
                FamilyId = family.Id,
                UserId = adult.Id,
                Role = FamilyMemberRole.AdultMember,
                Status = FamilyMemberStatus.Active,
                JoinedAt = DateTimeOffset.UtcNow
            });
            await dbContext.SaveChangesAsync();
        }

        using var adultClient = CreateAuthenticatedClient(factory, adultSupabaseId.ToString(), "adult@example.test");
        using var generateResponse = await adultClient.PostAsync($"/families/{family.Id}/join-code", null);
        using var reviewResponse = await adultClient.GetAsync($"/families/{family.Id}/join-requests");

        Assert.Equal(HttpStatusCode.Forbidden, generateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, reviewResponse.StatusCode);
    }

    [Fact]
    public async Task GenerateJoinCode_RevealsPlaintextOnceAndRotatesPreviousCode()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var ownerClient = CreateAuthenticatedClient(factory, "11111111-aaaa-aaaa-aaaa-111111111111", "owner@example.test");
        var family = await CreateFamilyAsync(ownerClient, "Rotation Nest");

        var first = await GenerateCodeAsync(ownerClient, family.Id);
        using var metadataResponse = await ownerClient.GetAsync($"/families/{family.Id}/join-code");
        var metadata = await metadataResponse.Content.ReadFromJsonAsync<GetJoinCodeResponse>();
        var second = await GenerateCodeAsync(ownerClient, family.Id);

        Assert.NotNull(metadata);
        Assert.True(metadata.HasActiveCode);
        Assert.Equal(first.Id, metadata.Id);
        Assert.NotEqual(first.Code, second.Code);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
        var stored = await dbContext.FamilyJoinCodes.OrderBy(code => code.CreatedAt).ToListAsync();
        Assert.Equal(2, stored.Count);
        Assert.Equal(FamilyJoinCodeStatus.Revoked, stored[0].Status);
        Assert.Equal(FamilyJoinCodeStatus.Active, stored[1].Status);
        Assert.DoesNotContain(stored, code => code.CodeHash == first.Code || code.CodeHash == second.Code);
    }

    [Fact]
    public async Task RedeemCode_IsIdempotentAndCreatesPendingRequestWithoutMembership()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var ownerClient = CreateAuthenticatedClient(factory, "22222222-aaaa-aaaa-aaaa-222222222222", "owner@example.test");
        var family = await CreateFamilyAsync(ownerClient, "Pending Nest");
        var code = await GenerateCodeAsync(ownerClient, family.Id);
        using var requesterClient = CreateAuthenticatedClient(factory, "22222222-bbbb-bbbb-bbbb-222222222222", "mia@gmail.com");

        var first = await RedeemCodeAsync(requesterClient, code.Code, HttpStatusCode.Created);
        var second = await RedeemCodeAsync(requesterClient, code.Code, HttpStatusCode.OK);
        using var reviewerResponse = await ownerClient.GetAsync($"/families/{family.Id}/join-requests");
        var reviewerPayload = await reviewerResponse.Content.ReadFromJsonAsync<ListFamilyRequestsResponse>();

        Assert.False(first.WasAlreadyPending);
        Assert.True(second.WasAlreadyPending);
        Assert.Equal(first.JoinRequest.Id, second.JoinRequest.Id);
        Assert.Equal(first.JoinRequest.CreatedAt, second.JoinRequest.CreatedAt);
        Assert.Equal(first.JoinRequest.ExpiresAt, second.JoinRequest.ExpiresAt);
        Assert.NotNull(reviewerPayload);
        var visibleRequest = Assert.Single(reviewerPayload.JoinRequests);
        Assert.Equal("mia", visibleRequest.RequesterDisplayName);
        Assert.Equal("m***@gmail.com", visibleRequest.MaskedRequesterEmail);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
        Assert.False(await dbContext.FamilyMembers.AnyAsync(member =>
            member.FamilyId == family.Id && member.User.Email == "mia@gmail.com"));
        var storedCode = await dbContext.FamilyJoinCodes.SingleAsync(candidate => candidate.Id == code.Id);
        Assert.Equal(1, storedCode.RequestCount);
        Assert.Single(await dbContext.FamilyJoinRequests.ToListAsync());
    }

    [Fact]
    public async Task ApproveJoinRequest_CreatesMembershipAndRequesterNotification()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var ownerClient = CreateAuthenticatedClient(factory, "33333333-aaaa-aaaa-aaaa-333333333333", "owner@example.test");
        var family = await CreateFamilyAsync(ownerClient, "Approval Nest");
        var code = await GenerateCodeAsync(ownerClient, family.Id);
        using var requesterClient = CreateAuthenticatedClient(factory, "33333333-bbbb-bbbb-bbbb-333333333333", "adult@example.test");
        var pending = await RedeemCodeAsync(requesterClient, code.Code, HttpStatusCode.Created);

        using var approveHttpResponse = await ownerClient.PostAsJsonAsync(
            $"/families/{family.Id}/join-requests/{pending.JoinRequest.Id}/approve",
            new ApproveRequest(FamilyMemberRole.AdultMember));
        var approved = await approveHttpResponse.Content.ReadFromJsonAsync<ApproveResponse>();
        using var notificationsResponse = await requesterClient.GetAsync("/notifications");
        var notifications = await notificationsResponse.Content.ReadFromJsonAsync<ListNotificationsResponse>();

        Assert.Equal(HttpStatusCode.OK, approveHttpResponse.StatusCode);
        Assert.NotNull(approved);
        Assert.Equal(FamilyJoinRequestStatus.Approved, approved.JoinRequest.Status);
        Assert.Equal(FamilyMemberRole.AdultMember, approved.JoinRequest.ApprovedRole);
        Assert.NotNull(notifications);
        Assert.Contains(notifications.Notifications, notification =>
            notification.Type == NotificationType.FamilyJoinRequestApproved &&
            notification.RelatedJoinRequestId == pending.JoinRequest.Id);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
        var membership = await dbContext.FamilyMembers
            .Include(member => member.User)
            .SingleAsync(member => member.FamilyId == family.Id && member.User.Email == "adult@example.test");
        Assert.Equal(FamilyMemberStatus.Active, membership.Status);
        Assert.Equal(FamilyMemberRole.AdultMember, membership.Role);
    }

    [Fact]
    public async Task WithdrawAndReject_AllowFreshRequestsWithoutGrantingAccess()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var ownerClient = CreateAuthenticatedClient(factory, "44444444-aaaa-aaaa-aaaa-444444444444", "owner@example.test");
        var family = await CreateFamilyAsync(ownerClient, "Retry Nest");
        var code = await GenerateCodeAsync(ownerClient, family.Id);
        using var requesterClient = CreateAuthenticatedClient(factory, "44444444-bbbb-bbbb-bbbb-444444444444", "requester@example.test");
        var first = await RedeemCodeAsync(requesterClient, code.Code, HttpStatusCode.Created);

        using var withdrawHttpResponse = await requesterClient.PostAsync(
            $"/family-join-requests/{first.JoinRequest.Id}/withdraw",
            null);
        var withdrawn = await withdrawHttpResponse.Content.ReadFromJsonAsync<WithdrawResponse>();
        var second = await RedeemCodeAsync(requesterClient, code.Code, HttpStatusCode.Created);
        using var rejectHttpResponse = await ownerClient.PostAsync(
            $"/families/{family.Id}/join-requests/{second.JoinRequest.Id}/reject",
            null);

        Assert.NotNull(withdrawn);
        Assert.Equal(FamilyJoinRequestStatus.Withdrawn, withdrawn.JoinRequest.Status);
        Assert.NotEqual(first.JoinRequest.Id, second.JoinRequest.Id);
        Assert.Equal(HttpStatusCode.OK, rejectHttpResponse.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
        Assert.False(await dbContext.FamilyMembers.AnyAsync(member =>
            member.FamilyId == family.Id && member.User.Email == "requester@example.test"));
    }

    [Fact]
    public async Task RedeemCode_StopsAtConfiguredDistinctRequestCapacity()
    {
        using var factory = TestingApiFactory.WithConfiguration(new Dictionary<string, string?>
        {
            ["Testing:UseInMemoryDatabase"] = "true",
            ["Testing:DatabaseName"] = $"peacenest-tests-{Guid.NewGuid():N}",
            ["JoinCodes:MaxRequestsPerCode"] = "1"
        });
        using var ownerClient = CreateAuthenticatedClient(factory, "55555555-aaaa-aaaa-aaaa-555555555555", "owner@example.test");
        var family = await CreateFamilyAsync(ownerClient, "Capacity Nest");
        var code = await GenerateCodeAsync(ownerClient, family.Id);
        using var firstClient = CreateAuthenticatedClient(factory, "55555555-bbbb-bbbb-bbbb-555555555555", "first@example.test");
        using var secondClient = CreateAuthenticatedClient(factory, "55555555-cccc-cccc-cccc-555555555555", "second@example.test");

        await RedeemCodeAsync(firstClient, code.Code, HttpStatusCode.Created);
        using var rejectedResponse = await secondClient.PostAsJsonAsync(
            "/family-join-requests",
            new CreateJoinRequest(code.Code));

        Assert.Equal((HttpStatusCode)422, rejectedResponse.StatusCode);
    }

    private static async Task<CreateFamilyResponse> CreateFamilyAsync(HttpClient client, string name)
    {
        using var response = await client.PostAsJsonAsync("/families", new CreateFamilyRequest(name, null));
        var payload = await response.Content.ReadFromJsonAsync<CreateFamilyResponse>();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(payload);
        return payload;
    }

    private static async Task<GenerateJoinCodeResponse> GenerateCodeAsync(HttpClient client, Guid familyId)
    {
        using var response = await client.PostAsync($"/families/{familyId}/join-code", null);
        var payload = await response.Content.ReadFromJsonAsync<GenerateJoinCodeResponse>();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Matches("^[23456789ABCDEFGHJKLMNPQRSTUVWXYZ]{5}-[23456789ABCDEFGHJKLMNPQRSTUVWXYZ]{5}$", payload.Code);
        return payload;
    }

    private static async Task<CreateJoinResponse> RedeemCodeAsync(
        HttpClient client,
        string code,
        HttpStatusCode expectedStatus)
    {
        using var response = await client.PostAsJsonAsync("/family-join-requests", new CreateJoinRequest(code));
        var payload = await response.Content.ReadFromJsonAsync<CreateJoinResponse>();
        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.NotNull(payload);
        return payload;
    }

    private static HttpClient CreateAuthenticatedClient(
        TestingApiFactory factory,
        string subject,
        string email)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            TestJwtTokenFactory.CreateSupabaseAccessToken(subject: subject, email: email));
        return client;
    }
}
