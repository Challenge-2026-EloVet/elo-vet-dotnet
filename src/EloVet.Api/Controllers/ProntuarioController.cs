using EloVet.Application.Interfaces;
using EloVet.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EloVet.Controllers.ProntuarioController;

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

    [HttpGet("{petId}")]
    public async Task<IActionResult> FindByPetIdAsync(string petId)
    {
        var prontuario = await _prontuarioService.FindByPetIdAsync(petId);
        return Ok(prontuario);
    }
}