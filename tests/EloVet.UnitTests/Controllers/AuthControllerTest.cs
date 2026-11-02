using EloVet.Api.Controllers;
using EloVet.Api.Models.Auth;
using EloVet.Api.Services;
using EloVet.Application.Interfaces;
using EloVet.Application.Models;
using EloVet.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EloVet.UnitTests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authService = new();
    private readonly Mock<ITokenService> _tokenService = new();
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _controller = new AuthController(
            _authService.Object,
            _tokenService.Object);
    }

    [Fact]
    public async Task Register_CredenciaisValidas_RetornaCreated()
    {
        var request = new AuthRequest { Username = "user", Password = "secret" };
        _authService.Setup(service => service.RegisterAsync(
                new AuthCredentials("user", "secret")))
            .ReturnsAsync(new User { Id = "user-id", Username = "user" });

        var result = await _controller.Register(request);

        var created = Assert.IsType<CreatedResult>(result);
        Assert.Equal("/api/Auth/users/user-id", created.Location);
        Assert.Equal("user", created.Value!.GetType().GetProperty("Username")!.GetValue(created.Value));
    }

    [Fact]
    public async Task Register_UsuarioDuplicado_RetornaConflict()
    {
        _authService.Setup(service => service.RegisterAsync(It.IsAny<AuthCredentials>()))
            .ThrowsAsync(new InvalidOperationException("O usuário informado já existe."));

        var result = await _controller.Register(
            new AuthRequest { Username = "user", Password = "secret" });

        var conflict = Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal("O usuário informado já existe.", conflict.Value);
    }

    [Fact]
    public async Task Register_CredenciaisInvalidas_RetornaBadRequest()
    {
        _authService.Setup(service => service.RegisterAsync(It.IsAny<AuthCredentials>()))
            .ThrowsAsync(new ArgumentException("Usuário e senha são obrigatórios."));

        var result = await _controller.Register(new AuthRequest());

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Usuário e senha são obrigatórios.", badRequest.Value);
    }

    [Fact]
    public async Task SignIn_CredenciaisValidas_RetornaToken()
    {
        var user = new User { Username = "user" };
        _authService.Setup(service => service.ValidateCredentialsAsync(
                new AuthCredentials("user", "secret")))
            .ReturnsAsync(user);
        _tokenService.Setup(service => service.GenerateToken("user"))
            .Returns("jwt-token");

        var result = await _controller.SignIn(
            new AuthRequest { Username = "user", Password = "secret" });

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("jwt-token", Assert.IsType<AuthResponse>(ok.Value).Token);
        _tokenService.Verify(service => service.GenerateToken("user"), Times.Once);
    }

    [Fact]
    public async Task SignIn_CredenciaisInvalidas_RetornaUnauthorized()
    {
        _authService.Setup(service => service.ValidateCredentialsAsync(It.IsAny<AuthCredentials>()))
            .ReturnsAsync((User?)null);

        var result = await _controller.SignIn(
            new AuthRequest { Username = "user", Password = "wrong" });

        Assert.IsType<UnauthorizedResult>(result);
        _tokenService.Verify(service => service.GenerateToken(It.IsAny<string>()), Times.Never);
    }
}