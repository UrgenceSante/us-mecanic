using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace UsMecanic.Api.IntegrationTests;

public class VehicleOwnerEndpointsTests : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly Uri Vehicles = new("/api/vehicles", UriKind.Relative);
    private static readonly Uri Companies = new("/api/companies", UriKind.Relative);

    private readonly HttpClient _client;

    public VehicleOwnerEndpointsTests(ApiFactory factory) => _client = factory.CreateAuthenticatedClient();

    private sealed record CompanyDto(Guid Id, string Name, bool IsActive);

    private sealed record UserDto(CompanyDto? Company, string Source);

    private sealed record VehicleDto(Guid Id, string LicensePlate, CompanyDto? Owner, UserDto CurrentUser);

    private static string NewPlate() => $"{(char)('A' + Random.Shared.Next(26))}{(char)('A' + Random.Shared.Next(26))}-{Random.Shared.Next(100, 1000)}-{(char)('A' + Random.Shared.Next(26))}{(char)('A' + Random.Shared.Next(26))}";

    private async Task<CompanyDto> CreateCompanyAsync(bool active = true)
    {
        var response = await _client.PostAsJsonAsync(Companies, new { name = $"Société {Guid.NewGuid():N}" });
        var company = (await response.Content.ReadFromJsonAsync<CompanyDto>(Json))!;
        if (!active)
        {
            await _client.PostAsync(new Uri($"/api/companies/{company.Id}/deactivate", UriKind.Relative), null);
        }

        return company;
    }

    private async Task<VehicleDto> CreateVehicleAsync(Guid? owner)
    {
        var response = await _client.PostAsJsonAsync(Vehicles, new { licensePlate = NewPlate(), details = new { brand = "Renault", ownerCompanyId = owner } });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<VehicleDto>(Json))!;
    }

    private Task<HttpResponseMessage> UpdateAsync(Guid id, Guid? owner) =>
        _client.PutAsJsonAsync(new Uri($"/api/vehicles/{id}", UriKind.Relative), new { brand = "Renault", ownerCompanyId = owner });

    private static async Task<string?> ErrorCodeAsync(HttpResponseMessage response)
    {
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return problem.RootElement.GetProperty("code").GetString();
    }

    [Fact]
    public async Task Vehicule_sans_proprietaire_a_un_utilisateur_non_renseigne()
    {
        var vehicle = await CreateVehicleAsync(null);

        Assert.Null(vehicle.Owner);
        Assert.Equal("NotSet", vehicle.CurrentUser.Source);
    }

    [Fact]
    public async Task Le_proprietaire_est_enregistre_relu_et_liste()
    {
        var company = await CreateCompanyAsync();

        var created = await CreateVehicleAsync(company.Id);
        var read = await _client.GetFromJsonAsync<VehicleDto>(new Uri($"/api/vehicles/{created.Id}", UriKind.Relative), Json);
        var list = await _client.GetFromJsonAsync<List<VehicleDto>>(new Uri($"/api/vehicles?search={created.LicensePlate}", UriKind.Relative), Json);

        Assert.Equal(company.Id, read!.Owner?.Id);
        Assert.Equal("Owner", read.CurrentUser.Source);
        Assert.Equal(company.Id, read.CurrentUser.Company?.Id);
        Assert.Equal(company.Id, Assert.Single(list!).Owner?.Id);
    }

    [Fact]
    public async Task Le_proprietaire_peut_etre_renseigne_apres_coup_puis_change_mais_pas_vide()
    {
        var first = await CreateCompanyAsync();
        var second = await CreateCompanyAsync();
        var vehicle = await CreateVehicleAsync(null);

        var set = await UpdateAsync(vehicle.Id, first.Id);
        var changed = await UpdateAsync(vehicle.Id, second.Id);
        var cleared = await UpdateAsync(vehicle.Id, null);

        Assert.Equal(HttpStatusCode.OK, set.StatusCode);
        Assert.Equal(second.Id, (await changed.Content.ReadFromJsonAsync<VehicleDto>(Json))!.Owner?.Id);
        Assert.Equal(HttpStatusCode.BadRequest, cleared.StatusCode);
        Assert.Equal("vehicle.owner_cannot_be_removed", await ErrorCodeAsync(cleared));
    }

    [Fact]
    public async Task Societe_inactive_ou_inconnue_est_refusee()
    {
        var inactive = await CreateCompanyAsync(active: false);

        var refused = await _client.PostAsJsonAsync(Vehicles, new { licensePlate = NewPlate(), details = new { ownerCompanyId = inactive.Id } });
        var unknown = await _client.PostAsJsonAsync(Vehicles, new { licensePlate = NewPlate(), details = new { ownerCompanyId = Guid.NewGuid() } });

        Assert.Equal("company.inactive", await ErrorCodeAsync(refused));
        Assert.Equal("company.unknown", await ErrorCodeAsync(unknown));
    }

    [Fact]
    public async Task Vehicule_sorti_du_parc_accepte_le_proprietaire_seulement_s_il_est_vide()
    {
        var company = await CreateCompanyAsync();
        var other = await CreateCompanyAsync();
        var vehicle = await CreateVehicleAsync(null);
        await _client.PutAsJsonAsync(new Uri($"/api/vehicles/{vehicle.Id}/status", UriKind.Relative), new { status = "Retired" });

        var assigned = await UpdateAsync(vehicle.Id, company.Id);
        var changed = await UpdateAsync(vehicle.Id, other.Id);

        Assert.Equal(HttpStatusCode.OK, assigned.StatusCode);
        Assert.Equal(company.Id, (await assigned.Content.ReadFromJsonAsync<VehicleDto>(Json))!.Owner?.Id);
        Assert.Equal("vehicle.retired", await ErrorCodeAsync(changed));
    }
}
