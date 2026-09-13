using EloVet.Domain.Entities;
using EloVet.IntegrationTests.FactoryFixture;
using System.Net;
using System.Net.Http.Json;

namespace EloVet.IntegrationTests.Integration;

[Collection("ApiCollection")]
public class ProntuarioControllerIntegrationTests
{
    private readonly HttpClient _client;

    public ProntuarioControllerIntegrationTests(ApiFactoryFixture factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CriarProntuario_DadosValidos_PermiteConsultarProntuarioCriado()
    {
        // Caso de uso: criar um prontuário e consultá-lo pelo Pet.

        // Arrange
        // Cria um Pet com ID único para evitar conflitos no banco de teste.
        var petId = $"pet-integracao-{Guid.NewGuid()}";

        var prontuario = new Prontuario
        {
            Pet = new Pet
            {
                Id = petId
            }
        };

        // Act
        // Cria o prontuário através da API.
        var postResponse = await _client.PostAsJsonAsync(
            "/api/Prontuario",
            prontuario);

        // Consulta o prontuário recém-criado pelo Pet.
        var getResponse = await _client.GetAsync(
            $"/api/Prontuario/pet/{petId}");

        var prontuarioConsultado =
            await getResponse.Content.ReadFromJsonAsync<Prontuario>();

        // Assert
        // A criação e a consulta devem ser realizadas com sucesso.
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        // O prontuário consultado deve ser o mesmo que foi criado.
        Assert.NotNull(prontuarioConsultado);
        Assert.Equal(
            petId,
            prontuarioConsultado.Pet.Id);
    }

    [Fact]
    public async Task CriarProntuario_PetJaPossuiProntuario_RetornaConflict()
    {
        // Caso de uso: impedir dois prontuários para o mesmo Pet.

        // Arrange
        // Cria um Pet com ID único para o teste.
        var petId = $"pet-integracao-{Guid.NewGuid()}";

        var prontuario = new Prontuario
        {
            Pet = new Pet
            {
                Id = petId
            }
        };

        // Act
        // Cria o primeiro prontuário.
        var primeiraResposta = await _client.PostAsJsonAsync(
            "/api/Prontuario",
            prontuario);

        // Tenta criar outro prontuário para o mesmo Pet.
        var segundaResposta = await _client.PostAsJsonAsync(
            "/api/Prontuario",
            prontuario);

        // Assert
        // O primeiro cadastro deve ser realizado com sucesso.
        Assert.Equal(
            HttpStatusCode.Created,
            primeiraResposta.StatusCode);

        // O segundo cadastro deve ser recusado.
        Assert.Equal(
            HttpStatusCode.Conflict,
            segundaResposta.StatusCode);
    }

    [Fact]
    public async Task ListarProntuarios_ProntuariosExistem_RetornaOk()
    {
        // Caso de uso: listar os prontuários cadastrados.

        // Arrange
        // Cria um prontuário para garantir que exista um registro no banco.
        var prontuario = new Prontuario
        {
            Pet = new Pet
            {
                Id = $"pet-integracao-{Guid.NewGuid()}"
            }
        };

        await _client.PostAsJsonAsync(
            "/api/Prontuario",
            prontuario);

        // Act
        // Consulta todos os prontuários.
        var response = await _client.GetAsync(
            "/api/Prontuario");

        var prontuarios =
            await response.Content.ReadFromJsonAsync<List<Prontuario>>();

        // Assert
        // A consulta deve retornar sucesso e uma lista.
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        Assert.NotNull(prontuarios);
        Assert.NotEmpty(prontuarios);
    }

    [Fact]
    public async Task BuscarPorId_ProntuarioExiste_RetornaOk()
    {
        // Caso de uso: consultar um prontuário pelo seu ID.

        // Arrange
        // Cria um prontuário para obter um ID real do MongoDB.
        var prontuario = new Prontuario
        {
            Pet = new Pet
            {
                Id = $"pet-integracao-{Guid.NewGuid()}"
            }
        };

        var postResponse = await _client.PostAsJsonAsync(
            "/api/Prontuario",
            prontuario);

        var prontuarioCriado =
            await postResponse.Content.ReadFromJsonAsync<Prontuario>();

        // Act
        // Consulta o prontuário utilizando o ID gerado pelo MongoDB.
        var response = await _client.GetAsync(
            $"/api/Prontuario/{prontuarioCriado!.Id}");

        var prontuarioConsultado =
            await response.Content.ReadFromJsonAsync<Prontuario>();

        // Assert
        // A consulta deve retornar o prontuário criado.
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        Assert.NotNull(prontuarioConsultado);
        Assert.Equal(
            prontuarioCriado.Id,
            prontuarioConsultado.Id);
    }

    [Fact]
    public async Task BuscarPorId_ProntuarioNaoExiste_RetornaNotFound()
    {
        // Caso de uso: consultar um prontuário inexistente.

        // Arrange
        // Utiliza um ID que não deve existir no banco de teste.
        var prontuarioId = "507f1f77bcf86cd799439011";

        // Act
        // Consulta o prontuário inexistente.
        var response = await _client.GetAsync(
            $"/api/Prontuario/{prontuarioId}");

        // Assert
        // A API deve informar que o prontuário não foi encontrado.
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task BuscarPorPetId_ProntuarioExiste_RetornaOk()
    {
        // Caso de uso: consultar o prontuário pelo ID do Pet.

        // Arrange
        // Cria um prontuário com um Pet identificável.
        var petId = $"pet-integracao-{Guid.NewGuid()}";

        var prontuario = new Prontuario
        {
            Pet = new Pet
            {
                Id = petId
            }
        };

        await _client.PostAsJsonAsync(
            "/api/Prontuario",
            prontuario);

        // Act
        // Consulta o prontuário utilizando o ID do Pet.
        var response = await _client.GetAsync(
            $"/api/Prontuario/pet/{petId}");

        var prontuarioConsultado =
            await response.Content.ReadFromJsonAsync<Prontuario>();

        // Assert
        // A API deve retornar o prontuário do Pet informado.
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        Assert.NotNull(prontuarioConsultado);
        Assert.Equal(
            petId,
            prontuarioConsultado.Pet.Id);
    }

    [Fact]
    public async Task BuscarPorPetId_ProntuarioNaoExiste_RetornaNotFound()
    {
        // Caso de uso: consultar o prontuário de um Pet sem cadastro.

        // Arrange
        // Utiliza um ID de Pet que não deve possuir prontuário.
        var petId = $"pet-inexistente-{Guid.NewGuid()}";

        // Act
        // Consulta o prontuário pelo Pet.
        var response = await _client.GetAsync(
            $"/api/Prontuario/pet/{petId}");

        // Assert
        // A API deve informar que o prontuário não foi encontrado.
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task EditarProntuario_DadosValidos_PermiteConsultarDadosAtualizados()
    {
        // Caso de uso: atualizar um prontuário e confirmar a alteração.

        // Arrange
        // Cria um prontuário que será atualizado.
        var petId = $"pet-integracao-{Guid.NewGuid()}";

        var prontuario = new Prontuario
        {
            Pet = new Pet
            {
                Id = petId,
                Nome = "Pet Inicial"
            }
        };

        var postResponse = await _client.PostAsJsonAsync(
            "/api/Prontuario",
            prontuario);

        var prontuarioCriado =
            await postResponse.Content.ReadFromJsonAsync<Prontuario>();

        // Cria uma nova versão do prontuário com os dados alterados.
        prontuarioCriado!.Pet.Nome = "Pet Atualizado";

        // Act
        // Atualiza o prontuário através da API.
        var putResponse = await _client.PutAsJsonAsync(
            $"/api/Prontuario/{prontuarioCriado.Id}",
            prontuarioCriado);

        // Consulta novamente para confirmar a alteração.
        var getResponse = await _client.GetAsync(
            $"/api/Prontuario/{prontuarioCriado.Id}");

        var prontuarioAtualizado =
            await getResponse.Content.ReadFromJsonAsync<Prontuario>();

        // Assert
        // A atualização e a consulta devem ser realizadas com sucesso.
        Assert.Equal(
            HttpStatusCode.OK,
            putResponse.StatusCode);

        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode);

        // O dado alterado deve estar persistido no banco.
        Assert.NotNull(prontuarioAtualizado);
        Assert.Equal(
            "Pet Atualizado",
            prontuarioAtualizado.Pet.Nome);
    }

    [Fact]
    public async Task EditarProntuario_IdDaRotaDiferenteDoBody_RetornaBadRequest()
    {
        // Caso de uso: impedir atualização quando os IDs são diferentes.

        // Arrange
        // Utiliza IDs diferentes entre a rota e o corpo da requisição.
        var idDaRota = "id-rota";
        var idDoBody = "id-body";

        var prontuario = new Prontuario
        {
            Id = idDoBody,
            Pet = new Pet
            {
                Id = $"pet-integracao-{Guid.NewGuid()}"
            }
        };

        // Act
        // Tenta atualizar utilizando IDs diferentes.
        var response = await _client.PutAsJsonAsync(
            $"/api/Prontuario/{idDaRota}",
            prontuario);

        // Assert
        // A API deve rejeitar a requisição.
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task EditarProntuario_ProntuarioNaoExiste_RetornaNotFound()
    {
        // Caso de uso: tentar atualizar um prontuário inexistente.

        // Arrange
        // Utiliza um ID que não deve existir no banco.
        var prontuarioId = "507f1f77bcf86cd799439011";

        var prontuario = new Prontuario
        {
            Pet = new Pet
            {
                Id = $"pet-integracao-{Guid.NewGuid()}"
            }
        };

        // Act
        // Tenta atualizar o prontuário inexistente.
        var response = await _client.PutAsJsonAsync(
            $"/api/Prontuario/{prontuarioId}",
            prontuario);

        // Assert
        // A API deve informar que o prontuário não foi encontrado.
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task ExcluirProntuario_ProntuarioExiste_PermiteConfirmarExclusao()
    {
        // Caso de uso: excluir um prontuário e confirmar sua remoção.

        // Arrange
        // Cria um prontuário que será excluído.
        var prontuario = new Prontuario
        {
            Pet = new Pet
            {
                Id = $"pet-integracao-{Guid.NewGuid()}"
            }
        };

        var postResponse = await _client.PostAsJsonAsync(
            "/api/Prontuario",
            prontuario);

        var prontuarioCriado =
            await postResponse.Content.ReadFromJsonAsync<Prontuario>();

        // Act
        // Exclui o prontuário através da API.
        var deleteResponse = await _client.DeleteAsync(
            $"/api/Prontuario/{prontuarioCriado!.Id}");

        // Consulta novamente para confirmar a exclusão.
        var getResponse = await _client.GetAsync(
            $"/api/Prontuario/{prontuarioCriado.Id}");

        // Assert
        // A exclusão deve ser concluída sem conteúdo na resposta.
        Assert.Equal(
            HttpStatusCode.NoContent,
            deleteResponse.StatusCode);

        // O prontuário não deve mais ser encontrado.
        Assert.Equal(
            HttpStatusCode.NotFound,
            getResponse.StatusCode);
    }

    [Fact]
    public async Task ExcluirProntuario_ProntuarioNaoExiste_RetornaNotFound()
    {
        // Caso de uso: tentar excluir um prontuário inexistente.

        // Arrange
        // Utiliza um ID que não deve existir no banco.
        var prontuarioId = "507f1f77bcf86cd799439011";

        // Act
        // Tenta excluir o prontuário inexistente.
        var response = await _client.DeleteAsync(
            $"/api/Prontuario/{prontuarioId}");

        // Assert
        // A API deve informar que o prontuário não foi encontrado.
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }
}