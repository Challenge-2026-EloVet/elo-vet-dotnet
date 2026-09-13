using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace EloVet.Infrastructure.Mongo;

public static class MongoDependencyInjection
{
    public static IServiceCollection AddMongoDb (this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetSection("MongoDb:ConnectionString").Value ?? throw new InvalidOperationException("Connection String não configurada.");

        services.AddSingleton<IMongoClient>(
            new MongoClient(connectionString));

        services.AddSingleton<IMongoDatabase>(serviceProvider =>
        {
            var mongoClient = serviceProvider.GetRequiredService<IMongoClient>();

            var databaseName = configuration
                .GetSection("MongoDb:DatabaseName")
                .Value
                ?? "elo-vet";

            return mongoClient.GetDatabase(databaseName);
        });

        services.AddHealthChecks()
            .AddCheck<MongoDbHealthCheck>(
                "mongodb",
                tags: new[] { "ready" });

        return services;
    }
}