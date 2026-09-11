namespace EloVet.Domain.Entities;

public class Pet
{
    public string Id { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Especie { get; set; } = string.Empty;
    public string Raca { get; set; } = string.Empty;
    public string Sexo { get; set; } = string.Empty;
    public DateTime DataNascimento { get; set; }
    public int IdadeAproximada { get; set; }
    public decimal PesoAtualKg { get; set; }

    public bool isCastrado { get; set; } = false;

}