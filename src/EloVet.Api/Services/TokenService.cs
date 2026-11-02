using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace EloVet.Api.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(string username)
    {
        var key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key não configurada.");

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException(
                "Jwt:Key está vazia. Configure uma chave secreta para gerar o token.");
        }

        if (key.Length < 32)
        {
            throw new InvalidOperationException(
                "Jwt:Key deve possuir pelo menos 32 caracteres.");
        }

        var issuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("Jwt:Issuer não configurado.");
        var audience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("Jwt:Audience não configurado.");
        var expiration = _configuration.GetValue("Jwt:ExpirationInMinutes", 120);

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            [
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            ],
            expires: DateTime.UtcNow.AddMinutes(expiration),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}