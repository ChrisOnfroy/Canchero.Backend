using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Infrastructure.Identity;

namespace Canchero.IntegrationTest;

public sealed class ApiIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("canchero_test")
        .WithUsername("postgres")
        .WithPassword("test_password_123")
        .Build();
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = _postgres.GetConnectionString(),
                    ["Jwt:SecretKey"] = "integration-test-secret-key-at-least-32-chars",
                    ["Database:Migrate"] = "false",
                    ["Swagger:Enabled"] = "false"
                }));
        });
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();
            var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            foreach (var role in new[] { "Customer", "VenueOwner", "Admin" })
                if (!await roles.RoleExistsAsync(role))
                    await roles.CreateAsync(new IdentityRole<Guid>(role));
        }
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Health_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health");
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task ProtectedEndpoint_RejectsAnonymousRequest()
    {
        var response = await _client.GetAsync("/api/v1/venues");
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RegisterThenLogin_PersistsIdentityAndReturnsToken()
    {
        const string password = "TestPassword123";
        var email = $"integration-{Guid.NewGuid():N}@example.test";
        var registration = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            firstName = "Integration",
            lastName = "Test",
            email,
            phoneNumber = (string?)null,
            password,
            role = "Customer"
        });
        Assert.Equal(System.Net.HttpStatusCode.NoContent, registration.StatusCode);

        var duplicate = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            firstName = "Integration",
            lastName = "Test",
            email,
            phoneNumber = (string?)null,
            password,
            role = "Customer"
        });
        Assert.Equal(System.Net.HttpStatusCode.Conflict, duplicate.StatusCode);

        var login = await _client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email,
            password
        });
        Assert.Equal(System.Net.HttpStatusCode.OK, login.StatusCode);
        Assert.Contains("token", await login.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Register_RejectsAdminRole()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            firstName = "Fake",
            lastName = "Admin",
            email = $"admin-{Guid.NewGuid():N}@example.test",
            phoneNumber = (string?)null,
            password = "TestPassword123",
            role = "Admin"
        });
        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        if (_factory is not null) await _factory.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}



