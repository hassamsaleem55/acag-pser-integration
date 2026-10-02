using System.Text.Json.Serialization;

namespace ACAG_PSER_Integration.Models.Responses;

public sealed class GeneratePserPayloadResponse
{
    [JsonPropertyName("username")]
    public string Username { get; init; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; init; } = string.Empty;
}