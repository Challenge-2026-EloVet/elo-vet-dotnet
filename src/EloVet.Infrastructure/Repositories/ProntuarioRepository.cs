using EloVet.Domain.Entities;
using EloVet.Application.Interfaces;
using MongoDB.Driver;
 
namespace EloVet.Infrastructure.Repositories;

public class ProntuarioRepository : IProntuarioRepository
{
    private readonly IMongoCollection<Prontuario> _prontuarios;

    public ProntuarioRepository(IMongoDatabase database)
    {
        _prontuarios = database.GetCollection<Prontuario>("prontuarios");
    }

    public async Task SalvarAsync(Prontuario prontuario)
    {
        await _prontuarios.InsertOneAsync(prontuario);
    }

    public async Task<Prontuario?> FindByPetIdAsync(string petId)
    {
        var filter = Builders<Prontuario>.Filter.Eq(p => p.Id, petId);
        return await _prontuarios.Find(filter).FirstOrDefaultAsync();
    }
}