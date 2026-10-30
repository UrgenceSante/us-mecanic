using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace UsMecanic.Api.IntegrationTests;

public class LoanEndpointsTests : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _client;
    private readonly HttpClient _anonymous;

    public LoanEndpointsTests(ApiFactory factory)
    {
        _client = factory.CreateAuthenticatedClient();
        _anonymous = factory.CreateClient();
    }

    private sealed record LoanDto(
        Guid Id, Guid VehicleId, string Lender, decimal BorrowedAmount, decimal MonthlyPayment, DateOnly StartDate, int DurationMonths, DateTimeOffset RecordedAt);

    private sealed record VehicleDto(Guid Id);

    /// <summary>Plaque SIV unique : les tests d'une classe partagent la même base.</summary>
    private static string NewPlate()
    {
        var n = Random.Shared.Next(0, 26 * 26 * 1000 * 26 * 26);
        static char L(int i) => (char)('A' + (i % 26));
        return $"{L(n)}{L(n / 26)}-{n / 676 % 1000:000}-{L(n / 676000)}{L(n / 17576000)}";
    }

    private static Uri LoansUri(Guid vehicleId) => new($"/api/vehicles/{vehicleId}/loans", UriKind.Relative);

    private static Uri LoanUri(Guid vehicleId, Guid loanId) => new($"/api/vehicles/{vehicleId}/loans/{loanId}", UriKind.Relative);

    private static object Body(string lender = "Banque Fictive", decimal borrowed = 12_000.50m, decimal monthly = 250.25m, string start = "2026-06-15", int months = 48) =>
        new { lender, borrowedAmount = borrowed, monthlyPayment = monthly, startDate = start, durationMonths = months };

    private async Task<Guid> CreateVehicleAsync()
    {
        var response = await _client.PostAsJsonAsync(new Uri("/api/vehicles", UriKind.Relative), new { licensePlate = NewPlate(), details = new { brand = "Renault" } });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<VehicleDto>(Json))!.Id;
    }

    private async Task<LoanDto> AddAsync(Guid vehicleId, object? body = null)
    {
        var response = await _client.PostAsJsonAsync(LoansUri(vehicleId), body ?? Body());
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<LoanDto>(Json))!;
    }

    private static async Task<string?> ErrorCodeAsync(HttpResponseMessage response)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return problem.RootElement.GetProperty("code").GetString();
    }

    [Fact]
    public async Task Les_emprunts_exigent_une_authentification()
    {
        var response = await _anonymous.GetAsync(LoansUri(Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Creation_puis_liste()
    {
        var vehicleId = await CreateVehicleAsync();

        var response = await _client.PostAsJsonAsync(LoansUri(vehicleId), Body(lender: "  Banque Fictive "));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = (await response.Content.ReadFromJsonAsync<LoanDto>(Json))!;
        Assert.Equal(LoanUri(vehicleId, created.Id).ToString(), response.Headers.Location?.ToString());
        Assert.Equal(vehicleId, created.VehicleId);
        Assert.Equal("Banque Fictive", created.Lender);
        Assert.Equal(12_000.50m, created.BorrowedAmount);
        Assert.Equal(250.25m, created.MonthlyPayment);
        Assert.Equal(new DateOnly(2026, 6, 15), created.StartDate);
        Assert.Equal(48, created.DurationMonths);

        var list = await _client.GetFromJsonAsync<List<LoanDto>>(LoansUri(vehicleId), Json);
        Assert.Equal(created.Id, Assert.Single(list!).Id);
        Assert.Equal(250.25m, list![0].MonthlyPayment);
    }

    [Fact]
    public async Task La_liste_est_propre_au_vehicule()
    {
        var vehicleId = await CreateVehicleAsync();
        var otherId = await CreateVehicleAsync();
        await AddAsync(vehicleId);

        var list = await _client.GetFromJsonAsync<List<LoanDto>>(LoansUri(otherId), Json);

        Assert.Empty(list!);
    }

    [Fact]
    public async Task Un_vehicule_inconnu_renvoie_404()
    {
        var unknown = Guid.NewGuid();

        var list = await _client.GetAsync(LoansUri(unknown));
        var add = await _client.PostAsJsonAsync(LoansUri(unknown), Body());

        Assert.Equal(HttpStatusCode.NotFound, list.StatusCode);
        Assert.Equal("vehicle.not_found", await ErrorCodeAsync(list));
        Assert.Equal(HttpStatusCode.NotFound, add.StatusCode);
        Assert.Equal("vehicle.not_found", await ErrorCodeAsync(add));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-6)]
    public async Task Une_duree_nulle_ou_negative_est_refusee(int months)
    {
        var vehicleId = await CreateVehicleAsync();

        var response = await _client.PostAsJsonAsync(LoansUri(vehicleId), Body(months: months));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("loan.duration_invalid", await ErrorCodeAsync(response));
    }

    [Fact]
    public async Task Une_mensualite_negative_est_refusee_mais_zero_est_accepte()
    {
        var vehicleId = await CreateVehicleAsync();

        var negative = await _client.PostAsJsonAsync(LoansUri(vehicleId), Body(monthly: -1m));
        var zero = await _client.PostAsJsonAsync(LoansUri(vehicleId), Body(borrowed: 0m, monthly: 0m));

        Assert.Equal(HttpStatusCode.BadRequest, negative.StatusCode);
        Assert.Equal("loan.monthly_payment_negative", await ErrorCodeAsync(negative));
        Assert.Equal(HttpStatusCode.Created, zero.StatusCode);
    }

    [Fact]
    public async Task Un_montant_emprunte_negatif_ou_a_trois_decimales_est_refuse()
    {
        var vehicleId = await CreateVehicleAsync();

        var negative = await _client.PostAsJsonAsync(LoansUri(vehicleId), Body(borrowed: -100m));
        var precision = await _client.PostAsJsonAsync(LoansUri(vehicleId), Body(borrowed: 100.123m));

        Assert.Equal(HttpStatusCode.BadRequest, negative.StatusCode);
        Assert.Equal("loan.borrowed_amount_negative", await ErrorCodeAsync(negative));
        Assert.Equal(HttpStatusCode.BadRequest, precision.StatusCode);
        Assert.Equal("loan.amount_precision", await ErrorCodeAsync(precision));
    }

    [Fact]
    public async Task Un_organisme_vide_ou_une_date_absente_est_refuse()
    {
        var vehicleId = await CreateVehicleAsync();

        var lender = await _client.PostAsJsonAsync(LoansUri(vehicleId), Body(lender: " "));
        var date = await _client.PostAsJsonAsync(LoansUri(vehicleId), new { lender = "Banque Fictive", borrowedAmount = 1m, monthlyPayment = 1m, durationMonths = 12 });

        Assert.Equal(HttpStatusCode.BadRequest, lender.StatusCode);
        Assert.Equal("loan.lender_required", await ErrorCodeAsync(lender));
        Assert.Equal(HttpStatusCode.BadRequest, date.StatusCode);
        Assert.Equal("loan.start_date_required", await ErrorCodeAsync(date));
    }

    [Fact]
    public async Task La_modification_de_la_duree_est_enregistree()
    {
        var vehicleId = await CreateVehicleAsync();
        var loan = await AddAsync(vehicleId);

        var response = await _client.PutAsJsonAsync(LoanUri(vehicleId, loan.Id), Body(months: 18));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = (await response.Content.ReadFromJsonAsync<LoanDto>(Json))!;
        Assert.Equal(18, updated.DurationMonths);
        // PostgreSQL stocke les horodatages à la microseconde : la réponse de création est plus précise.
        Assert.Equal(loan.RecordedAt, updated.RecordedAt, TimeSpan.FromMilliseconds(1));
        var list = await _client.GetFromJsonAsync<List<LoanDto>>(LoansUri(vehicleId), Json);
        Assert.Equal(18, Assert.Single(list!).DurationMonths);
    }

    [Fact]
    public async Task Une_modification_invalide_est_refusee_et_ne_change_rien()
    {
        var vehicleId = await CreateVehicleAsync();
        var loan = await AddAsync(vehicleId);

        var response = await _client.PutAsJsonAsync(LoanUri(vehicleId, loan.Id), Body(months: 0));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("loan.duration_invalid", await ErrorCodeAsync(response));
        var list = await _client.GetFromJsonAsync<List<LoanDto>>(LoansUri(vehicleId), Json);
        Assert.Equal(48, Assert.Single(list!).DurationMonths);
    }

    [Fact]
    public async Task Modification_et_suppression_d_un_emprunt_inconnu_renvoient_404()
    {
        var vehicleId = await CreateVehicleAsync();

        var put = await _client.PutAsJsonAsync(LoanUri(vehicleId, Guid.NewGuid()), Body());
        var delete = await _client.DeleteAsync(LoanUri(vehicleId, Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.NotFound, put.StatusCode);
        Assert.Equal("loan.not_found", await ErrorCodeAsync(put));
        Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);
        Assert.Equal("loan.not_found", await ErrorCodeAsync(delete));
    }

    [Fact]
    public async Task Un_emprunt_d_un_autre_vehicule_reste_introuvable()
    {
        var vehicleId = await CreateVehicleAsync();
        var otherId = await CreateVehicleAsync();
        var loan = await AddAsync(vehicleId);

        var delete = await _client.DeleteAsync(LoanUri(otherId, loan.Id));

        Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);
    }

    [Fact]
    public async Task Suppression()
    {
        var vehicleId = await CreateVehicleAsync();
        var loan = await AddAsync(vehicleId);

        var delete = await _client.DeleteAsync(LoanUri(vehicleId, loan.Id));

        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        var list = await _client.GetFromJsonAsync<List<LoanDto>>(LoansUri(vehicleId), Json);
        Assert.Empty(list!);
    }

    [Fact]
    public async Task Un_vehicule_sorti_du_parc_refuse_la_creation_mais_autorise_modification_et_suppression()
    {
        var vehicleId = await CreateVehicleAsync();
        var loan = await AddAsync(vehicleId);
        var retire = await _client.PutAsJsonAsync(new Uri($"/api/vehicles/{vehicleId}/status", UriKind.Relative), new { status = "Retired" });
        Assert.Equal(HttpStatusCode.OK, retire.StatusCode);

        var add = await _client.PostAsJsonAsync(LoansUri(vehicleId), Body());
        var put = await _client.PutAsJsonAsync(LoanUri(vehicleId, loan.Id), Body(months: 6));
        var list = await _client.GetFromJsonAsync<List<LoanDto>>(LoansUri(vehicleId), Json);
        var delete = await _client.DeleteAsync(LoanUri(vehicleId, loan.Id));

        Assert.Equal(HttpStatusCode.Conflict, add.StatusCode);
        Assert.Equal("vehicle.retired", await ErrorCodeAsync(add));
        Assert.Equal(HttpStatusCode.OK, put.StatusCode);
        Assert.Equal(6, Assert.Single(list!).DurationMonths);
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
    }

    [Fact]
    public async Task Un_montant_ou_une_mensualite_absents_sont_refuses()
    {
        var vehicleId = await CreateVehicleAsync();

        var borrowed = await _client.PostAsJsonAsync(LoansUri(vehicleId), new { lender = "Banque Fictive", monthlyPayment = 1m, startDate = "2026-06-15", durationMonths = 12 });
        var monthly = await _client.PostAsJsonAsync(LoansUri(vehicleId), new { lender = "Banque Fictive", borrowedAmount = 1m, startDate = "2026-06-15", durationMonths = 12 });

        Assert.Equal(HttpStatusCode.BadRequest, borrowed.StatusCode);
        Assert.Equal("loan.borrowed_amount_required", await ErrorCodeAsync(borrowed));
        Assert.Equal(HttpStatusCode.BadRequest, monthly.StatusCode);
        Assert.Equal("loan.monthly_payment_required", await ErrorCodeAsync(monthly));
    }
}
