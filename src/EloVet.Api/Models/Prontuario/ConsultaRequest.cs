namespace EloVet.Api.Models.Prontuario;

public class ConsultaRequest
{
    public string Id { get; set; } = string.Empty;
    public DateTime Data { get; set; }
    public string VeterinarioId { get; set; } = string.Empty;
    public string VeterinarioNome { get; set; } = string.Empty;
    public SoapRequest Soap { get; set; } = new();
    public string PlanoAcao { get; set; } = string.Empty;
}

public class SoapRequest
{
    public string Subjetivo { get; set; } = string.Empty;
    public string Objetivo { get; set; } = string.Empty;
    public string Avaliacao { get; set; } = string.Empty;
    public string Plano { get; set; } = string.Empty;
}