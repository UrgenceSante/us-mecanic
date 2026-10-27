using Microsoft.AspNetCore.Hosting;
using System.Net;

namespace UsMecanic.Api.IntegrationTests;

public class AuthDisabledTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public AuthDisabledTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("false")]
    [InlineData("n'importe quoi")]
    [InlineData("1")]
    public async Task Reglage_absent_ou_inconnu_laisse_l_authentification_active(string? value)
    {
        using var factory = _factory.WithWebHostBuilder(builder =>
        {
            if (value is not null)
            {
                builder.UseSetting("Auth:Disabled", value);
            }
        });
        using var client = factory.CreateClient();

        var response = await client.GetAsync(new Uri("/api/vehicles", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Reglage_active_ouvre_les_endpoints_sans_jeton()
    {
        using var factory = _factory.WithWebHostBuilder(builder => builder.UseSetting("Auth:Disabled", "true"));
        using var client = factory.CreateClient();

        var response = await client.GetAsync(new Uri("/api/vehicles", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
