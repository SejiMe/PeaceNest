namespace PeaceNest.Api.Common.Database;

public interface IAuditableEntity
{
    DateTimeOffset CreatedAt { get; set; }

    DateTimeOffset UpdatedAt { get; set; }
}
