namespace PeaceNest.Api.Common.Database;

public interface IConcurrencyTrackedEntity
{
    int Version { get; set; }
}
