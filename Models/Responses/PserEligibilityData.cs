using System.Text.Json.Serialization;

namespace ACAG_PSER_Integration.Models.Responses;

public sealed class PserEligibilityData
{
    [JsonPropertyName("cnic")]
    public string Cnic { get; init; } = string.Empty;

    [JsonPropertyName("pser_eligibility")]
    public string PserEligibility { get; init; } = string.Empty;

    [JsonPropertyName("reason_code")]
    public string ReasonCode { get; init; } = string.Empty;
}