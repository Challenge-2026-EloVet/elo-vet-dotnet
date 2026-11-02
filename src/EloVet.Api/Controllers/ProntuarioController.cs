using EloVet.Application.Interfaces;
using EloVet.Application.Models;
using EloVet.Api.Models;
using EloVet.Api.Mapping;
using EloVet.Domain.Entities;
using EloVet.Api.Models.Prontuario;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace EloVet.Controllers.ProntuarioController;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProntuarioController : ControllerBase
{
    private readonly IProntuarioService _prontuarioService;
    private readonly IProntuarioMapper _prontuarioMapper;
    private readonly ILogger<ProntuarioController> _logger;

    public ProntuarioController(
        IProntuarioService prontuarioService,
        IProntuarioMapper prontuarioMapper,
        ILogger<ProntuarioController> logger)
    {
        _prontuarioService = prontuarioService;
        _prontuarioMapper = prontuarioMapper;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> SalvarAsync(ProntuarioRequest request)
    {
        _logger.LogInformation(
            "Recebida solicitação para cadastrar prontuário do pet {PetId}",
            request.Pet.Id);

        var prontuario = _prontuarioMapper.ToEntity(request);

        try
        {
            var prontuarioSalvo =
                await _prontuarioService.SalvarAsync(prontuario);

            _logger.LogInformation(
                "Cadastro de prontuário concluido para o pet {PetId}",
                prontuario.Pet.Id);

            return CreatedAtRoute(
                "FindProntuarioById",
                new { id = prontuarioSalvo.Id },
                CreateResource(prontuarioSalvo));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(exception.Message);
        }
    }

    [HttpGet(Name = "ListProntuarios")]
    public async Task<IActionResult> ListarAsync([FromQuery] ProntuarioQuery? query = null)
    {
        _logger.LogInformation(
            "Recebida solicitação para listar prontuarios");

        var normalizedQuery = query ?? new ProntuarioQuery();
        var page = await _prontuarioService.ListarAsync(normalizedQuery);
        var items = page.Items.Select(CreateResource).ToList();
        var links = CreatePaginationLinks(normalizedQuery, page);

        return Ok(new PagedResponse<HateoasResource<ProntuarioResponse>>(
            items,
            page.PageNumber,
            page.PageSize,
            page.TotalPages,
            page.TotalRecords,
            links));
    }

    [HttpGet("{id}", Name = "FindProntuarioById")]
    public async Task<IActionResult> FindByIdAsync(string id)
    {
        _logger.LogInformation(
            "Recebida solicitação para consultar prontuario {ProntuarioId}",
            id);

        if (!ObjectId.TryParse(id, out _))
        {
            _logger.LogWarning(
                "Consulta de prontuario recusada porque o id {ProntuarioId} é inválido",
                id);

            return BadRequest("O id do prontuário informado não possui um formato válido.");
        }

        var prontuario = await _prontuarioService.FindByIdAsync(id);

        if (prontuario is null)
        {
            return NotFound();
        }

        return Ok(CreateResource(prontuario));
    }

    [HttpGet("pet/{petId}", Name = "FindProntuarioByPetId")]
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

        return Ok(CreateResource(prontuario));
    }

    [HttpPut("{id}", Name = "EditProntuario")]
    public async Task<IActionResult> EditarAsync(
        string id,
        [FromBody] ProntuarioRequest request)
    {
        _logger.LogInformation(
            "Recebida solicitação para atualizar prontuario {ProntuarioId}",
            id);

        if (!ObjectId.TryParse(id, out _))
        {
            _logger.LogWarning(
                "Atualização recusada porque o id {ProntuarioId} é inválido",
                id);

            return BadRequest(
                "O id do prontuário informado não possui um formato válido.");
        }

        if (!string.IsNullOrWhiteSpace(request.Id) && request.Id != id)
        {
            _logger.LogWarning(
                "Atualização recusada porque o id da rota {ProntuarioId} nao corresponde ao id informado no body {BodyProntuarioId}",
                id,
                request.Id);

            return BadRequest("O id não corresponde ao id do prontuário no body.");
        }

        var prontuario = _prontuarioMapper.ToEntity(request);
        var prontuarioEditado =
            await _prontuarioService.EditarAsync(id, prontuario);

        if (prontuarioEditado is null)
        {
            _logger.LogWarning(
                "Prontuario {ProntuarioId} nao encontrado para atualizacao",
                id);

            return NotFound();
        }

        return Ok(CreateResource(prontuarioEditado));
    }

    [HttpDelete("{id}", Name = "DeleteProntuario")]
    public async Task<IActionResult> ExcluirAsync(string id)
    {
        _logger.LogInformation(
            "Recebida solicitação para excluir prontuario {ProntuarioId}",
            id);

        if (!ObjectId.TryParse(id, out _))
        {
            _logger.LogWarning(
                "Exclusão recusada porque o id {ProntuarioId} é inválido",
                id);

            return BadRequest(
                "O id do prontuário informado não possui um formato válido.");
        }

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

    private HateoasResource<ProntuarioResponse> CreateResource(Prontuario prontuario)
    {
        var id = prontuario.Id!;
        var links = new List<LinkDto>
        {
            new(CreateLink("FindProntuarioById", new { id }, $"/api/Prontuario/{id}"), "self", "GET"),
            new(CreateLink("EditProntuario", new { id }, $"/api/Prontuario/{id}"), "update", "PUT"),
            new(CreateLink("DeleteProntuario", new { id }, $"/api/Prontuario/{id}"), "delete", "DELETE")
        };

        return new HateoasResource<ProntuarioResponse>(_prontuarioMapper.ToResponse(prontuario), links);
    }

    private IReadOnlyCollection<LinkDto> CreatePaginationLinks(
        ProntuarioQuery query,
        PagedResult<Prontuario> page)
    {
        var links = new List<LinkDto>
        {
            new(CreateLink("ListProntuarios", CreateQueryRouteValues(query, page.PageNumber), "/api/Prontuario"), "self", "GET")
        };

        if (page.PageNumber < page.TotalPages)
        {
            links.Add(new(
                CreateLink("ListProntuarios", CreateQueryRouteValues(query, page.PageNumber + 1), "/api/Prontuario"),
                "next",
                "GET"));
        }

        if (page.PageNumber > 1)
        {
            links.Add(new(
                CreateLink("ListProntuarios", CreateQueryRouteValues(query, page.PageNumber - 1), "/api/Prontuario"),
                "previous",
                "GET"));
        }

        return links;
    }

    private string CreateLink(string routeName, object routeValues, string fallback)
    {
        return Url?.Link(routeName, routeValues) ?? fallback;
    }

    private static object CreateQueryRouteValues(ProntuarioQuery query, int pageNumber)
    {
        return new
        {
            pageNumber,
            pageSize = query.PageSize,
            query.PetId,
            query.PetName,
            query.Species,
            query.Breed,
            query.Sex,
            query.IsCastrado,
            sortBy = query.SortBy,
            ascending = query.Ascending
        };
    }

}