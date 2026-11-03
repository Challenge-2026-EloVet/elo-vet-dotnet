namespace EloVet.Api.Models.Prontuario;

public class ProntuarioResponse
{
    public string? Id { get; set; }
    public PetResponse Pet { get; set; } = new();
    public List<ConsultaResponse> Consultas { get; set; } = [];
    public List<ExameResponse> Exames { get; set; } = [];
    public List<VacinaResponse> Vacinas { get; set; } = [];
}