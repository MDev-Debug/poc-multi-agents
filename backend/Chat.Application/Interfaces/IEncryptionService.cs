namespace Chat.Application.Interfaces;

/// <summary>
/// Serviço de criptografia simétrica para dados sensíveis em repouso.
/// Implementação usa AES-256-GCM (AEAD): confidencialidade + integridade.
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Cifra o texto plano e retorna string Base64 no formato: IV(12b)+CipherText+Tag(16b).
    /// </summary>
    string Encrypt(string plaintext);

    /// <summary>
    /// Decifra e verifica a integridade. Lança <see cref="System.Security.Cryptography.CryptographicException"/> se corrompido/adulterado.
    /// </summary>
    string Decrypt(string ciphertext);
}
