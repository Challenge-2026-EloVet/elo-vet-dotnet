using System.Text;
using System.Text.Json;
using EloVet.Api.ExceptionHandling;
using EloVet.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;

namespace EloVet.UnitTests.ExceptionHandling;

public class GlobalExceptionHandlerTests
{
    private readonly Mock<ILogger<GlobalExceptionHandler>> _logger = new();
    private readonly ServiceProvider _services;
    private readonly GlobalExceptionHandler _handler;

    public GlobalExceptionHandlerTests()
    {
        // Usa o IProblemDetailsService real, o mesmo registrado por AddProblemDetails() na API.
        _services = new ServiceCollection()
            .AddLogging()
            .AddProblemDetails()
            .BuildServiceProvider();

        _handler = new GlobalExceptionHandler(
            _services.GetRequiredService<IProblemDetailsService>(),
            _logger.Object);
    }

    public static TheoryData<Exception, int> ExcecoesMapeadas => new()
    {
        { new InvalidInputException("O id do prontuário informado não possui um formato válido."), StatusCodes.Status400BadRequest },
        { new ConflictException("O pet já possui um prontuário."), StatusCodes.Status409Conflict },
        { new MongoClientException("Servidor MongoDB inacessível."), StatusCodes.Status503ServiceUnavailable },
        { new TimeoutException("Tempo de seleção do servidor esgotado."), StatusCodes.Status503ServiceUnavailable },
        { new InvalidOperationException("Falha inesperada."), StatusCodes.Status500InternalServerError }
    };

    [Theory]
    [MemberData(nameof(ExcecoesMapeadas))]
    public async Task TryHandleAsync_ExcecaoLancada_RetornaProblemDetailsComStatusEsperado(
        Exception excecao,
        int statusEsperado)
    {
        // Arrange
        var httpContext = CriarHttpContext();

        // Act
        var tratada = await _handler.TryHandleAsync(httpContext, excecao, CancellationToken.None);
        var corpo = await LerJsonAsync(httpContext);

        // Assert
        Assert.True(tratada);
        Assert.Equal(statusEsperado, httpContext.Response.StatusCode);
        Assert.StartsWith("application/problem+json", httpContext.Response.ContentType);
        Assert.Equal(statusEsperado, corpo.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task TryHandleAsync_ConflictException_UsaMensagemDaRegraNoDetail()
    {
        // Arrange
        var httpContext = CriarHttpContext();
        var excecao = new ConflictException("O pet já possui um prontuário.");

        // Act
        await _handler.TryHandleAsync(httpContext, excecao, CancellationToken.None);
        var corpo = await LerJsonAsync(httpContext);

        // Assert
        Assert.Equal("Conflito", corpo.GetProperty("title").GetString());
        Assert.Equal("O pet já possui um prontuário.", corpo.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task TryHandleAsync_ExcecaoInesperada_NaoExpoeMensagemInterna()
    {
        // Arrange
        var httpContext = CriarHttpContext();
        var excecao = new InvalidOperationException("mongodb://admin:segredo@db:27017 recusou a conexão");

        // Act
        await _handler.TryHandleAsync(httpContext, excecao, CancellationToken.None);
        var corpo = await LerTextoAsync(httpContext);

        // Assert
        Assert.DoesNotContain("segredo", corpo);
        Assert.Contains(GlobalExceptionHandler.MensagemErroInesperado, corpo);
    }

    [Fact]
    public async Task TryHandleAsync_QualquerExcecao_IncluiTraceIdParaCorrelacaoComLogs()
    {
        // Arrange
        var httpContext = CriarHttpContext();

        // Act
        await _handler.TryHandleAsync(httpContext, new InvalidOperationException("Falha."), CancellationToken.None);
        var corpo = await LerJsonAsync(httpContext);

        // Assert
        Assert.True(corpo.TryGetProperty("traceId", out var traceId));
        Assert.False(string.IsNullOrWhiteSpace(traceId.GetString()));
    }

    [Fact]
    public async Task TryHandleAsync_ExcecaoInesperada_RegistraLogDeErroComExcecao()
    {
        // Arrange
        var httpContext = CriarHttpContext();
        var excecao = new InvalidOperationException("Falha inesperada.");

        // Act
        await _handler.TryHandleAsync(httpContext, excecao, CancellationToken.None);

        // Assert
        _logger.Verify(logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                excecao,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task TryHandleAsync_ExcecaoDeNegocio_RegistraAvisoSemLogDeErro()
    {
        // Arrange
        var httpContext = CriarHttpContext();

        // Act
        await _handler.TryHandleAsync(
            httpContext,
            new ConflictException("O pet já possui um prontuário."),
            CancellationToken.None);

        // Assert
        _logger.Verify(logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        _logger.Verify(logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    private DefaultHttpContext CriarHttpContext()
    {
        var httpContext = new DefaultHttpContext { RequestServices = _services };
        httpContext.Request.Method = HttpMethods.Post;
        httpContext.Request.Path = "/api/Prontuario";
        httpContext.Response.Body = new MemoryStream();
        return httpContext;
    }

    private static async Task<string> LerTextoAsync(HttpContext httpContext)
    {
        httpContext.Response.Body.Position = 0;
        using var reader = new StreamReader(httpContext.Response.Body, Encoding.UTF8, leaveOpen: true);
        return await reader.ReadToEndAsync();
    }

    private static async Task<JsonElement> LerJsonAsync(HttpContext httpContext)
    {
        using var documento = JsonDocument.Parse(await LerTextoAsync(httpContext));
        return documento.RootElement.Clone();
    }
}
