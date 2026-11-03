using EloVet.Api.Models.Prontuario;
using EloVet.IntegrationTests.FactoryFixture;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;

namespace EloVet.IntegrationTests.Integration;

[Collection("ApiCollection")]
public class ExceptionHandlingIntegrationTests : IAsyncLifetime
{
    private const string ProblemJson = "application/problem+json";

    private readonly ApiFactoryFixture _factory;
    private HttpClient _client = null!;

    public ExceptionHandlingIntegrationTests(ApiFactoryFixture factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        _client = await _factory.CreateAuthenticatedClientAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CriarProntuario_PetJaPossuiProntuario_RetornaProblemDetailsComStatus409()
    {
        // Arrange
        var request = new ProntuarioRequest
        {
            Pet = new PetRequest { Id = $"pet-integracao-{Guid.NewGuid()}" }
        };
        await _client.PostAsJsonAsync("/api/Prontuario", request);

        // Act
        var response = await _client.PostAsJsonAsync("/api/Prontuario", request);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(ProblemJson, response.Content.Headers.ContentType?.MediaType);
        Assert.NotNull(problem);
        Assert.Equal(409, problem.Status);
        Assert.Equal("O pet já possui um prontuário.", problem.Detail);
        Assert.Equal("POST /api/Prontuario", problem.Instance);
        Assert.True(problem.Extensions.ContainsKey("traceId"));
    }

    [Fact]
    public async Task BuscarPorId_IdComFormatoInvalido_RetornaProblemDetailsComStatus400()
    {
        // Arrange
        var idInvalido = "id-invalido";

        // Act
        var response = await _client.GetAsync($"/api/Prontuario/{idInvalido}");
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(ProblemJson, response.Content.Headers.ContentType?.MediaType);
        Assert.NotNull(problem);
        Assert.Equal(
            "O id do prontuário informado não possui um formato válido.",
            problem.Detail);
    }

    [Fact]
    public async Task BuscarPorId_ProntuarioNaoExiste_RetornaProblemDetailsComStatus404()
    {
        // Arrange
        var idInexistente = "507f1f77bcf86cd799439011";

        // Act
        var response = await _client.GetAsync($"/api/Prontuario/{idInexistente}");
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(ProblemJson, response.Content.Headers.ContentType?.MediaType);
        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
    }

    [Fact]
    public async Task ListarProntuarios_SemToken_RetornaProblemDetailsComStatus401()
    {
        // Arrange
        var clienteSemToken = _factory.CreateClient();

        // Act
        var response = await clienteSemToken.GetAsync("/api/Prontuario");
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(ProblemJson, response.Content.Headers.ContentType?.MediaType);
        Assert.NotNull(problem);
        Assert.Equal(401, problem.Status);
    }
}
