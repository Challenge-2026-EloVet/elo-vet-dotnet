using EloVet.Application.Interfaces;
using EloVet.Domain.Entities;

namespace EloVet.Application.Services;

public class ProntuarioService : IProntuarioService
{
    private readonly IProntuarioRepository _prontuarioRepository;

    public ProntuarioService(IProntuarioRepository prontuarioRepository)
    {
        _prontuarioRepository = prontuarioRepository;
    }

    public async Task SalvarAsync(Prontuario prontuario)
    {
        await _prontuarioRepository.SalvarAsync(prontuario);
    }

    public async Task<IEnumerable<Prontuario>> ListarAsync()
    {
        return await _prontuarioRepository.ListarAsync();
    }

    public async Task<Prontuario?> FindByIdAsync(string id)
    {
        return await _prontuarioRepository.FindByIdAsync(id);
    }

    public async Task<Prontuario?> FindByPetIdAsync(string petId)
    {
        return await _prontuarioRepository.FindByPetIdAsync(petId);
    }

    public async Task<Prontuario?> EditarAsync(string id, Prontuario prontuario)
    {
        prontuario.Id = id;
        return await _prontuarioRepository.EditarAsync(id, prontuario);
    }

    public async Task ExcluirAsync(string id)
    {
        await _prontuarioRepository.ExcluirAsync(id);
    }
}