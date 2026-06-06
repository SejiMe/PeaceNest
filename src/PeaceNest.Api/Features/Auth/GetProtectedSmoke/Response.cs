namespace PeaceNest.Api.Features.Auth.GetProtectedSmoke;

public sealed record Response(
    string? Subject,
    string? Email,
    string? Role);
