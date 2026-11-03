using EloVet.Domain.Entities;
using EloVet.Application.Interfaces;
using EloVet.Application.Models;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
 
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

    public async Task<Prontuario> SalvarAsync(Prontuario prontuario)
    {
        try
        {
            await _prontuarios.InsertOneAsync(prontuario);
            return prontuario;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Falha ao salvar prontuario do pet {PetId}", prontuario.Pet.Id);
            throw;
        }
    }

    public async Task<PagedResult<Prontuario>> ListarAsync(ProntuarioQuery query)
    {
        try
        {
            // Começa com um filtro vazio: sem filtros informados, todos os documentos podem ser retornados.
            var filter = Builders<Prontuario>.Filter.Empty;

            // Aplica filtros exatos para o ID do pet e para o campo booleano de castração.
            if (!string.IsNullOrWhiteSpace(query.PetId))
            {
                filter &= Builders<Prontuario>.Filter.Eq(
                    prontuario => prontuario.Pet.Id,
                    query.PetId.Trim());
            }

            filter &= CreateTextFilter(query.PetName, prontuario => prontuario.Pet.Nome);
            filter &= CreateTextFilter(query.Species, prontuario => prontuario.Pet.Especie);
            filter &= CreateTextFilter(query.Breed, prontuario => prontuario.Pet.Raca);
            filter &= CreateTextFilter(query.Sex, prontuario => prontuario.Pet.Sexo);

            if (query.IsCastrado.HasValue)
            {
                filter &= Builders<Prontuario>.Filter.Eq(
                    prontuario => prontuario.Pet.isCastrado,
                    query.IsCastrado.Value);
            }

            // Conta os registros que atendem aos filtros para calcular o total de páginas.
            var totalRecords = await _prontuarios.CountDocumentsAsync(filter);

            // Executa no MongoDB a ordenação e a paginação antes de materializar a lista em memória.
            var items = await _prontuarios
                .Find(filter)
                .Sort(CreateSort(query.SortBy, query.Ascending))
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Limit(query.PageSize)
                .ToListAsync();

            // Arredonda para cima: uma página incompleta ainda conta como uma página.
            var totalPages = (int)Math.Ceiling(totalRecords / (double)query.PageSize);

            // Encapsula os itens e os metadados necessários para o cliente navegar pela paginação.
            return new PagedResult<Prontuario>(
                items,
                query.PageNumber,
                query.PageSize,
                totalPages,
                totalRecords);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Falha ao listar prontuarios paginados");
            throw;
        }
    }

    private static FilterDefinition<Prontuario> CreateTextFilter(
        string? value,
        Expression<Func<Prontuario, string>> field)
    {
        // Sem valor, não restringe a consulta; com valor, faz busca textual sem diferenciar maiúsculas.
        return string.IsNullOrWhiteSpace(value)
            ? Builders<Prontuario>.Filter.Empty
            : Builders<Prontuario>.Filter.Regex(
                new ExpressionFieldDefinition<Prontuario, string>(field),
                new MongoDB.Bson.BsonRegularExpression(value.Trim(), "i"));
    }

    private static SortDefinition<Prontuario> CreateSort(string sortBy, bool ascending)
    {
        var sortBuilder = Builders<Prontuario>.Sort;

        // Traduz o nome recebido na query para um campo permitido do documento MongoDB.
        return sortBy switch
        {
            "species" => ascending
                ? sortBuilder.Ascending(prontuario => prontuario.Pet.Especie)
                : sortBuilder.Descending(prontuario => prontuario.Pet.Especie),
            "breed" => ascending
                ? sortBuilder.Ascending(prontuario => prontuario.Pet.Raca)
                : sortBuilder.Descending(prontuario => prontuario.Pet.Raca),
            "sex" => ascending
                ? sortBuilder.Ascending(prontuario => prontuario.Pet.Sexo)
                : sortBuilder.Descending(prontuario => prontuario.Pet.Sexo),
            "weight" => ascending
                ? sortBuilder.Ascending(prontuario => prontuario.Pet.PesoAtualKg)
                : sortBuilder.Descending(prontuario => prontuario.Pet.PesoAtualKg),
            "birthdate" => ascending
                ? sortBuilder.Ascending(prontuario => prontuario.Pet.DataNascimento)
                : sortBuilder.Descending(prontuario => prontuario.Pet.DataNascimento),
            "id" => ascending
                ? sortBuilder.Ascending(prontuario => prontuario.Id)
                : sortBuilder.Descending(prontuario => prontuario.Id),
            _ => ascending
                ? sortBuilder.Ascending(prontuario => prontuario.Pet.Nome)
                : sortBuilder.Descending(prontuario => prontuario.Pet.Nome)
        };
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