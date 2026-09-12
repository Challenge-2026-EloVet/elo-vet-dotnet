using EloVet.Application.Interfaces;
using EloVet.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EloVet.Controllers.ProntuarioController;

[ApiController]
[Route("api/[controller]")]
public class ProntuarioController : ControllerBase
{
    private readonly IProntuarioService _prontuarioService;
    
    public ProntuarioController(IProntuarioService prontuarioService)
    {
        _prontuarioService = prontuarioService;
    }

    [HttpPost]
    public async Task<IActionResult> SalvarAsync(Prontuario prontuario)
    {
        await _prontuarioService.SalvarAsync(prontuario);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> ListarAsync()
    {
        var prontuarios = await _prontuarioService.ListarAsync();
        return Ok(prontuarios);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> FindByIdAsync(string id)
    {
        var prontuario = await _prontuarioService.FindByIdAsync(id);

        if (prontuario is null)
        {
            return NotFound();
        }

        return Ok(prontuario);
    }

    [HttpGet("pet/{petId}")]
    public async Task<IActionResult> FindByPetIdAsync(string petId)
    {
        var prontuario = await _prontuarioService.FindByPetIdAsync(petId);

        if (prontuario is null)
        {
            return NotFound();
        }

        return Ok(prontuario);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> EditarAsync(string id, [FromBody] Prontuario prontuario)
    {
        if (!string.IsNullOrWhiteSpace(prontuario.Id) && prontuario.Id != id)
        {
            return BadRequest("O id não corresponde ao id do prontuário no body.");
        }

        var prontuarioEditado = await _prontuarioService.EditarAsync(id, prontuario);

        if (prontuarioEditado is null)
        {
            return NotFound();
        }

        return Ok(prontuarioEditado);   
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> ExcluirAsync(string id)
    {
        var prontuario = await _prontuarioService.FindByIdAsync(id);

        if (prontuario is null)
        {
            return NotFound();
        }

        await _prontuarioService.ExcluirAsync(id);

        return NoContent();
    }
    
}