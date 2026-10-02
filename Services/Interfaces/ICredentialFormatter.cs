using ACAG_PSER_Integration.Services.Enums;

namespace ACAG_PSER_Integration.Services.Interfaces;

public interface ICredentialFormatter
{
    string Format(string value, CredentialType type);
    string ExtractOriginalValue(string value, CredentialType type);

    // Convenience methods
    string FormatUsername(string username) => Format(username, CredentialType.Username);
    string FormatPassword(string password) => Format(password, CredentialType.Password);
    string ExtractUsername(string formatted) => ExtractOriginalValue(formatted, CredentialType.Username);
    string ExtractPassword(string formatted) => ExtractOriginalValue(formatted, CredentialType.Password);
}