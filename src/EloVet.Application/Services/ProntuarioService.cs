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

    public async Task<Prontuario> FindByPetIdAsync(string petId)
    {
        var prontuario = await _prontuarioRepository.FindByPetIdAsync(petId);

        return prontuario ?? throw new InvalidOperationException("Prontuário não encontrado.");
    }
}