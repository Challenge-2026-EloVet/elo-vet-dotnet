using EloVet.Api.Services;
using Microsoft.Extensions.Configuration;

namespace EloVet.UnitTests.Services;

public class TokenServiceTests
{
    [Fact]
    public void GenerateToken_ConfiguracaoValida_GeraToken()
    {
        var service = new TokenService(CreateConfiguration());

        var token = service.GenerateToken("user");

        Assert.NotEmpty(token);
        Assert.Equal(3, token.Split('.').Length);
    }

    [Fact]
    public void GenerateToken_SemChave_LancaExcecao()
    {
        var service = new TokenService(new ConfigurationBuilder().Build());

        var exception = Assert.Throws<InvalidOperationException>(() =>
            service.GenerateToken("user"));

        Assert.Equal("Jwt:Key não configurada.", exception.Message);
    }

    [Fact]
    public void GenerateToken_ComChaveVazia_LancaExcecaoClara()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "   "
            })
            .Build();
        var service = new TokenService(configuration);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            service.GenerateToken("user"));

        Assert.Equal(
            "Jwt:Key está vazia. Configure uma chave secreta para gerar o token.",
            exception.Message);
    }

    [Fact]
    public void GenerateToken_ComChaveCurta_LancaExcecaoClara()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "short-key"
            })
            .Build();
        var service = new TokenService(configuration);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            service.GenerateToken("user"));

        Assert.Equal(
            "Jwt:Key deve possuir pelo menos 32 caracteres.",
            exception.Message);
    }

    private static IConfiguration CreateConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "unit-test-key-change-me-32-characters",
                ["Jwt:Issuer"] = "EloVet.Api",
                ["Jwt:Audience"] = "EloVet.Client",
                ["Jwt:ExpirationInMinutes"] = "120"
            })
            .Build();
    }
}