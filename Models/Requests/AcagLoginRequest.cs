using System.Text.Json.Serialization;

namespace ACAG_PSER_Integration.Requests.Models;

public sealed class AcagLoginRequest
{
    [JsonPropertyName("username")]
    public string Username { get; init; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; init; } = string.Empty;
}