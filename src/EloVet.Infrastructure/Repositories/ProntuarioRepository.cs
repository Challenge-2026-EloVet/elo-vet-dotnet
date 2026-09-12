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

    public async Task<IEnumerable<Prontuario>> ListarAsync()
    {
        return await _prontuarios.Find(_ => true).ToListAsync();
    }

    public async Task<Prontuario?> FindByIdAsync(string id)
    {
        var filter = Builders<Prontuario>.Filter.Eq(prontuario => prontuario.Id, id);
        return await _prontuarios.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<Prontuario?> FindByPetIdAsync(string petId)
    {
        var filter = Builders<Prontuario>.Filter.Eq(prontuario => prontuario.Pet.Id, petId);
        return await _prontuarios.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<Prontuario?> EditarAsync(string id, Prontuario prontuarioAtualizado)
    {
        var existente = await FindByIdAsync(id);

        if (existente is null)
        {
            return null;
        }

        prontuarioAtualizado.Id = id;

        var filter = Builders<Prontuario>.Filter.Eq(prontuario => prontuario.Id, id);
        await _prontuarios.ReplaceOneAsync(filter, prontuarioAtualizado);

        return prontuarioAtualizado;
    }

    public async Task ExcluirAsync(string id)
    {
        var filter = Builders<Prontuario>.Filter.Eq(prontuario => prontuario.Id, id);
        await _prontuarios.DeleteOneAsync(filter);
    }
}