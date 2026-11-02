using EloVet.Infrastructure.Security;

namespace EloVet.UnitTests.Security;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Hash_SenhaValida_GeraHashComFormatoEsperado()
    {
        var hash = _hasher.Hash("secret");

        var parts = hash.Split('.');
        Assert.Equal(3, parts.Length);
        Assert.Equal("100000", parts[0]);
        Assert.NotEqual("secret", hash);
    }

    [Fact]
    public void Hash_MesmaSenha_GeraHashesDiferentesPorUsarSalt()
    {
        var firstHash = _hasher.Hash("secret");
        var secondHash = _hasher.Hash("secret");

        Assert.NotEqual(firstHash, secondHash);
    }

    [Fact]
    public void Verify_SenhaCorreta_RetornaTrue()
    {
        var hash = _hasher.Hash("secret");

        Assert.True(_hasher.Verify("secret", hash));
    }

    [Fact]
    public void Verify_SenhaIncorretaOuHashInvalido_RetornaFalse()
    {
        var hash = _hasher.Hash("secret");

        Assert.False(_hasher.Verify("wrong", hash));
        Assert.False(_hasher.Verify("secret", "invalid-hash"));
    }
}