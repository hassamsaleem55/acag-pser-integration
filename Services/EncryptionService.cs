using ACAG_PSER_Integration.Models.Configuration;
using ACAG_PSER_Integration.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace ACAG_PSER_Integration.Services;

public sealed class EncryptionService : IEncryptionService
{
    private readonly string _secretKey;

    public EncryptionService(IOptions<PserApiSettings> options)
    {
        _secretKey = options.Value.SecretKey;

        if (string.IsNullOrWhiteSpace(_secretKey))
        {
            throw new InvalidOperationException("EncryptionSettings:SecretKey is not configured.");
        }
    }

    public string Encrypt(string plainText)
    {
        return AesEncryptionService.Encrypt(plainText, _secretKey);
    }
}