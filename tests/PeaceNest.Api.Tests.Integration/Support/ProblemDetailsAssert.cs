using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace PeaceNest.Api.Tests.Integration.Support;

public static class ProblemDetailsAssert
{
    public static async Task<ProblemDetails> HasProblemDetailsAsync(
        HttpResponseMessage response,
        int expectedStatusCode,
        string expectedErrorCode)
    {
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problemDetails);
        Assert.Equal(expectedStatusCode, problemDetails.Status);
        Assert.Equal(expectedErrorCode, ReadStringExtension(problemDetails, "errorCode"));
        Assert.False(string.IsNullOrWhiteSpace(ReadStringExtension(problemDetails, "traceId")));

        return problemDetails;
    }

    private static string? ReadStringExtension(ProblemDetails problemDetails, string key)
    {
        var value = problemDetails.Extensions[key];

        return value switch
        {
            JsonElement element => element.GetString(),
            string text => text,
            _ => value?.ToString()
        };
    }
}
