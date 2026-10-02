using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ACAG_PSER_Integration.Models.Requests;

public sealed class GeneratePserPayloadRequest
{
    [Required(ErrorMessage = "Username is required.")]
    [JsonPropertyName("username")]
    public string Username { get; init; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [JsonPropertyName("password")]
    public string Password { get; init; } = string.Empty;
}