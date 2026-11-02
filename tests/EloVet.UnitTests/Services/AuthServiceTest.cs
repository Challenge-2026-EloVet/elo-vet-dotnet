using EloVet.Application.Interfaces;
using EloVet.Application.Models;
using EloVet.Application.Services;
using EloVet.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace EloVet.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _service = new AuthService(
            _userRepository.Object,
            _passwordHasher.Object);
    }

    [Fact]
    public async Task RegisterAsync_CredenciaisValidas_CriaUsuarioComSenhaHash()
    {
        var credentials = new AuthCredentials(" user ", "secret");
        _passwordHasher.Setup(hasher => hasher.Hash("secret"))
            .Returns("hashed-secret");
        _userRepository.Setup(repository => repository.FindByUsernameAsync("user"))
            .ReturnsAsync((User?)null);
        _userRepository.Setup(repository => repository.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync((User user) => user);

        var result = await _service.RegisterAsync(credentials);

        Assert.Equal("user", result.Username);
        Assert.Equal("hashed-secret", result.PasswordHash);
        _passwordHasher.Verify(hasher => hasher.Hash("secret"), Times.Once);
        _userRepository.Verify(repository => repository.CreateAsync(
            It.Is<User>(user => user.Username == "user" && user.PasswordHash == "hashed-secret")),
            Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_UsuarioExistente_LancaExcecao()
    {
        _userRepository.Setup(repository => repository.FindByUsernameAsync("user"))
            .ReturnsAsync(new User { Username = "user" });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.RegisterAsync(new AuthCredentials("user", "secret")));

        Assert.Equal("O usuário informado já existe.", exception.Message);
        _passwordHasher.Verify(hasher => hasher.Hash(It.IsAny<string>()), Times.Never);
        _userRepository.Verify(repository => repository.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    [Theory]
    [InlineData("", "secret")]
    [InlineData("user", "")]
    public async Task RegisterAsync_CredencialObrigatoriaAusente_LancaExcecao(
        string username,
        string password)
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.RegisterAsync(new AuthCredentials(username, password)));

        Assert.Equal("Usuário e senha são obrigatórios.", exception.Message);
        _userRepository.Verify(repository => repository.FindByUsernameAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ValidateCredentialsAsync_CredenciaisValidas_RetornaUsuario()
    {
        var user = new User { Username = "user", PasswordHash = "hash" };
        _userRepository.Setup(repository => repository.FindByUsernameAsync("user"))
            .ReturnsAsync(user);
        _passwordHasher.Setup(hasher => hasher.Verify("secret", "hash"))
            .Returns(true);

        var result = await _service.ValidateCredentialsAsync(
            new AuthCredentials(" user ", "secret"));

        Assert.Same(user, result);
        _passwordHasher.Verify(hasher => hasher.Verify("secret", "hash"), Times.Once);
    }

    [Fact]
    public async Task ValidateCredentialsAsync_SenhaIncorreta_RetornaNull()
    {
        _userRepository.Setup(repository => repository.FindByUsernameAsync("user"))
            .ReturnsAsync(new User { Username = "user", PasswordHash = "hash" });
        _passwordHasher.Setup(hasher => hasher.Verify("wrong", "hash"))
            .Returns(false);

        var result = await _service.ValidateCredentialsAsync(
            new AuthCredentials("user", "wrong"));

        Assert.Null(result);
    }

    [Fact]
    public async Task ValidateCredentialsAsync_UsuarioInexistente_RetornaNull()
    {
        _userRepository.Setup(repository => repository.FindByUsernameAsync("user"))
            .ReturnsAsync((User?)null);

        var result = await _service.ValidateCredentialsAsync(
            new AuthCredentials("user", "secret"));

        Assert.Null(result);
        _passwordHasher.Verify(hasher => hasher.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}