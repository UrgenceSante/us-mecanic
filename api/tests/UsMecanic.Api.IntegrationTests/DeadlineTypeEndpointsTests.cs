using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace UsMecanic.Api.IntegrationTests;

public class DeadlineTypeEndpointsTests : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly Uri Types = new("/api/deadline-types", UriKind.Relative);
    private static readonly Uri Thresholds = new("/api/deadline-thresholds", UriKind.Relative);

    private static readonly string[] InitialNames =
    [
        "Contrôle technique",
        "Assurance",
        "Entretien constructeur",
        "Agrément ARS",
        "Matériel médical",
        "Oxygène",
        "Extincteur",
    ];

    private static readonly string[] YearlyNames = ["Contrôle technique", "Assurance", "Extincteur"];

    private static readonly string[] OneOffNames = ["Agrément ARS", "Matériel médical", "Oxygène"];

    private readonly HttpClient _client;
    private readonly HttpClient _anonymous;

    public DeadlineTypeEndpointsTests(ApiFactory factory)
    {
        _client = factory.CreateAuthenticatedClient();
        _anonymous = factory.CreateClient();
    }

    private sealed record TypeDto(Guid Id, string Name, int? DefaultIntervalMonths, int? DefaultIntervalKm, int? DueSoonDays, int? DueSoonKm, bool IsOneOff, bool IsActive);

    private sealed record ThresholdsDto(int DueSoonDays, int DueSoonKm);

    /// <summary>Nom unique : les tests d'une classe partagent la même base.</summary>
    private static string NewName(string prefix) => $"{prefix} {Guid.NewGuid():N}";

    private static Uri TypeUri(Guid id) => new($"/api/deadline-types/{id}", UriKind.Relative);

    private async Task<TypeDto> CreateTypeAsync(object body)
    {
        var response = await _client.PostAsJsonAsync(Types, body);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<TypeDto>(Json))!;
    }

    private static async Task<string?> ErrorCodeAsync(HttpResponseMessage response)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return problem.RootElement.GetProperty("code").GetString();
    }

    [Fact]
    public async Task Les_types_et_les_seuils_exigent_une_authentification()
    {
        var types = await _anonymous.GetAsync(Types);
        var thresholds = await _anonymous.GetAsync(Thresholds);

        Assert.Equal(HttpStatusCode.Unauthorized, types.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, thresholds.StatusCode);
    }

    [Fact]
    public async Task Les_sept_types_initiaux_existent_apres_la_migration()
    {
        var list = await _client.GetFromJsonAsync<List<TypeDto>>(new Uri("/api/deadline-types?includeInactive=true", UriKind.Relative), Json);

        foreach (var name in InitialNames)
        {
            Assert.Contains(list!, t => t.Name == name);
        }

        var insurance = list!.Single(t => t.Name == "Assurance");
        Assert.Equal(12, insurance.DefaultIntervalMonths);
        Assert.False(insurance.IsOneOff);
        Assert.True(list!.Single(t => t.Name == "Agrément ARS").IsOneOff);
    }

    [Fact]
    public async Task Entretien_constructeur_est_periodique_et_les_autres_types_initiaux_sont_inchanges()
    {
        var response = await _client.GetAsync(new Uri("/api/deadline-types?includeInactive=true", UriKind.Relative));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var list = (await response.Content.ReadFromJsonAsync<List<TypeDto>>(Json))!;

        var maintenance = list.Single(t => t.Name == "Entretien constructeur");
        Assert.Equal(12, maintenance.DefaultIntervalMonths);
        Assert.Equal(30000, maintenance.DefaultIntervalKm);
        Assert.False(maintenance.IsOneOff);

        foreach (var name in YearlyNames)
        {
            var type = list.Single(t => t.Name == name);
            Assert.Equal(12, type.DefaultIntervalMonths);
            Assert.Null(type.DefaultIntervalKm);
        }

        foreach (var name in OneOffNames)
        {
            var type = list.Single(t => t.Name == name);
            Assert.Null(type.DefaultIntervalMonths);
            Assert.Null(type.DefaultIntervalKm);
        }
    }

    [Fact]
    public async Task Creation_sans_periodicite_donne_un_type_ponctuel()
    {
        var created = await CreateTypeAsync(new { name = $"  {NewName("Visite")} " });

        Assert.True(created.IsOneOff);
        Assert.True(created.IsActive);
        Assert.Null(created.DefaultIntervalMonths);
        Assert.StartsWith("Visite ", created.Name, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Creation_avec_periodicite_et_seuils_puis_modification()
    {
        var created = await CreateTypeAsync(new { name = NewName("CT"), defaultIntervalMonths = 12, dueSoonDays = 45, dueSoonKm = 500 });
        Assert.False(created.IsOneOff);
        Assert.Equal(12, created.DefaultIntervalMonths);
        Assert.Equal(45, created.DueSoonDays);
        Assert.Equal(500, created.DueSoonKm);

        var newName = NewName("Entretien");
        var updated = await _client.PutAsJsonAsync(TypeUri(created.Id), new { name = newName, defaultIntervalKm = 15000 });

        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        var dto = (await updated.Content.ReadFromJsonAsync<TypeDto>(Json))!;
        Assert.Equal(newName, dto.Name);
        Assert.Null(dto.DefaultIntervalMonths);
        Assert.Equal(15000, dto.DefaultIntervalKm);
        Assert.Null(dto.DueSoonDays);
    }

    [Fact]
    public async Task Un_nom_vide_ou_en_double_renvoie_un_ProblemDetails()
    {
        var name = NewName("Sang");
        await CreateTypeAsync(new { name });

        var empty = await _client.PostAsJsonAsync(Types, new { name = " " });
        var duplicate = await _client.PostAsJsonAsync(Types, new { name = name.ToUpperInvariant() });

        Assert.Equal(HttpStatusCode.BadRequest, empty.StatusCode);
        Assert.Equal("deadline_type.name_required", await ErrorCodeAsync(empty));
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.Equal("deadline_type.name_already_exists", await ErrorCodeAsync(duplicate));
    }

    [Fact]
    public async Task Les_periodicites_et_seuils_nuls_ou_negatifs_sont_refuses()
    {
        var months = await _client.PostAsJsonAsync(Types, new { name = NewName("A"), defaultIntervalMonths = 0 });
        var km = await _client.PostAsJsonAsync(Types, new { name = NewName("B"), defaultIntervalKm = -10 });
        var days = await _client.PostAsJsonAsync(Types, new { name = NewName("C"), dueSoonDays = -1 });
        var dueKm = await _client.PostAsJsonAsync(Types, new { name = NewName("D"), dueSoonKm = 0 });

        Assert.Equal(HttpStatusCode.BadRequest, months.StatusCode);
        Assert.Equal("deadline_type.interval_months_invalid", await ErrorCodeAsync(months));
        Assert.Equal(HttpStatusCode.BadRequest, km.StatusCode);
        Assert.Equal("deadline_type.interval_km_invalid", await ErrorCodeAsync(km));
        Assert.Equal(HttpStatusCode.BadRequest, days.StatusCode);
        Assert.Equal("deadline_type.due_soon_days_invalid", await ErrorCodeAsync(days));
        Assert.Equal(HttpStatusCode.BadRequest, dueKm.StatusCode);
        Assert.Equal("deadline_type.due_soon_km_invalid", await ErrorCodeAsync(dueKm));
    }

    [Fact]
    public async Task Modifier_vers_un_nom_existant_une_valeur_invalide_ou_un_type_inconnu_est_refuse()
    {
        var other = await CreateTypeAsync(new { name = NewName("Autre") });
        var type = await CreateTypeAsync(new { name = NewName("Type") });

        var duplicate = await _client.PutAsJsonAsync(TypeUri(type.Id), new { name = other.Name });
        var invalid = await _client.PutAsJsonAsync(TypeUri(type.Id), new { name = type.Name, defaultIntervalMonths = -1 });
        var unknown = await _client.PutAsJsonAsync(TypeUri(Guid.NewGuid()), new { name = NewName("X") });

        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.Equal("deadline_type.name_already_exists", await ErrorCodeAsync(duplicate));
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.Equal("deadline_type.interval_months_invalid", await ErrorCodeAsync(invalid));
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
        Assert.Equal("deadline_type.not_found", await ErrorCodeAsync(unknown));
    }

    [Fact]
    public async Task Un_type_desactive_n_est_plus_propose_mais_reste_listable_sur_demande()
    {
        var type = await CreateTypeAsync(new { name = NewName("Ancien") });

        var deactivated = await _client.PostAsync(new Uri($"/api/deadline-types/{type.Id}/deactivate", UriKind.Relative), null);
        Assert.Equal(HttpStatusCode.OK, deactivated.StatusCode);
        Assert.False((await deactivated.Content.ReadFromJsonAsync<TypeDto>(Json))!.IsActive);

        var proposed = await _client.GetFromJsonAsync<List<TypeDto>>(Types, Json);
        var all = await _client.GetFromJsonAsync<List<TypeDto>>(new Uri("/api/deadline-types?includeInactive=true", UriKind.Relative), Json);
        Assert.DoesNotContain(proposed!, t => t.Id == type.Id);
        Assert.Contains(all!, t => t.Id == type.Id && !t.IsActive);
    }

    [Fact]
    public async Task Un_type_ne_peut_pas_etre_supprime()
    {
        var type = await CreateTypeAsync(new { name = NewName("Permanent") });

        var response = await _client.DeleteAsync(TypeUri(type.Id));

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public async Task Les_seuils_globaux_sont_lus_modifies_et_refuses_si_invalides()
    {
        var initial = await _client.GetFromJsonAsync<ThresholdsDto>(Thresholds, Json);
        Assert.Equal(new ThresholdsDto(30, 1000), initial);

        try
        {
            var updated = await _client.PutAsJsonAsync(Thresholds, new { dueSoonDays = 45, dueSoonKm = 1500 });
            Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
            Assert.Equal(new ThresholdsDto(45, 1500), await updated.Content.ReadFromJsonAsync<ThresholdsDto>(Json));
            Assert.Equal(new ThresholdsDto(45, 1500), await _client.GetFromJsonAsync<ThresholdsDto>(Thresholds, Json));

            var negativeDays = await _client.PutAsJsonAsync(Thresholds, new { dueSoonDays = -1, dueSoonKm = 1500 });
            var zeroKm = await _client.PutAsJsonAsync(Thresholds, new { dueSoonDays = 45, dueSoonKm = 0 });
            Assert.Equal(HttpStatusCode.BadRequest, negativeDays.StatusCode);
            Assert.Equal("deadline_thresholds.days_invalid", await ErrorCodeAsync(negativeDays));
            Assert.Equal(HttpStatusCode.BadRequest, zeroKm.StatusCode);
            Assert.Equal("deadline_thresholds.km_invalid", await ErrorCodeAsync(zeroKm));
        }
        finally
        {
            // Les tests de la classe partagent la base : on remet les valeurs par défaut.
            var restored = await _client.PutAsJsonAsync(Thresholds, new { dueSoonDays = 30, dueSoonKm = 1000 });
            Assert.Equal(HttpStatusCode.OK, restored.StatusCode);
        }
    }
}
