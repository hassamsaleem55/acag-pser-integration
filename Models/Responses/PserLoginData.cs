using System.Text.Json.Serialization;

namespace ACAG_PSER_Integration.Models.Responses;

public sealed class PserLoginData
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; init; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; }

    [JsonPropertyName("expires_at")]
    public string ExpiresAt { get; init; } = string.Empty;
}