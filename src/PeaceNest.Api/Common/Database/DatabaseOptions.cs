namespace PeaceNest.Api.Common.Database;

public sealed class DatabaseOptions
{
    public const string ConnectionStringName = "PeaceNest";
    public const string SectionName = "Database";

    public bool EnableSensitiveDataLogging { get; set; }
}
