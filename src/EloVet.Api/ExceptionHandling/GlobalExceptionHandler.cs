using EloVet.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace EloVet.Api.ExceptionHandling;

/// <summary>
/// Tratamento global de exceções da API.
/// Toda exceção não tratada chega aqui e vira uma resposta ProblemDetails (RFC 9457,
/// application/problem+json) com status, título, detalhe e traceId para correlação com logs e traces.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    public const string MensagemErroInesperado =
        "Ocorreu um erro inesperado. Informe o traceId ao suporte para investigação.";

    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var erro = Mapear(exception);
        var request = httpContext.Request;

        if (erro.StatusCode >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Erro ao processar {Method} {Path}: resposta {StatusCode}",
                request.Method,
                request.Path,
                erro.StatusCode);
        }
        else
        {
            _logger.LogWarning(
                "Requisição {Method} {Path} recusada com {StatusCode}: {Motivo}",
                request.Method,
                request.Path,
                erro.StatusCode,
                exception.Message);
        }

        var problemDetails = new ProblemDetails
        {
            Status = erro.StatusCode,
            Title = erro.Titulo,
            Detail = erro.Detalhe
        };

        httpContext.Response.StatusCode = erro.StatusCode;

        var escrito = await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });

        if (!escrito)
        {
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        }

        return true;
    }

    private static ErroMapeado Mapear(Exception exception) => exception switch
    {
        InvalidInputException => new(
            StatusCodes.Status400BadRequest,
            "Requisição inválida",
            exception.Message),
        ConflictException => new(
            StatusCodes.Status409Conflict,
            "Conflito",
            exception.Message),
        BadHttpRequestException badRequest => new(
            badRequest.StatusCode,
            "Requisição inválida",
            "Não foi possível ler a requisição enviada."),
        MongoException or TimeoutException => new(
            StatusCodes.Status503ServiceUnavailable,
            "Serviço indisponível",
            "O banco de dados está indisponível no momento. Tente novamente em instantes."),
        _ => new(
            StatusCodes.Status500InternalServerError,
            "Erro interno",
            MensagemErroInesperado)
    };

    private sealed record ErroMapeado(int StatusCode, string Titulo, string Detalhe);
}
