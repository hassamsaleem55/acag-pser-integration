namespace ACAG_PSER_Integration.Models.Configuration;

public sealed class PserApiSettings
{
    public const string SectionName = "PserApiSettings";

    public string BaseUrl { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
}