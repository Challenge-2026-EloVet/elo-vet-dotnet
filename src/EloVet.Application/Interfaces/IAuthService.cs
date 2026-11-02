using EloVet.Application.Models;
using EloVet.Domain.Entities;

namespace EloVet.Application.Interfaces;

public interface IAuthService
{
    Task<User> RegisterAsync(AuthCredentials credentials);

    Task<User?> ValidateCredentialsAsync(AuthCredentials credentials);
}