namespace EloVet.Domain.Entities
{
    public class Prontuario
    {
        public string Id { get; set; } = string.Empty;
        public Pet Pet { get; set; } = new();
        public List<Consulta> Consultas { get; set; } = [];
        public List<Exame> Exames { get; set; } = [];
        
        public List<Vacina> Vacinas { get; set; } = [];
    }
}