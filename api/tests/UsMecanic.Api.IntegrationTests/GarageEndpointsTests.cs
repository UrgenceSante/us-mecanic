using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace UsMecanic.Api.IntegrationTests;

public class GarageEndpointsTests : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly Uri Garages = new("/api/garages", UriKind.Relative);

    private readonly HttpClient _client;
    private readonly HttpClient _anonymous;

    public GarageEndpointsTests(ApiFactory factory)
    {
        _client = factory.CreateAuthenticatedClient();
        _anonymous = factory.CreateClient();
    }

    private sealed record GarageDto(Guid Id, string Name, bool IsActive);

    /// <summary>Nom unique : les tests d'une classe partagent la même base.</summary>
    private static string NewName(string prefix) => $"{prefix} {Guid.NewGuid():N}";

    private async Task<GarageDto> CreateGarageAsync(string name)
    {
        var response = await _client.PostAsJsonAsync(Garages, new { name });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<GarageDto>(Json))!;
    }

    private static async Task<string?> ErrorCodeAsync(HttpResponseMessage response)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return problem.RootElement.GetProperty("code").GetString();
    }

    [Fact]
    public async Task Les_garages_exigent_une_authentification()
    {
        var response = await _anonymous.GetAsync(Garages);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Creation_puis_liste_et_renommage()
    {
        var created = await CreateGarageAsync($"  {NewName("Garage Martin")} ");
        Assert.True(created.IsActive);
        Assert.StartsWith("Garage Martin ", created.Name, StringComparison.Ordinal);

        var list = await _client.GetFromJsonAsync<List<GarageDto>>(Garages, Json);
        Assert.Contains(list!, g => g.Id == created.Id);

        var newName = NewName("Garage Durand");
        var renamed = await _client.PutAsJsonAsync(new Uri($"/api/garages/{created.Id}", UriKind.Relative), new { name = newName });
        Assert.Equal(HttpStatusCode.OK, renamed.StatusCode);
        Assert.Equal(newName, (await renamed.Content.ReadFromJsonAsync<GarageDto>(Json))!.Name);
    }

    [Fact]
    public async Task Un_nom_vide_ou_en_double_renvoie_un_ProblemDetails()
    {
        var name = NewName("Garage Vidal");
        await CreateGarageAsync(name);

        var empty = await _client.PostAsJsonAsync(Garages, new { name = " " });
        var duplicate = await _client.PostAsJsonAsync(Garages, new { name = name.ToUpperInvariant() });

        Assert.Equal(HttpStatusCode.BadRequest, empty.StatusCode);
        Assert.Equal("garage.name_required", await ErrorCodeAsync(empty));
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.Equal("garage.name_already_exists", await ErrorCodeAsync(duplicate));
    }

    [Fact]
    public async Task Un_nom_trop_long_renvoie_un_ProblemDetails()
    {
        var response = await _client.PostAsJsonAsync(Garages, new { name = new string('x', 101) });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("garage.name_too_long", await ErrorCodeAsync(response));
    }

    [Fact]
    public async Task Renommer_vers_un_nom_existant_ou_un_garage_inconnu_est_refuse()
    {
        var other = await CreateGarageAsync(NewName("Autre"));
        var garage = await CreateGarageAsync(NewName("Garage"));

        var duplicate = await _client.PutAsJsonAsync(new Uri($"/api/garages/{garage.Id}", UriKind.Relative), new { name = other.Name });
        var unknown = await _client.PutAsJsonAsync(new Uri($"/api/garages/{Guid.NewGuid()}", UriKind.Relative), new { name = NewName("X") });

        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.Equal("garage.name_already_exists", await ErrorCodeAsync(duplicate));
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
        Assert.Equal("garage.not_found", await ErrorCodeAsync(unknown));
    }

    [Fact]
    public async Task Un_garage_desactive_n_est_plus_propose_mais_reste_liste_sur_demande()
    {
        var garage = await CreateGarageAsync(NewName("Ancien"));

        var deactivated = await _client.PostAsync(new Uri($"/api/garages/{garage.Id}/deactivate", UriKind.Relative), null);
        Assert.Equal(HttpStatusCode.OK, deactivated.StatusCode);
        Assert.False((await deactivated.Content.ReadFromJsonAsync<GarageDto>(Json))!.IsActive);

        var proposed = await _client.GetFromJsonAsync<List<GarageDto>>(Garages, Json);
        var all = await _client.GetFromJsonAsync<List<GarageDto>>(new Uri("/api/garages?includeInactive=true", UriKind.Relative), Json);
        Assert.DoesNotContain(proposed!, g => g.Id == garage.Id);
        Assert.Contains(all!, g => g.Id == garage.Id && !g.IsActive);
    }

    [Fact]
    public async Task Desactiver_un_garage_inconnu_renvoie_introuvable()
    {
        var response = await _client.PostAsync(new Uri($"/api/garages/{Guid.NewGuid()}/deactivate", UriKind.Relative), null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("garage.not_found", await ErrorCodeAsync(response));
    }

    [Fact]
    public async Task Un_garage_ne_peut_pas_etre_supprime()
    {
        var garage = await CreateGarageAsync(NewName("Permanent"));

        var response = await _client.DeleteAsync(new Uri($"/api/garages/{garage.Id}", UriKind.Relative));

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }
}
