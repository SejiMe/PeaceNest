using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;
using PeaceNest.Api.Common.Errors;

namespace PeaceNest.Api.Common.Auth;

public sealed class CurrentUserService
{
    private readonly PeaceNestDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public CurrentUserService(PeaceNestDbContext dbContext, TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    public async Task<User> GetOrCreateUserAsync(
        AuthenticatedSupabaseUser authenticatedUser,
        CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .SingleOrDefaultAsync(user => user.SupabaseUserId == authenticatedUser.SupabaseUserId, cancellationToken);

        if (user is not null)
        {
            user.Email = authenticatedUser.Email;
            user.LastSeenAt = _timeProvider.GetUtcNow();
            await _dbContext.SaveChangesAsync(cancellationToken);
            return user;
        }

        var emailOwnerExists = await _dbContext.Users
            .AnyAsync(user => user.Email == authenticatedUser.Email, cancellationToken);

        if (emailOwnerExists)
        {
            throw new ConflictAppException("This Google email is already connected to another PeaceNest user.");
        }

        user = new User
        {
            Id = Guid.NewGuid(),
            SupabaseUserId = authenticatedUser.SupabaseUserId,
            Email = authenticatedUser.Email,
            DisplayName = CreateDisplayName(authenticatedUser.Email),
            LastSeenAt = _timeProvider.GetUtcNow()
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return user;
    }

    private static string CreateDisplayName(string email)
    {
        var atIndex = email.IndexOf('@', StringComparison.Ordinal);
        return atIndex > 0 ? email[..atIndex] : email;
    }
}
