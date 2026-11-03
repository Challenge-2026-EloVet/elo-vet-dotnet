using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace EloVet.IntegrationTests.FactoryFixture;

public class ApiFactoryFixture : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Jwt:Key", "integration-test-key-change-me-32-characters");
        builder.UseSetting("Jwt:Issuer", "EloVet.Api");
        builder.UseSetting("Jwt:Audience", "EloVet.Client");

        builder.ConfigureAppConfiguration((context, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MongoDb:DatabaseName"] = "elo-vet-test",
            });
        });
    }

    public async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = CreateClient();
        var username = $"integration-{Guid.NewGuid():N}";
        var credentials = new { username, password = "Password123!" };

        var registerResponse = await client.PostAsJsonAsync(
            "/api/Auth/register",
            credentials);
        registerResponse.EnsureSuccessStatusCode();

        var signInResponse = await client.PostAsJsonAsync(
            "/api/Auth/signin",
            credentials);
        signInResponse.EnsureSuccessStatusCode();

        var authentication = await signInResponse.Content
            .ReadFromJsonAsync<AuthenticationResponse>();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authentication!.Token);

        return client;
    }

    private sealed record AuthenticationResponse(string Token);
}

[CollectionDefinition("ApiCollection")]
public class ApiCollection : ICollectionFixture<ApiFactoryFixture>
{
}