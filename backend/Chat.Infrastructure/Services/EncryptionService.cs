using System.Security.Cryptography;
using System.Text;
using Chat.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Chat.Infrastructure.Services;

/// <summary>
/// Implementação de AES-256-GCM.
/// Formato do payload armazenado (Base64 de bytes concatenados):
///   [12 bytes IV] + [N bytes ciphertext] + [16 bytes GCM tag]
///
/// Configuração obrigatória (uma das duas):
///   - Variável de ambiente: CHAT_ENCRYPTION_KEY (Base64, 32 bytes = 256 bits)
///   - appsettings: Encryption:Key (mesmo formato)
/// </summary>
public sealed class EncryptionService : IEncryptionService
{
    private const int IvSize  = 12;  // GCM recomenda 96 bits
    private const int TagSize = 16;  // 128-bit authentication tag

    private readonly byte[] _key;

    public EncryptionService(IConfiguration configuration)
    {
        // Prioridade: variável de ambiente > appsettings
        var keyBase64 =
            Environment.GetEnvironmentVariable("CHAT_ENCRYPTION_KEY")
            ?? configuration["Encryption:Key"]
            ?? throw new InvalidOperationException(
                "A chave de criptografia não está configurada. " +
                "Defina a variável de ambiente CHAT_ENCRYPTION_KEY " +
                "ou a configuração Encryption:Key com 32 bytes em Base64.");

        _key = Convert.FromBase64String(keyBase64);

        if (_key.Length != 32)
            throw new InvalidOperationException(
                "CHAT_ENCRYPTION_KEY deve ter exatamente 32 bytes (256 bits) em Base64.");
    }

    public string Encrypt(string plaintext)
    {
        ArgumentNullException.ThrowIfNull(plaintext);

        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var iv             = new byte[IvSize];
        RandomNumberGenerator.Fill(iv);

        var ciphertext = new byte[plaintextBytes.Length];
        var tag        = new byte[TagSize];

        using var aes = new AesGcm(_key, TagSize);
        aes.Encrypt(iv, plaintextBytes, ciphertext, tag);

        // Layout: IV | ciphertext | tag
        var combined = new byte[IvSize + ciphertext.Length + TagSize];
        Buffer.BlockCopy(iv,         0, combined, 0,                          IvSize);
        Buffer.BlockCopy(ciphertext, 0, combined, IvSize,                     ciphertext.Length);
        Buffer.BlockCopy(tag,        0, combined, IvSize + ciphertext.Length, TagSize);

        return Convert.ToBase64String(combined);
    }

    public string Decrypt(string ciphertext)
    {
        ArgumentNullException.ThrowIfNull(ciphertext);

        var combined = Convert.FromBase64String(ciphertext);

        if (combined.Length < IvSize + TagSize)
            throw new CryptographicException("Payload cifrado inválido: tamanho insuficiente.");

        var iv             = combined[..IvSize];
        var tag            = combined[^TagSize..];
        var encryptedBytes = combined[IvSize..^TagSize];
        var plaintextBytes = new byte[encryptedBytes.Length];

        using var aes = new AesGcm(_key, TagSize);
        aes.Decrypt(iv, encryptedBytes, tag, plaintextBytes);

        return Encoding.UTF8.GetString(plaintextBytes);
    }
}
