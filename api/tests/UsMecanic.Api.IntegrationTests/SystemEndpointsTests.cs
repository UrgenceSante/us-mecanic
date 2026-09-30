using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace UsMecanic.Api.IntegrationTests;

public class SystemEndpointsTests : IClassFixture<SystemEndpointsTests.ApiFactory>
{
    private readonly HttpClient _client;

    public SystemEndpointsTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_est_accessible_sans_authentification()
    {
        var response = await _client.GetAsync(new Uri("/health", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Info_retourne_la_version_et_l_environnement()
    {
        var info = await _client.GetFromJsonAsync<AppInfoDto>(new Uri("/api/info", UriKind.Relative));

        Assert.NotNull(info);
        Assert.Equal("us-mecanic-api", info.Name);
        Assert.Equal("Testing", info.Environment);
        Assert.False(string.IsNullOrWhiteSpace(info.Version));
    }

    [Fact]
    public async Task Les_endpoints_sont_proteges_par_defaut()
    {
        var response = await _client.GetAsync(new Uri("/api/me", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private sealed record AppInfoDto(string Name, string Version, string Environment);

    public sealed class ApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseEnvironment("Testing");
    }
}
