using System.Text.Json.Serialization;

namespace ACAG_PSER_Integration.Models.Responses;

public sealed class PserApiResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("status_code")]
    public int StatusCode { get; init; }

    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;

    [JsonPropertyName("data")]
    public T? Data { get; init; }

    [JsonPropertyName("error")]
    public PserErrorDetails? Error { get; init; }
}