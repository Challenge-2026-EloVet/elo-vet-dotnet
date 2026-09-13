using EloVet.Domain.Entities;

namespace EloVet.Application.Interfaces;

public interface IProntuarioService
{
    Task<Prontuario> SalvarAsync(Prontuario prontuario);

    Task<IEnumerable<Prontuario>> ListarAsync();

    Task<Prontuario?> FindByIdAsync(string id);

    Task<Prontuario?> FindByPetIdAsync(string petId);

    Task<Prontuario?> EditarAsync(string id, Prontuario prontuario);

    Task ExcluirAsync(string id);
}