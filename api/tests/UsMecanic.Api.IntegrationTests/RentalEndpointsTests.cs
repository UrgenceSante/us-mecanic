using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace UsMecanic.Api.IntegrationTests;

public class RentalEndpointsTests : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly Uri Vehicles = new("/api/vehicles", UriKind.Relative);
    private static readonly Uri Companies = new("/api/companies", UriKind.Relative);

    private readonly HttpClient _client;
    private readonly HttpClient _anonymous;

    public RentalEndpointsTests(ApiFactory factory)
    {
        _client = factory.CreateAuthenticatedClient();
        _anonymous = factory.CreateClient();
    }

    private sealed record CompanyDto(Guid Id, string Name, bool IsActive);

    private sealed record UserDto(CompanyDto? Company, string Source);

    private sealed record VehicleDto(Guid Id, CompanyDto? Owner, UserDto CurrentUser);

    private sealed record RentalDto(Guid Id, Guid VehicleId, CompanyDto Tenant, DateOnly StartDate, DateOnly? EndDate, string State);

    private static string NewPlate() => $"{(char)('A' + Random.Shared.Next(26))}{(char)('A' + Random.Shared.Next(26))}-{Random.Shared.Next(100, 1000)}-{(char)('A' + Random.Shared.Next(26))}{(char)('A' + Random.Shared.Next(26))}";

    private static Uri RentalsUri(Guid vehicleId) => new($"/api/vehicles/{vehicleId}/rentals", UriKind.Relative);

    private static Uri RentalUri(Guid vehicleId, Guid rentalId, string suffix = "") => new($"/api/vehicles/{vehicleId}/rentals/{rentalId}{suffix}", UriKind.Relative);

    private async Task<CompanyDto> CreateCompanyAsync()
    {
        var response = await _client.PostAsJsonAsync(Companies, new { name = $"Société {Guid.NewGuid():N}" });
        return (await response.Content.ReadFromJsonAsync<CompanyDto>(Json))!;
    }

    private async Task<VehicleDto> CreateVehicleAsync(Guid? owner)
    {
        var response = await _client.PostAsJsonAsync(Vehicles, new { licensePlate = NewPlate(), details = new { brand = "Renault", ownerCompanyId = owner } });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<VehicleDto>(Json))!;
    }

    private static async Task<string?> ErrorCodeAsync(HttpResponseMessage response)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return problem.RootElement.GetProperty("code").GetString();
    }

    private static object Body(Guid tenant, string start, string? end) => new { tenantCompanyId = tenant, startDate = start, endDate = end };

    [Fact]
    public async Task Les_endpoints_exigent_une_authentification()
    {
        var response = await _anonymous.GetAsync(RentalsUri(Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Cycle_complet_creer_terminer_modifier_supprimer()
    {
        var owner = await CreateCompanyAsync();
        var tenant = await CreateCompanyAsync();
        var vehicle = await CreateVehicleAsync(owner.Id);

        var created = await _client.PostAsJsonAsync(RentalsUri(vehicle.Id), Body(tenant.Id, "2026-02-01", null));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var rental = (await created.Content.ReadFromJsonAsync<RentalDto>(Json))!;
        Assert.Equal(tenant.Id, rental.Tenant.Id);

        var ended = await _client.PostAsJsonAsync(RentalUri(vehicle.Id, rental.Id, "/end"), new { endDate = "2026-03-31" });
        Assert.Equal(HttpStatusCode.OK, ended.StatusCode);
        Assert.Equal(new DateOnly(2026, 3, 31), (await ended.Content.ReadFromJsonAsync<RentalDto>(Json))!.EndDate);

        var updated = await _client.PutAsJsonAsync(RentalUri(vehicle.Id, rental.Id), Body(tenant.Id, "2026-02-01", "2026-04-15"));
        Assert.Equal(new DateOnly(2026, 4, 15), (await updated.Content.ReadFromJsonAsync<RentalDto>(Json))!.EndDate);

        var list = await _client.GetFromJsonAsync<List<RentalDto>>(RentalsUri(vehicle.Id), Json);
        Assert.Equal(rental.Id, Assert.Single(list!).Id);

        var deleted = await _client.DeleteAsync(RentalUri(vehicle.Id, rental.Id));
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        Assert.Empty((await _client.GetFromJsonAsync<List<RentalDto>>(RentalsUri(vehicle.Id), Json))!);
    }

    [Fact]
    public async Task Refuse_les_cas_metier()
    {
        var owner = await CreateCompanyAsync();
        var tenant = await CreateCompanyAsync();
        var vehicle = await CreateVehicleAsync(owner.Id);
        var noOwner = await CreateVehicleAsync(null);

        var first = await _client.PostAsJsonAsync(RentalsUri(vehicle.Id), Body(tenant.Id, "2026-02-01", "2026-03-31"));
        var overlap = await _client.PostAsJsonAsync(RentalsUri(vehicle.Id), Body(tenant.Id, "2026-03-31", null));
        var self = await _client.PostAsJsonAsync(RentalsUri(vehicle.Id), Body(owner.Id, "2026-05-01", null));
        var reversed = await _client.PostAsJsonAsync(RentalsUri(vehicle.Id), Body(tenant.Id, "2026-05-10", "2026-05-01"));
        var ownerless = await _client.PostAsJsonAsync(RentalsUri(noOwner.Id), Body(tenant.Id, "2026-05-01", null));
        var unknown = await _client.GetAsync(RentalsUri(Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, overlap.StatusCode);
        Assert.Equal("rental.overlap", await ErrorCodeAsync(overlap));
        Assert.Equal("rental.tenant_is_owner", await ErrorCodeAsync(self));
        Assert.Equal("rental.end_before_start", await ErrorCodeAsync(reversed));
        Assert.Equal("rental.owner_required", await ErrorCodeAsync(ownerless));
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
    }

    [Fact]
    public async Task Vehicule_sorti_du_parc_refuse_une_nouvelle_location()
    {
        var owner = await CreateCompanyAsync();
        var tenant = await CreateCompanyAsync();
        var vehicle = await CreateVehicleAsync(owner.Id);
        var created = await _client.PostAsJsonAsync(RentalsUri(vehicle.Id), Body(tenant.Id, "2026-02-01", null));
        var rental = (await created.Content.ReadFromJsonAsync<RentalDto>(Json))!;

        var retire = await _client.PutAsJsonAsync(new Uri($"/api/vehicles/{vehicle.Id}/status", UriKind.Relative), new { status = "Retired" });
        Assert.True(retire.IsSuccessStatusCode, await retire.Content.ReadAsStringAsync());
        var added = await _client.PostAsJsonAsync(RentalsUri(vehicle.Id), Body(tenant.Id, "2027-01-01", null));
        var ended = await _client.PostAsJsonAsync(RentalUri(vehicle.Id, rental.Id, "/end"), new { endDate = "2026-06-30" });

        Assert.Equal("rental.vehicle_retired", await ErrorCodeAsync(added));
        Assert.Equal(HttpStatusCode.OK, ended.StatusCode);
    }

    [Fact]
    public async Task Utilisateur_actuel_et_changement_de_proprietaire_refuse_pour_un_locataire()
    {
        var owner = await CreateCompanyAsync();
        var tenant = await CreateCompanyAsync();
        var vehicle = await CreateVehicleAsync(owner.Id);
        await _client.PostAsJsonAsync(RentalsUri(vehicle.Id), Body(tenant.Id, "2020-01-01", null));

        var read = await _client.GetFromJsonAsync<VehicleDto>(new Uri($"/api/vehicles/{vehicle.Id}", UriKind.Relative), Json);
        var change = await _client.PutAsJsonAsync(new Uri($"/api/vehicles/{vehicle.Id}", UriKind.Relative), new { brand = "Renault", ownerCompanyId = tenant.Id });

        Assert.Equal("Tenant", read!.CurrentUser.Source);
        Assert.Equal(tenant.Id, read.CurrentUser.Company?.Id);
        Assert.Equal(owner.Id, read.Owner?.Id);
        Assert.Equal(HttpStatusCode.BadRequest, change.StatusCode);
        Assert.Equal("vehicle.owner_is_tenant", await ErrorCodeAsync(change));
    }
}
