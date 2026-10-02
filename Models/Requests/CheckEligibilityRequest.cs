using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ACAG_PSER_Integration.Models.Requests;

public sealed class CheckEligibilityRequest
{
    [Required(ErrorMessage = "Username is required.")]
    [JsonPropertyName("username")]
    public string Username { get; init; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [JsonPropertyName("password")]
    public string Password { get; init; } = string.Empty;

    [Required(ErrorMessage = "Applicant CNIC is required.")]
    [RegularExpression(@"^\d{13}$", ErrorMessage = "Applicant CNIC  must be exactly 13 numeric digits.")]
    [JsonPropertyName("ApplicantCNIC")]
    public string ApplicantCNIC { get; init; } = string.Empty;
}