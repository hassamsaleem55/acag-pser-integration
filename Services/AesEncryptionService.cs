using System.Security.Cryptography;
using System.Text;

namespace ACAG_PSER_Integration.Services;

internal static class AesEncryptionService
{
    private const int IvSizeBytes = 16;

    public static string Encrypt(string plainText, string base64Key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plainText);
        ArgumentException.ThrowIfNullOrWhiteSpace(base64Key);

        byte[] key = Convert.FromBase64String(base64Key);

        using var aes = Aes.Create();

        aes.Key = key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.GenerateIV();

        using var memoryStream = new MemoryStream();

        // Store the IV at the beginning of the payload.
        memoryStream.Write(aes.IV);

        using (var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write, leaveOpen: true))
        using (var writer = new StreamWriter(cryptoStream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(plainText);
        }

        return Convert.ToBase64String(memoryStream.ToArray());
    }

    public static string Decrypt(string cipherText, string base64Key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cipherText);
        ArgumentException.ThrowIfNullOrWhiteSpace(base64Key);

        byte[] key = Convert.FromBase64String(base64Key);
        byte[] payload = Convert.FromBase64String(cipherText);

        if (payload.Length <= IvSizeBytes)
        {
            throw new CryptographicException("The supplied data could not be processed.");
        }

        byte[] iv = payload[..IvSizeBytes];
        byte[] cipherBytes = payload[IvSizeBytes..];

        using var aes = Aes.Create();

        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var memoryStream = new MemoryStream(cipherBytes);

        using var cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read);

        using var reader = new StreamReader(cryptoStream, Encoding.UTF8);

        return reader.ReadToEnd();
    }
}