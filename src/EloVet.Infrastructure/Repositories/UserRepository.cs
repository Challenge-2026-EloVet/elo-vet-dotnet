using EloVet.Application.Interfaces;
using EloVet.Domain.Entities;
using MongoDB.Driver;

namespace EloVet.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _users;

    public UserRepository(IMongoDatabase database)
    {
        _users = database.GetCollection<User>("Users");
    }

    public Task<User?> FindByUsernameAsync(string username)
    {
        var filter = Builders<User>.Filter.Eq(user => user.Username, username);
        return FindFirstOrDefaultAsync(filter);
    }

    private async Task<User?> FindFirstOrDefaultAsync(FilterDefinition<User> filter)
    {
        return await _users.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<User> CreateAsync(User user)
    {
        await _users.InsertOneAsync(user);
        return user;
    }
}