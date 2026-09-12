using EloVet.Application.Interfaces;
using EloVet.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EloVet.Controllers.ProntuarioController;

[ApiController]
[Route("api/[controller]")]
public class ProntuarioController : ControllerBase
{
    private readonly IProntuarioService _prontuarioService;
    private readonly ILogger<ProntuarioController> _logger;

    public ProntuarioController(
        IProntuarioService prontuarioService,
        ILogger<ProntuarioController> logger)
    {
        _prontuarioService = prontuarioService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> SalvarAsync(Prontuario prontuario)
    {
        _logger.LogInformation(
            "Recebida solicitação para cadastrar prontuário do pet {PetId}",
            prontuario.Pet.Id);

        try
        {
            await _prontuarioService.SalvarAsync(prontuario);
        }
        catch (InvalidOperationException exception)
        {
            _logger.LogWarning(
                exception,
                "Cadastro de prontuário recusado para o pet {PetId}",
                prontuario.Pet.Id);

            return Conflict(exception.Message);
        }

        _logger.LogInformation(
            "Cadastro de prontuário concluido para o pet {PetId}",
            prontuario.Pet.Id);

        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> ListarAsync()
    {
        _logger.LogInformation(
            "Recebida solicitação para listar prontuarios");

        var prontuarios = await _prontuarioService.ListarAsync();

        return Ok(prontuarios);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> FindByIdAsync(string id)
    {
        _logger.LogInformation(
            "Recebida solicitação para consultar prontuario {ProntuarioId}",
            id);

        var prontuario = await _prontuarioService.FindByIdAsync(id);

        if (prontuario is null)
        {
            _logger.LogWarning(
                "Prontuario {ProntuarioId} nao encontrado",
                id);

            return NotFound();
        }

        return Ok(prontuario);
    }

    [HttpGet("pet/{petId}")]
    public async Task<IActionResult> FindByPetIdAsync(string petId)
    {
        _logger.LogInformation(
            "Recebida solicitação para consultar prontuario do pet {PetId}",
            petId);

        var prontuario = await _prontuarioService.FindByPetIdAsync(petId);

        if (prontuario is null)
        {
            _logger.LogWarning(
                "Prontuario do pet {PetId} nao encontrado",
                petId);

            return NotFound();
        }

        return Ok(prontuario);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> EditarAsync(
        string id,
        [FromBody] Prontuario prontuario)
    {
        _logger.LogInformation(
            "Recebida solicitação para atualizar prontuario {ProntuarioId}",
            id);

        if (!string.IsNullOrWhiteSpace(prontuario.Id) && prontuario.Id != id)
        {
            _logger.LogWarning(
                "Atualização recusada porque o id da rota {ProntuarioId} nao corresponde ao id informado no body {BodyProntuarioId}",
                id,
                prontuario.Id);

            return BadRequest("O id não corresponde ao id do prontuário no body.");
        }

        var prontuarioEditado =
            await _prontuarioService.EditarAsync(id, prontuario);

        if (prontuarioEditado is null)
        {
            _logger.LogWarning(
                "Prontuario {ProntuarioId} nao encontrado para atualizacao",
                id);

            return NotFound();
        }

        return Ok(prontuarioEditado);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> ExcluirAsync(string id)
    {
        _logger.LogInformation(
            "Recebida solicitação para excluir prontuario {ProntuarioId}",
            id);

        var prontuario = await _prontuarioService.FindByIdAsync(id);

        if (prontuario is null)
        {
            _logger.LogWarning(
                "Prontuario {ProntuarioId} nao encontrado para exclusao",
                id);

            return NotFound();
        }

        await _prontuarioService.ExcluirAsync(id);

        _logger.LogInformation(
            "Exclusao do prontuario {ProntuarioId} concluida",
            id);

        return NoContent();
    }
}