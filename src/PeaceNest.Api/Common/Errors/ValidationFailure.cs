namespace PeaceNest.Api.Common.Errors;

public sealed record ValidationFailure(string Field, string Message);
