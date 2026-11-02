using EloVet.Domain.Entities;

namespace EloVet.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> FindByUsernameAsync(string username);

    Task<User> CreateAsync(User user);
}