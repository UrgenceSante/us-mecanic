using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace UsMecanic.Api.IntegrationTests;

/// <summary>
/// API hébergée en mémoire sur une base PostgreSQL jetable (créée par les migrations, supprimée à la fin).
/// Serveur : variable TEST_POSTGRES_CONNECTION, sinon PostgreSQL local (postgres/postgres).
/// L'authentification Keycloak est remplacée : un client est authentifié s'il envoie l'en-tête <see cref="UserHeader"/>.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string UserHeader = "X-Test-User";

    private readonly string _connectionString;

    public ApiFactory()
    {
        var server = Environment.GetEnvironmentVariable("TEST_POSTGRES_CONNECTION")
            ?? "Host=localhost;Port=5432;Username=postgres;Password=postgres";
        _connectionString = new NpgsqlConnectionStringBuilder(server) { Database = $"us_mecanic_test_{Guid.NewGuid():N}" }.ConnectionString;
    }

    public HttpClient CreateAuthenticatedClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(UserHeader, "testeur");
        return client;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Default", _connectionString);
        builder.ConfigureTestServices(services => services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { }));
    }

    public Task InitializeAsync() => Task.CompletedTask;

    async Task IAsyncLifetime.DisposeAsync()
    {
        await DisposeAsync();
        NpgsqlConnection.ClearAllPools();
        var builder = new NpgsqlConnectionStringBuilder(_connectionString);
        var database = builder.Database;
        builder.Database = "postgres";
        await using var connection = new NpgsqlConnection(builder.ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{database}\" WITH (FORCE)", connection);
        await command.ExecuteNonQueryAsync();
    }

    private sealed class TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        public const string SchemeName = "Test";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(UserHeader, out var user))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var identity = new ClaimsIdentity([new Claim("preferred_username", user.ToString())], SchemeName);
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName)));
        }
    }
}
