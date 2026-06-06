namespace PeaceNest.Api.Common.Database;

public interface ISoftDeletableEntity
{
    DateTimeOffset? DeletedAt { get; set; }
}
