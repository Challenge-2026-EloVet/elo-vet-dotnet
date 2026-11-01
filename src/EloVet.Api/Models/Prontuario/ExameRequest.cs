namespace EloVet.Api.Models.Prontuario;

public class ExameRequest
{
    public string Id { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public DateTime Data { get; set; }
    public string Resultado { get; set; } = string.Empty;
}