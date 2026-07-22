using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;
using PeaceNest.Api.Common.Errors;
using PeaceNest.Api.Common.Security;
using PeaceNest.Api.Tests.Integration.Support;
using AcceptInvitationRequest = PeaceNest.Api.Features.Invitations.AcceptInvitation.Request;
using AcceptInvitationResponse = PeaceNest.Api.Features.Invitations.AcceptInvitation.Response;
using CreateFamilyRequest = PeaceNest.Api.Features.Families.CreateFamily.Request;
using CreateFamilyResponse = PeaceNest.Api.Features.Families.CreateFamily.Response;
using CreateInvitationRequest = PeaceNest.Api.Features.Invitations.CreateInvitation.Request;
using CreateInvitationResponse = PeaceNest.Api.Features.Invitations.CreateInvitation.Response;

namespace PeaceNest.Api.Tests.Integration.Features.Invitations;

public sealed class FamilyInvitationEndpointTests
{
    [Fact]
    public async Task CreateInvitation_AsOwnerStoresOnlyCredentialHashes()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var ownerClient = CreateAuthenticatedClient(
            factory,
            "aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa",
            "owner@example.test");
        var family = await CreateFamilyAsync(ownerClient, "Invite Nest");

        using var response = await ownerClient.PostAsJsonAsync(
            $"/families/{family.Id}/invitations",
            new CreateInvitationRequest(" invited@example.test ", FamilyMemberRole.AdultMember));
        var payload = await response.Content.ReadFromJsonAsync<CreateInvitationResponse>();

        Assert.True(
            response.StatusCode == HttpStatusCode.Created,
            await response.Content.ReadAsStringAsync());
        Assert.NotNull(payload);
        Assert.Equal("invited@example.test", payload.InvitedEmail);
        Assert.Equal(FamilyInvitationStatus.Pending, payload.Status);
        Assert.NotEmpty(payload.InvitationToken);
        Assert.NotEmpty(payload.InvitationCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<InvitationTokenService>();
        var invitation = await dbContext.FamilyInvitations.SingleAsync();

        Assert.NotEqual(payload.InvitationToken, invitation.TokenHash);
        Assert.Equal(tokenService.HashToken(payload.InvitationToken), invitation.TokenHash);
        Assert.NotEqual(payload.InvitationCode, invitation.InvitationCodeHash);
        Assert.Equal(tokenService.HashCode(payload.InvitationCode), invitation.InvitationCodeHash);
    }


    [Fact]
    public async Task CreateInvitation_RejectsMemberWithoutInvitePermission()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var ownerClient = CreateAuthenticatedClient(
            factory,
            "bbbbbbbb-1111-1111-1111-bbbbbbbbbbbb",
            "owner@example.test");
        var family = await CreateFamilyAsync(ownerClient, "Permission Nest");

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
        var memberUser = new User
        {
            Id = Guid.NewGuid(),
            SupabaseUserId = Guid.Parse("bbbbbbbb-2222-2222-2222-bbbbbbbbbbbb"),
            Email = "adult@example.test",
            DisplayName = "adult"
        };
        dbContext.Users.Add(memberUser);
        dbContext.FamilyMembers.Add(new FamilyMember
        {
            Id = Guid.NewGuid(),
            FamilyId = family.Id,
            UserId = memberUser.Id,
            Role = FamilyMemberRole.AdultMember,
            Status = FamilyMemberStatus.Active,
            JoinedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        using var adultClient = CreateAuthenticatedClient(
            factory,
            memberUser.SupabaseUserId.ToString(),
            memberUser.Email);
        using var response = await adultClient.PostAsJsonAsync(
            $"/families/{family.Id}/invitations",
            new CreateInvitationRequest("child@example.test", FamilyMemberRole.ChildMember));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        await ProblemDetailsAssert.HasProblemDetailsAsync(
            response,
            403,
            ErrorCodes.AuthorizationDenied);
    }

    [Fact]
    public async Task AcceptInvitation_WithMatchingGoogleEmailCreatesMembership()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var ownerClient = CreateAuthenticatedClient(
            factory,
            "cccccccc-1111-1111-1111-cccccccccccc",
            "owner@example.test");
        var family = await CreateFamilyAsync(ownerClient, "Accept Nest");
        var invitation = await CreateInvitationAsync(
            ownerClient,
            family.Id,
            "new-parent@example.test",
            FamilyMemberRole.ParentAdmin);
        using var invitedClient = CreateAuthenticatedClient(
            factory,
            "cccccccc-2222-2222-2222-cccccccccccc",
            "new-parent@example.test");

        using var response = await invitedClient.PostAsJsonAsync(
            "/family-invitations/accept",
            new AcceptInvitationRequest(InvitationCode: invitation.InvitationCode));
        var payload = await response.Content.ReadFromJsonAsync<AcceptInvitationResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(family.Id, payload.FamilyId);
        Assert.Equal(FamilyMemberRole.ParentAdmin, payload.Role);
        Assert.Equal(FamilyMemberStatus.Active, payload.MembershipStatus);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
        var storedInvitation = await dbContext.FamilyInvitations.SingleAsync(invite => invite.Id == invitation.Id);
        var invitedUser = await dbContext.Users.SingleAsync(user => user.Email == "new-parent@example.test");
        var membership = await dbContext.FamilyMembers.SingleAsync(member =>
            member.FamilyId == family.Id && member.UserId == invitedUser.Id);

        Assert.Equal(FamilyInvitationStatus.Accepted, storedInvitation.Status);
        Assert.Equal(invitedUser.Id, storedInvitation.AcceptedByUserId);
        Assert.Equal(FamilyMemberRole.ParentAdmin, membership.Role);
    }

