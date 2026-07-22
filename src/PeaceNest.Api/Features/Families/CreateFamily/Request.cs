namespace PeaceNest.Api.Features.Families.CreateFamily;

public sealed record Request(
    string Name,
    string? Description,
    string PreferredCurrency = "PHP");
