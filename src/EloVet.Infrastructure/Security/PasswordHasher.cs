using System.Security.Cryptography;
using EloVet.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace EloVet.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    private readonly ILogger<PasswordHasher> _logger;

    public PasswordHasher(ILogger<PasswordHasher> logger)
    {
        _logger = logger;
    }

    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);

        _logger.LogInformation("Hash de senha gerado com PBKDF2");

        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }

    public bool Verify(string password, string passwordHash)
    {
        var parts = passwordHash.Split('.');

        if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
        {
            _logger.LogWarning("Tentativa de validar um hash de senha inválido");
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(parts[1]);
            var expectedKey = Convert.FromBase64String(parts[2]);
            var actualKey = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expectedKey.Length);

            var isValid = CryptographicOperations.FixedTimeEquals(actualKey, expectedKey);

            _logger.LogInformation(
                "Validação de hash de senha concluída. Resultado: {IsValid}",
                isValid);

            return isValid;
        }
        catch (FormatException)
        {
            _logger.LogWarning("Tentativa de validar um hash de senha malformado");
            return false;
        }
    }
}