    [Fact]
    public async Task AcceptInvitation_RejectsDifferentGoogleEmail()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var ownerClient = CreateAuthenticatedClient(
            factory,
            "dddddddd-1111-1111-1111-dddddddddddd",
            "owner@example.test");
        var family = await CreateFamilyAsync(ownerClient, "Email Bound Nest");
        var invitation = await CreateInvitationAsync(
            ownerClient,
            family.Id,
            "invited@example.test",
            FamilyMemberRole.AdultMember);
        using var wrongEmailClient = CreateAuthenticatedClient(
            factory,
            "dddddddd-2222-2222-2222-dddddddddddd",
            "someone-else@example.test");

        using var response = await wrongEmailClient.PostAsJsonAsync(
            "/family-invitations/accept",
            new AcceptInvitationRequest(invitation.InvitationToken));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        await ProblemDetailsAssert.HasProblemDetailsAsync(
            response,
            403,
            ErrorCodes.AuthorizationDenied);
    }

    [Fact]
    public async Task AcceptInvitation_MarksExpiredInvitationAsExpired()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var ownerClient = CreateAuthenticatedClient(
            factory,
            "eeeeeeee-1111-1111-1111-eeeeeeeeeeee",
            "owner@example.test");
        var family = await CreateFamilyAsync(ownerClient, "Expired Nest");
        var invitation = await CreateInvitationAsync(
            ownerClient,
            family.Id,
            "late@example.test",
            FamilyMemberRole.Viewer);

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
            var storedInvitation = await dbContext.FamilyInvitations.SingleAsync(invite => invite.Id == invitation.Id);
            storedInvitation.ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1);
            await dbContext.SaveChangesAsync();
        }

        using var invitedClient = CreateAuthenticatedClient(
            factory,
            "eeeeeeee-2222-2222-2222-eeeeeeeeeeee",
            "late@example.test");
        using var response = await invitedClient.PostAsJsonAsync(
            "/family-invitations/accept",
            new AcceptInvitationRequest(invitation.InvitationToken));

        Assert.Equal((HttpStatusCode)422, response.StatusCode);
        await ProblemDetailsAssert.HasProblemDetailsAsync(
            response,
            422,
            ErrorCodes.DomainRuleViolated);

        using var verifyScope = factory.Services.CreateScope();
        var verifyDbContext = verifyScope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
        var expiredInvitation = await verifyDbContext.FamilyInvitations.SingleAsync(invite => invite.Id == invitation.Id);

        Assert.Equal(FamilyInvitationStatus.Expired, expiredInvitation.Status);
    }

    private static async Task<CreateFamilyResponse> CreateFamilyAsync(HttpClient client, string name)
    {
        using var response = await client.PostAsJsonAsync("/families", new CreateFamilyRequest(name, null));
        var payload = await response.Content.ReadFromJsonAsync<CreateFamilyResponse>();

        Assert.True(
            response.StatusCode == HttpStatusCode.Created,
            await response.Content.ReadAsStringAsync());
        Assert.NotNull(payload);
        return payload;
    }

    private static async Task<CreateInvitationResponse> CreateInvitationAsync(
        HttpClient client,
        Guid familyId,
        string invitedEmail,
        FamilyMemberRole invitedRole)
    {
        using var response = await client.PostAsJsonAsync(
            $"/families/{familyId}/invitations",
            new CreateInvitationRequest(invitedEmail, invitedRole));
        var payload = await response.Content.ReadFromJsonAsync<CreateInvitationResponse>();

        Assert.True(
            response.StatusCode == HttpStatusCode.Created,
            await response.Content.ReadAsStringAsync());
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
