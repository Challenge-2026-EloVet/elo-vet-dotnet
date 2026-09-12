using EloVet.Domain.Entities;
using EloVet.Application.Interfaces;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;
 
namespace EloVet.Infrastructure.Repositories;

public class ProntuarioRepository : IProntuarioRepository
{
    private readonly IMongoCollection<Prontuario> _prontuarios;
    private readonly ILogger<ProntuarioRepository> _logger;

    public ProntuarioRepository(IMongoDatabase database, ILogger<ProntuarioRepository> logger)
    {
        _prontuarios = database.GetCollection<Prontuario>("prontuarios");
        _logger = logger;
    }

    public async Task SalvarAsync(Prontuario prontuario)
    {
        try
        {
            await _prontuarios.InsertOneAsync(prontuario);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Falha ao salvar prontuario do pet {PetId}", prontuario.Pet.Id);
            throw;
        }
    }

    public async Task<IEnumerable<Prontuario>> ListarAsync()
    {
        try
        {
            return await _prontuarios.Find(_ => true).ToListAsync();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Falha ao listar prontuarios");
            throw;
        }
    }

    public async Task<Prontuario?> FindByIdAsync(string id)
    {
        try
        {
            var filter = Builders<Prontuario>.Filter.Eq(prontuario => prontuario.Id, id);
            return await _prontuarios.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Falha ao consultar prontuario {ProntuarioId}", id);
            throw;
        }
    }

    public async Task<Prontuario?> FindByPetIdAsync(string petId)
    {
        try
        {
            var filter = Builders<Prontuario>.Filter.Eq(prontuario => prontuario.Pet.Id, petId);
            return await _prontuarios.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Falha ao consultar prontuario do pet {PetId}", petId);
            throw;
        }
    }

    public async Task<Prontuario?> EditarAsync(string id, Prontuario prontuarioAtualizado)
    {
        try
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
        catch (Exception exception)
        {
            _logger.LogError(exception, "Falha ao atualizar prontuario {ProntuarioId}", id);
            throw;
        }
    }

    public async Task ExcluirAsync(string id)
    {
        try
        {
            var filter = Builders<Prontuario>.Filter.Eq(prontuario => prontuario.Id, id);
            await _prontuarios.DeleteOneAsync(filter);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Falha ao excluir prontuario {ProntuarioId}", id);
            throw;
        }
    }
}