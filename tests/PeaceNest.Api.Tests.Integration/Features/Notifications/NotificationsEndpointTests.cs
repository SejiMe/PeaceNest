using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;
using PeaceNest.Api.Common.Errors;
using PeaceNest.Api.Tests.Integration.Support;
using CreateFamilyRequest = PeaceNest.Api.Features.Families.CreateFamily.Request;
using CreateFamilyResponse = PeaceNest.Api.Features.Families.CreateFamily.Response;
using ListNotificationsResponse = PeaceNest.Api.Features.Notifications.ListNotifications.Response;
using MarkNotificationReadResponse = PeaceNest.Api.Features.Notifications.MarkNotificationRead.Response;

namespace PeaceNest.Api.Tests.Integration.Features.Notifications;

public sealed class NotificationsEndpointTests
{
    [Fact]
    public async Task ListNotifications_ReturnsOnlyAuthenticatedRecipientsFamilyNotifications()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var ownerClient = CreateAuthenticatedClient(
            factory,
            "aaaaaaaa-8888-8888-8888-aaaaaaaaaaaa",
            "owner@example.test");
        var family = await CreateFamilyAsync(ownerClient, "Notification Nest");
        var owner = await FindUserByEmailAsync(factory, "owner@example.test");
        var adultUser = await AddMemberAsync(
            factory,
            family.Id,
            "aaaaaaaa-9999-9999-9999-aaaaaaaaaaaa",
            "adult@example.test",
            FamilyMemberRole.AdultMember);
        await AddNotificationAsync(
            factory,
            family.Id,
            owner.Id,
            adultUser.Id,
            NotificationType.CommentAdded,
            "New family note",
            "A note was added to a family plan.");
        await AddNotificationAsync(
            factory,
            family.Id,
            adultUser.Id,
            owner.Id,
            NotificationType.VoteCast,
            "Someone voted",
            "A family member shared a vote.");

        using var response = await ownerClient.GetAsync($"/families/{family.Id}/notifications");
        var payload = await response.Content.ReadFromJsonAsync<ListNotificationsResponse>();

