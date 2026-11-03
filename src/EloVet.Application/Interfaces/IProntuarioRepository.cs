using EloVet.Domain.Entities;
using EloVet.Application.Models;

namespace EloVet.Application.Interfaces;

public interface IProntuarioRepository
{
    Task<Prontuario> SalvarAsync(Prontuario prontuario);

    Task<PagedResult<Prontuario>> ListarAsync(ProntuarioQuery query);

    Task<Prontuario?> FindByIdAsync(string id);

    Task<Prontuario?> FindByPetIdAsync(string petId);

    Task<Prontuario?> EditarAsync(string id, Prontuario prontuario);

    Task ExcluirAsync(string id);
}