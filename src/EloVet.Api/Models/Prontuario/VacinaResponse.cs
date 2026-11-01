namespace EloVet.Api.Models.Prontuario;

public class VacinaResponse
{
    public string Id { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public DateTime DataAplicacao { get; set; }
    public DateTime? DataProximaDose { get; set; }
}