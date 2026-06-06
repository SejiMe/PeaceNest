namespace PeaceNest.Api.Common.Errors;

public static class ErrorCodes
{
    public const string ValidationFailed = "validation.failed";
    public const string AuthenticationRequired = "authentication.required";
    public const string AuthorizationDenied = "authorization.denied";
    public const string ResourceNotFound = "resource.not_found";
    public const string ResourceConflict = "resource.conflict";
    public const string DomainRuleViolated = "domain.rule_violated";
    public const string RateLimitExceeded = "rate_limit.exceeded";
    public const string ExternalProviderUnavailable = "external_provider.unavailable";
    public const string Unexpected = "server.unexpected";
}