        Assert.True(
            response.StatusCode == HttpStatusCode.OK,
            await response.Content.ReadAsStringAsync());
        Assert.NotNull(payload);
        var notification = Assert.Single(payload.Notifications);
        Assert.Equal(owner.Id, notification.RecipientUserId);
        Assert.Equal(adultUser.Id, notification.ActorUserId);
        Assert.Equal("adult", notification.ActorDisplayName);
        Assert.Equal("New family note", notification.Title);
        Assert.Equal("A note was added to a family plan.", notification.Body);
        Assert.Equal(1, payload.UnreadCount);
    }

    [Fact]
    public async Task ListNotifications_KeepsPreviewTextMinimal()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var ownerClient = CreateAuthenticatedClient(
            factory,
            "bbbbbbbb-8888-8888-8888-bbbbbbbbbbbb",
            "owner@example.test");
        var family = await CreateFamilyAsync(ownerClient, "Privacy Nest");
        var owner = await FindUserByEmailAsync(factory, "owner@example.test");
        var plan = await AddPlanAsync(
            factory,
            family.Id,
            owner.Id,
            "Private tuition plan",
            "Actual tuition amount is private.");
        var comment = await AddCommentAsync(
            factory,
            plan.Id,
            owner.Id,
            "Sensitive note about family finances.");
        await AddNotificationAsync(
            factory,
            family.Id,
            owner.Id,
            owner.Id,
            NotificationType.CommentAdded,
            "New family note",
            "A note was added to a family plan.",
            relatedPlanId: plan.Id,
            relatedCommentId: comment.Id);

        using var response = await ownerClient.GetAsync($"/families/{family.Id}/notifications");
        var payload = await response.Content.ReadFromJsonAsync<ListNotificationsResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        var notification = Assert.Single(payload.Notifications);

        Assert.DoesNotContain("tuition", notification.Body ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("finances", notification.Body ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(plan.Id, notification.RelatedPlanId);
        Assert.Equal(comment.Id, notification.RelatedCommentId);
    }

    [Fact]
    public async Task MarkNotificationRead_SetsReadTimestampForRecipient()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var ownerClient = CreateAuthenticatedClient(
            factory,
            "cccccccc-8888-8888-8888-cccccccccccc",
            "owner@example.test");
        var family = await CreateFamilyAsync(ownerClient, "Read Nest");
        var owner = await FindUserByEmailAsync(factory, "owner@example.test");
        var notificationId = await AddNotificationAsync(
            factory,
            family.Id,
            owner.Id,
            actorUserId: null,
            NotificationType.MonthlyRecapReady,
            "Monthly recap is ready",
            "Your family recap is ready.");

        using var response = await ownerClient.PutAsync($"/families/{family.Id}/notifications/{notificationId}/read", content: null);
        var payload = await response.Content.ReadFromJsonAsync<MarkNotificationReadResponse>();

        Assert.True(
            response.StatusCode == HttpStatusCode.OK,
            await response.Content.ReadAsStringAsync());
        Assert.NotNull(payload);
        Assert.Equal(notificationId, payload.Notification.Id);
        Assert.NotNull(payload.Notification.ReadAt);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
        var notification = await dbContext.Notifications.SingleAsync();

        Assert.NotNull(notification.ReadAt);
    }

    [Fact]
    public async Task MarkNotificationRead_ReturnsNotFoundForAnotherRecipientsNotification()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var ownerClient = CreateAuthenticatedClient(
            factory,
            "dddddddd-8888-8888-8888-dddddddddddd",
            "owner@example.test");
        var family = await CreateFamilyAsync(ownerClient, "Recipient Nest");
        var owner = await FindUserByEmailAsync(factory, "owner@example.test");
        var adultUser = await AddMemberAsync(
            factory,
            family.Id,
            "dddddddd-9999-9999-9999-dddddddddddd",
            "adult@example.test",
            FamilyMemberRole.AdultMember);
        var notificationId = await AddNotificationAsync(
            factory,
            family.Id,
            adultUser.Id,
            owner.Id,
            NotificationType.PlanUpdated,
            "Plan updated",
            "A family plan changed.");

        using var response = await ownerClient.PutAsync($"/families/{family.Id}/notifications/{notificationId}/read", content: null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await ProblemDetailsAssert.HasProblemDetailsAsync(
            response,
            404,
            ErrorCodes.ResourceNotFound);
    }

    [Fact]
    public async Task ListNotifications_RejectsOutsider()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var ownerClient = CreateAuthenticatedClient(
            factory,
            "eeeeeeee-8888-8888-8888-eeeeeeeeeeee",
            "owner@example.test");
        using var outsiderClient = CreateAuthenticatedClient(
            factory,
            "eeeeeeee-9999-9999-9999-eeeeeeeeeeee",
            "outsider@example.test");
        var family = await CreateFamilyAsync(ownerClient, "Private Notification Nest");

        using var response = await outsiderClient.GetAsync($"/families/{family.Id}/notifications");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        await ProblemDetailsAssert.HasProblemDetailsAsync(
            response,
            403,
            ErrorCodes.AuthorizationDenied);
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

    private static async Task<User> FindUserByEmailAsync(TestingApiFactory factory, string email)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
        return await dbContext.Users.SingleAsync(user => user.Email == email);
    }

    private static async Task<User> AddMemberAsync(
        TestingApiFactory factory,
        Guid familyId,
        string supabaseUserId,
        string email,
        FamilyMemberRole role)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
        var user = new User
        {
            Id = Guid.NewGuid(),
            SupabaseUserId = Guid.Parse(supabaseUserId),
            Email = email,
            DisplayName = email.Split('@')[0]
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

        return user;
    }

    private static async Task<FamilyPlan> AddPlanAsync(
        TestingApiFactory factory,
        Guid familyId,
        Guid createdByUserId,
        string title,
        string description)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
        var plan = new FamilyPlan
        {
            Id = Guid.NewGuid(),
            FamilyId = familyId,
            CreatedByUserId = createdByUserId,
            PlanType = PlanType.WantNeed,
            Title = title,
            Description = description,
            ProgressPercent = 0,
            PriorityScore = 1
        };

        dbContext.FamilyPlans.Add(plan);
        await dbContext.SaveChangesAsync();

        return plan;
    }

    private static async Task<Comment> AddCommentAsync(
        TestingApiFactory factory,
        Guid planId,
        Guid authorUserId,
        string body)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            PlanId = planId,
            AuthorUserId = authorUserId,
            Body = body
        };

        dbContext.Comments.Add(comment);
        await dbContext.SaveChangesAsync();

        return comment;
    }

    private static async Task<Guid> AddNotificationAsync(
        TestingApiFactory factory,
        Guid familyId,
        Guid recipientUserId,
        Guid? actorUserId,
        NotificationType type,
        string title,
        string body,
        Guid? relatedPlanId = null,
        Guid? relatedCommentId = null,
        Guid? relatedRecapId = null)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            FamilyId = familyId,
            RecipientUserId = recipientUserId,
            ActorUserId = actorUserId,
            Type = type,
            Title = title,
            Body = body,
            RelatedPlanId = relatedPlanId,
            RelatedCommentId = relatedCommentId,
            RelatedRecapId = relatedRecapId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.Notifications.Add(notification);
        await dbContext.SaveChangesAsync();

        return notification.Id;
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
