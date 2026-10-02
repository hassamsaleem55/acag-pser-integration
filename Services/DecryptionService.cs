using ACAG_PSER_Integration.Models.Configuration;
using ACAG_PSER_Integration.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace ACAG_PSER_Integration.Services;

public sealed class DecryptionService : IDecryptionService
{
    private readonly string _secretKey;

    public DecryptionService(IOptions<PserApiSettings> options)
    {
        _secretKey = options.Value.SecretKey;

        if (string.IsNullOrWhiteSpace(_secretKey))
        {
            throw new InvalidOperationException("EncryptionSettings:SecretKey is not configured.");
        }
    }

    public string Decrypt(string cipherText)
    {
        return AesEncryptionService.Decrypt(cipherText, _secretKey);
    }
}