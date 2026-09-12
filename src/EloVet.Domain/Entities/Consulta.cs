namespace EloVet.Domain.Entities;

public class Consulta
{
    public string Id { get; set; } = string.Empty;
    public DateTime Data { get; set; }
    public string VeterinarioId { get; set; } = string.Empty;
    public string VeterinarioNome { get; set; } = string.Empty;
    public Soap Soap { get; set; } = new();
    public string PlanoAcao { get; set; } = string.Empty;
}