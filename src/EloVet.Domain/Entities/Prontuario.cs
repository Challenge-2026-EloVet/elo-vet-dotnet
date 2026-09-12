using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EloVet.Domain.Entities
{
    public class Prontuario
    {

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public Pet Pet { get; set; } = new();
        public List<Consulta> Consultas { get; set; } = [];
        public List<Exame> Exames { get; set; } = [];
        
        public List<Vacina> Vacinas { get; set; } = [];
    }
}