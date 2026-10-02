using System.Text.Json.Serialization;

namespace ACAG_PSER_Integration.Models.Responses;

public sealed class PserErrorDetails
{
    [JsonPropertyName("code")]
    public string? Code { get; init; }

    [JsonPropertyName("details")]
    public string? Details { get; init; }
}