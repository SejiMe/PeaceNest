using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace PeaceNest.Api.Common.Database;

public static class JsonDocumentMapping
{
    public static readonly ValueConverter<JsonDocument, string> Converter = new(
        document => document.RootElement.GetRawText(),
        json => JsonDocument.Parse(json));

    public static readonly ValueComparer<JsonDocument> Comparer = new(
        (left, right) => ToJson(left) == ToJson(right),
        document => ToJson(document).GetHashCode(StringComparison.Ordinal),
        document => JsonDocument.Parse(ToJson(document)));

    private static string ToJson(JsonDocument? document) =>
        document?.RootElement.GetRawText() ?? "{}";
}
