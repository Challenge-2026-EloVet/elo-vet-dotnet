namespace EloVet.Api.Models.Prontuario;

public class ProntuarioRequest
{
    public string? Id { get; set; }
    public PetRequest Pet { get; set; } = new();
    public List<ConsultaRequest> Consultas { get; set; } = [];
    public List<ExameRequest> Exames { get; set; } = [];
    public List<VacinaRequest> Vacinas { get; set; } = [];
}