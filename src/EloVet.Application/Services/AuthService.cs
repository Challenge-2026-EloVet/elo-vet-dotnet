using EloVet.Application.Interfaces;
using EloVet.Application.Models;
using EloVet.Domain.Entities;

namespace EloVet.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<User> RegisterAsync(AuthCredentials credentials)
    {
        var username = credentials.Username.Trim();

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(credentials.Password))
        {
            throw new ArgumentException("Usuário e senha são obrigatórios.");
        }

        if (await _userRepository.FindByUsernameAsync(username) is not null)
        {
            throw new InvalidOperationException("O usuário informado já existe.");
        }

        return await _userRepository.CreateAsync(new User
        {
            Username = username,
            PasswordHash = _passwordHasher.Hash(credentials.Password)
        });
    }

    public async Task<User?> ValidateCredentialsAsync(AuthCredentials credentials)
    {
        var user = await _userRepository.FindByUsernameAsync(credentials.Username.Trim());

        return user is not null && _passwordHasher.Verify(credentials.Password, user.PasswordHash)
            ? user
            : null;
    }
}