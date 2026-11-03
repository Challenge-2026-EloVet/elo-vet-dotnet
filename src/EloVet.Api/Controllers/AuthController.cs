using EloVet.Api.Models.Auth;
using EloVet.Api.Services;
using EloVet.Application.Interfaces;
using EloVet.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EloVet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        ITokenService tokenService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _tokenService = tokenService;
        _logger = logger;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(AuthRequest request)
    {
        _logger.LogInformation(
            "Recebida solicitação para cadastrar usuário {Username}",
            request.Username);
       
        var user = await _authService.RegisterAsync(
            new AuthCredentials(request.Username, request.Password));

        _logger.LogInformation(
            "Cadastro do usuário {Username} concluído",
            user.Username);

        return Created($"/api/Auth/users/{user.Id}", new { user.Username });
    }

    [HttpPost("signin")]
    [AllowAnonymous]
    public async Task<IActionResult> SignIn(AuthRequest request)
    {
        _logger.LogInformation(
            "Recebida tentativa de autenticação do usuário {Username}",
            request.Username);

        var user = await _authService.ValidateCredentialsAsync(
            new AuthCredentials(request.Username, request.Password));

        if (user is null)
        {
            _logger.LogWarning(
                "Autenticação recusada para o usuário {Username}",
                request.Username);

            return Unauthorized();
        }

        var token = _tokenService.GenerateToken(user.Username);

        _logger.LogInformation(
            "Autenticação do usuário {Username} concluída",
            user.Username);

        return Ok(new AuthResponse(token));
    }
}