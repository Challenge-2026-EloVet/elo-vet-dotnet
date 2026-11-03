using EloVet.Application.Exceptions;
using EloVet.Application.Interfaces;
using EloVet.Application.Models;
using EloVet.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EloVet.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<User> RegisterAsync(AuthCredentials credentials)
    {
        var username = credentials.Username.Trim();

        _logger.LogInformation(
            "Iniciando cadastro do usuário {Username}",
            username);

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(credentials.Password))
        {
            _logger.LogWarning(
                "Cadastro do usuário {Username} recusado por credenciais obrigatórias ausentes",
                username);

            throw new InvalidInputException("Usuário e senha são obrigatórios.");
        }

        if (await _userRepository.FindByUsernameAsync(username) is not null)
        {
            _logger.LogWarning(
                "Cadastro recusado porque o usuário {Username} já existe",
                username);

            throw new ConflictException("O usuário informado já existe.");
        }

        var user = await _userRepository.CreateAsync(new User
        {
            Username = username,
            PasswordHash = _passwordHasher.Hash(credentials.Password)
        });

        _logger.LogInformation(
            "Cadastro do usuário {Username} concluído",
            user.Username);

        return user;
    }

    public async Task<User?> ValidateCredentialsAsync(AuthCredentials credentials)
    {
        var username = credentials.Username.Trim();

        _logger.LogInformation(
            "Validando credenciais do usuário {Username}",
            username);

        var user = await _userRepository.FindByUsernameAsync(username);

        if (user is null || !_passwordHasher.Verify(credentials.Password, user.PasswordHash))
        {
            _logger.LogWarning(
                "Credenciais inválidas para o usuário {Username}",
                username);

            return null;
        }

        _logger.LogInformation(
            "Credenciais do usuário {Username} validadas",
            username);

        return user;
    }
}