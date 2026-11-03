using EloVet.Api.Models.Prontuario;
using EloVet.Domain.Entities;

namespace EloVet.Api.Mapping;

public class ProntuarioMapper : IProntuarioMapper
{
    public Prontuario ToEntity(ProntuarioRequest request)
    {
        return new Prontuario
        {
            Id = request.Id,
            Pet = new Pet
            {
                Id = request.Pet.Id,
                Nome = request.Pet.Nome,
                Especie = request.Pet.Especie,
                Raca = request.Pet.Raca,
                Sexo = request.Pet.Sexo,
                DataNascimento = request.Pet.DataNascimento,
                IdadeAproximada = request.Pet.IdadeAproximada,
                PesoAtualKg = request.Pet.PesoAtualKg,
                isCastrado = request.Pet.IsCastrado
            },
            Consultas = request.Consultas.Select(consulta => new Consulta
            {
                Id = consulta.Id,
                Data = consulta.Data,
                VeterinarioId = consulta.VeterinarioId,
                VeterinarioNome = consulta.VeterinarioNome,
                PlanoAcao = consulta.PlanoAcao,
                Soap = new Soap
                {
                    Subjetivo = consulta.Soap.Subjetivo,
                    Objetivo = consulta.Soap.Objetivo,
                    Avaliacao = consulta.Soap.Avaliacao,
                    Plano = consulta.Soap.Plano
                }
            }).ToList(),
            Exames = request.Exames.Select(exame => new Exame
            {
                Id = exame.Id,
                Tipo = exame.Tipo,
                Data = exame.Data,
                Resultado = exame.Resultado
            }).ToList(),
            Vacinas = request.Vacinas.Select(vacina => new Vacina
            {
                Id = vacina.Id,
                Nome = vacina.Nome,
                DataAplicacao = vacina.DataAplicacao,
                DataProximaDose = vacina.DataProximaDose
            }).ToList()
        };
    }

    public ProntuarioResponse ToResponse(Prontuario prontuario)
    {
        return new ProntuarioResponse
        {
            Id = prontuario.Id,
            Pet = new PetResponse
            {
                Id = prontuario.Pet.Id,
                Nome = prontuario.Pet.Nome,
                Especie = prontuario.Pet.Especie,
                Raca = prontuario.Pet.Raca,
                Sexo = prontuario.Pet.Sexo,
                DataNascimento = prontuario.Pet.DataNascimento,
                IdadeAproximada = prontuario.Pet.IdadeAproximada,
                PesoAtualKg = prontuario.Pet.PesoAtualKg,
                IsCastrado = prontuario.Pet.isCastrado
            },
            Consultas = prontuario.Consultas.Select(consulta => new ConsultaResponse
            {
                Id = consulta.Id,
                Data = consulta.Data,
                VeterinarioId = consulta.VeterinarioId,
                VeterinarioNome = consulta.VeterinarioNome,
                PlanoAcao = consulta.PlanoAcao,
                Soap = new SoapResponse
                {
                    Subjetivo = consulta.Soap.Subjetivo,
                    Objetivo = consulta.Soap.Objetivo,
                    Avaliacao = consulta.Soap.Avaliacao,
                    Plano = consulta.Soap.Plano
                }
            }).ToList(),
            Exames = prontuario.Exames.Select(exame => new ExameResponse
            {
                Id = exame.Id,
                Tipo = exame.Tipo,
                Data = exame.Data,
                Resultado = exame.Resultado
            }).ToList(),
            Vacinas = prontuario.Vacinas.Select(vacina => new VacinaResponse
            {
                Id = vacina.Id,
                Nome = vacina.Nome,
                DataAplicacao = vacina.DataAplicacao,
                DataProximaDose = vacina.DataProximaDose
            }).ToList()
        };
    }
}