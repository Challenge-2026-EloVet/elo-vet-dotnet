using EloVet.Application.Interfaces;
using EloVet.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EloVet.Application.Services;

public class ProntuarioService : IProntuarioService
{
    private readonly IProntuarioRepository _prontuarioRepository;
    private readonly ILogger<ProntuarioService> _logger;

    public ProntuarioService(IProntuarioRepository prontuarioRepository, ILogger<ProntuarioService> logger)
    {
        _prontuarioRepository = prontuarioRepository;
        _logger = logger;
    }

    public async Task SalvarAsync(Prontuario prontuario)
    {
        _logger.LogInformation("Iniciando cadastro de prontuario para o pet {PetId}", prontuario.Pet.Id);

        var existente = await _prontuarioRepository.FindByPetIdAsync(prontuario.Pet.Id);

        if (existente is not null)
        {
            _logger.LogWarning("Cadastro recusado porque o pet {PetId} ja possui prontuario", prontuario.Pet.Id);
            throw new InvalidOperationException("O pet já possui um prontuário.");
        }

        await _prontuarioRepository.SalvarAsync(prontuario);

        _logger.LogInformation("Prontuario salvo com sucesso para o pet {PetId}", prontuario.Pet.Id);
    }

    public async Task<IEnumerable<Prontuario>> ListarAsync()
    {
        _logger.LogInformation("Listando prontuarios");
        return await _prontuarioRepository.ListarAsync();
    }

    public async Task<Prontuario?> FindByIdAsync(string id)
    {
        _logger.LogInformation("Consultando prontuario por id {ProntuarioId}", id);

        var prontuario = await _prontuarioRepository.FindByIdAsync(id);

        if (prontuario is null)
        {
            _logger.LogWarning("Prontuario {ProntuarioId} nao encontrado", id);
        }

        return prontuario;
    }

    public async Task<Prontuario?> FindByPetIdAsync(string petId)
    {
        _logger.LogInformation("Consultando prontuario pelo pet {PetId}", petId);

        var prontuario = await _prontuarioRepository.FindByPetIdAsync(petId);

        if (prontuario is null)
        {
            _logger.LogWarning("Prontuario do pet {PetId} nao encontrado", petId);
        }

        return prontuario;
    }

    public async Task<Prontuario?> EditarAsync(string id, Prontuario prontuario)
    {
        _logger.LogInformation("Atualizando prontuario {ProntuarioId}", id);

        prontuario.Id = id;

        var prontuarioEditado = await _prontuarioRepository.EditarAsync(id, prontuario);

        if (prontuarioEditado is null)
        {
            _logger.LogWarning("Prontuario {ProntuarioId} nao encontrado para atualizacao", id);
        }

        return prontuarioEditado;
    }

    public async Task ExcluirAsync(string id)
    {
        _logger.LogInformation("Excluindo prontuario {ProntuarioId}", id);
        await _prontuarioRepository.ExcluirAsync(id);
    }
}