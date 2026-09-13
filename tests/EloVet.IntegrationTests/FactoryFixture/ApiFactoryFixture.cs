using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace EloVet.IntegrationTests.FactoryFixture;

public class ApiFactoryFixture : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MongoDb:DatabaseName"] = "elo-vet-test"
            });
        });
    }
}

[CollectionDefinition("ApiCollection")]
public class ApiCollection : ICollectionFixture<ApiFactoryFixture>
{
}