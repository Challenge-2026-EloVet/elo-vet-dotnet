using EloVet.Infrastructure.Mongo;
using EloVet.Application.Interfaces;
using EloVet.Application.Services;
using EloVet.Infrastructure.Repositories;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

//Service e Repository
builder.Services.AddScoped<IProntuarioService, ProntuarioService>();
builder.Services.AddScoped<IProntuarioRepository, ProntuarioRepository>();

//Configuração do MongoDB
builder.Services.AddMongoDb(builder.Configuration);

//Health Check
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Configura o endpoint de Liveness (Vivacidade)
// Retorna 200 OK imediatamente se a aplicação não estiver travada.
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});

// Configura o endpoint de Readiness (Prontidão)
// Só retorna 200 OK se todas as dependências com a tag "ready" (como o banco de dados) estiverem ok.
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse // Formata a saída em um JSON amigável
});

app.Run();