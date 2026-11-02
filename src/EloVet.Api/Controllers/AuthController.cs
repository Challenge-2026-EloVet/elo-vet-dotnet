using EloVet.Api.Models.Auth;
using EloVet.Api.Services;
using EloVet.Application.Interfaces;
using EloVet.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EloVet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;

    public AuthController(IAuthService authService, ITokenService tokenService)
    {
        _authService = authService;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(AuthRequest request)
    {
        try
        {
            var user = await _authService.RegisterAsync(
                new AuthCredentials(request.Username, request.Password));

            return Created($"/api/Auth/users/{user.Id}", new { user.Username });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(exception.Message);
        }
    }

    [HttpPost("signin")]
    [AllowAnonymous]
    public async Task<IActionResult> SignIn(AuthRequest request)
    {
        var user = await _authService.ValidateCredentialsAsync(
            new AuthCredentials(request.Username, request.Password));

        return user is null
            ? Unauthorized()
            : Ok(new AuthResponse(_tokenService.GenerateToken(user.Username)));
    }
}