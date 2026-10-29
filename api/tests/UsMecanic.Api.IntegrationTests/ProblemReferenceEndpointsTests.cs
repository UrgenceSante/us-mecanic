using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace UsMecanic.Api.IntegrationTests;

public class ProblemReferenceEndpointsTests : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _client;
    private readonly HttpClient _anonymous;

    public ProblemReferenceEndpointsTests(ApiFactory factory)
    {
        _client = factory.CreateAuthenticatedClient();
        _anonymous = factory.CreateClient();
    }

    private sealed record ReferenceDto(Guid Id, string Name, bool IsActive);

    /// <summary>Libellé unique : les tests d'une classe partagent la même base.</summary>
    private static string NewName(string prefix) => $"{prefix} {Guid.NewGuid():N}";

    private static Uri Url(string route, string suffix = "") => new($"{route}{suffix}", UriKind.Relative);

    private async Task<ReferenceDto> CreateAsync(string route, string name)
    {
        var response = await _client.PostAsJsonAsync(Url(route), new { name });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<ReferenceDto>(Json))!;
    }

    private static async Task<string?> ErrorCodeAsync(HttpResponseMessage response)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return problem.RootElement.GetProperty("code").GetString();
    }

    [Theory]
    [InlineData("/api/problem-natures")]
    [InlineData("/api/problem-elements")]
    [InlineData("/api/problem-actors")]
    [InlineData("/api/problem-action-types")]
    [InlineData("/api/problem-constraints")]
    public async Task Les_referentiels_exigent_une_authentification(string route)
    {
        var response = await _anonymous.GetAsync(Url(route));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/problem-natures", "Mécanique")]
    [InlineData("/api/problem-elements", "Freins")]
    [InlineData("/api/problem-actors", "Atelier interne")]
    [InlineData("/api/problem-action-types", "Remplacement")]
    [InlineData("/api/problem-constraints", "Sous 7 jours")]
    public async Task Les_valeurs_de_depart_sont_presentes_apres_migration(string route, string expected)
    {
        var list = await _client.GetFromJsonAsync<List<ReferenceDto>>(Url(route), Json);

        Assert.True(list!.Count >= 3);
        Assert.Contains(list, v => v.Name == expected && v.IsActive);
    }

    [Theory]
    [InlineData("/api/problem-natures")]
    [InlineData("/api/problem-elements")]
    [InlineData("/api/problem-actors")]
    [InlineData("/api/problem-action-types")]
    [InlineData("/api/problem-constraints")]
    public async Task Creation_puis_liste_et_renommage(string route)
    {
        var created = await CreateAsync(route, $"  {NewName("Valeur")} ");
        Assert.True(created.IsActive);
        Assert.StartsWith("Valeur ", created.Name, StringComparison.Ordinal);

        var list = await _client.GetFromJsonAsync<List<ReferenceDto>>(Url(route), Json);
        Assert.Contains(list!, v => v.Id == created.Id);

        var newName = NewName("Autre valeur");
        var renamed = await _client.PutAsJsonAsync(Url(route, $"/{created.Id}"), new { name = newName });
        Assert.Equal(HttpStatusCode.OK, renamed.StatusCode);
        Assert.Equal(newName, (await renamed.Content.ReadFromJsonAsync<ReferenceDto>(Json))!.Name);
    }

    [Theory]
    [InlineData("/api/problem-natures", "problem_nature")]
    [InlineData("/api/problem-elements", "problem_element")]
    [InlineData("/api/problem-actors", "problem_actor")]
    [InlineData("/api/problem-action-types", "problem_action_type")]
    [InlineData("/api/problem-constraints", "problem_constraint")]
    public async Task Un_libelle_vide_en_double_ou_trop_long_renvoie_un_ProblemDetails(string route, string codePrefix)
    {
        var name = NewName("Doublon");
        await CreateAsync(route, name);

        var empty = await _client.PostAsJsonAsync(Url(route), new { name = " " });
        var duplicate = await _client.PostAsJsonAsync(Url(route), new { name = name.ToUpperInvariant() });
        var tooLong = await _client.PostAsJsonAsync(Url(route), new { name = new string('x', 101) });

        Assert.Equal(HttpStatusCode.BadRequest, empty.StatusCode);
        Assert.Equal($"{codePrefix}.name_required", await ErrorCodeAsync(empty));
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.Equal($"{codePrefix}.name_already_exists", await ErrorCodeAsync(duplicate));
        Assert.Equal(HttpStatusCode.BadRequest, tooLong.StatusCode);
        Assert.Equal($"{codePrefix}.name_too_long", await ErrorCodeAsync(tooLong));
    }

    [Fact]
    public async Task Le_meme_libelle_est_accepte_dans_deux_referentiels_differents()
    {
        var name = NewName("Commun");
        await CreateAsync("/api/problem-natures", name);

        var created = await CreateAsync("/api/problem-actors", name);

        Assert.Equal(name, created.Name);
    }

    [Fact]
    public async Task Renommer_vers_un_libelle_existant_ou_une_valeur_inconnue_est_refuse()
    {
        const string route = "/api/problem-natures";
        var other = await CreateAsync(route, NewName("Autre"));
        var item = await CreateAsync(route, NewName("Nature"));

        var duplicate = await _client.PutAsJsonAsync(Url(route, $"/{item.Id}"), new { name = other.Name });
        var unknown = await _client.PutAsJsonAsync(Url(route, $"/{Guid.NewGuid()}"), new { name = NewName("X") });

        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.Equal("problem_nature.name_already_exists", await ErrorCodeAsync(duplicate));
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
        Assert.Equal("problem_nature.not_found", await ErrorCodeAsync(unknown));
    }

    [Fact]
    public async Task Une_valeur_desactivee_n_est_plus_proposee_mais_reste_listee_sur_demande()
    {
        const string route = "/api/problem-constraints";
        var item = await CreateAsync(route, NewName("Ancienne"));

        var deactivated = await _client.PostAsync(Url(route, $"/{item.Id}/deactivate"), null);
        Assert.Equal(HttpStatusCode.OK, deactivated.StatusCode);
        Assert.False((await deactivated.Content.ReadFromJsonAsync<ReferenceDto>(Json))!.IsActive);

        var proposed = await _client.GetFromJsonAsync<List<ReferenceDto>>(Url(route), Json);
        var all = await _client.GetFromJsonAsync<List<ReferenceDto>>(Url(route, "?includeInactive=true"), Json);
        Assert.DoesNotContain(proposed!, v => v.Id == item.Id);
        Assert.Contains(all!, v => v.Id == item.Id && !v.IsActive);
    }

    [Fact]
    public async Task Desactiver_une_valeur_inconnue_renvoie_introuvable()
    {
        var response = await _client.PostAsync(Url("/api/problem-actors", $"/{Guid.NewGuid()}/deactivate"), null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("problem_actor.not_found", await ErrorCodeAsync(response));
    }

    [Fact]
    public async Task Une_valeur_ne_peut_pas_etre_supprimee()
    {
        var item = await CreateAsync("/api/problem-elements", NewName("Permanent"));

        var response = await _client.DeleteAsync(Url("/api/problem-elements", $"/{item.Id}"));

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }
}
