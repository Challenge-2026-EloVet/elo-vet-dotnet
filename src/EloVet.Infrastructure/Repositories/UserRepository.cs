using EloVet.Application.Interfaces;
using EloVet.Domain.Entities;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;

namespace EloVet.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _users;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(
        IMongoDatabase database,
        ILogger<UserRepository> logger)
    {
        _users = database.GetCollection<User>("Users");
        _logger = logger;
    }

    public async Task<User?> FindByUsernameAsync(string username)
    {
        try
        {
            _logger.LogInformation(
                "Consultando usuário {Username}",
                username);

            var filter = Builders<User>.Filter.Eq(user => user.Username, username);
            return await FindFirstOrDefaultAsync(filter);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Falha ao consultar usuário {Username}",
                username);
            throw;
        }
    }

    private async Task<User?> FindFirstOrDefaultAsync(FilterDefinition<User> filter)
    {
        return await _users.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<User> CreateAsync(User user)
    {
        try
        {
            _logger.LogInformation(
                "Persistindo usuário {Username}",
                user.Username);

            await _users.InsertOneAsync(user);

            _logger.LogInformation(
                "Usuário {Username} persistido com sucesso",
                user.Username);

            return user;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Falha ao persistir usuário {Username}",
                user.Username);
            throw;
        }
    }
}