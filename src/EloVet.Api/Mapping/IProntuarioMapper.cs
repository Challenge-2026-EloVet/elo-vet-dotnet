using EloVet.Api.Models.Prontuario;
using EloVet.Domain.Entities;

namespace EloVet.Api.Mapping;

public interface IProntuarioMapper
{
    Prontuario ToEntity(ProntuarioRequest request);

    ProntuarioResponse ToResponse(Prontuario prontuario);
}