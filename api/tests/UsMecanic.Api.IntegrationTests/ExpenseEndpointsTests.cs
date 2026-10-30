using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UsMecanic.Infrastructure.Persistence;

namespace UsMecanic.Api.IntegrationTests;

public class ExpenseEndpointsTests : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly ApiFactory _factory;
    private readonly HttpClient _client;
    private readonly HttpClient _anonymous;

    public ExpenseEndpointsTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
        _anonymous = factory.CreateClient();
    }

    private sealed record IdDto(Guid Id);

    private sealed record CategoryDto(Guid Id, string Name, bool IsActive);

    private sealed record ExpenseDto(
        Guid Id, Guid VehicleId, DateOnly Date, string Family, Guid? CategoryId, string? CategoryName, decimal Amount, string? Label, string Source, DateTimeOffset RecordedAt);

    /// <summary>Jour décalé par rapport à aujourd'hui (jamais 0, pour rester stable autour de minuit UTC).</summary>
    private static DateOnly Offset(int days) => DateOnly.FromDateTime(DateTime.UtcNow).AddDays(days);

    private static string NewPlate()
    {
        var n = Random.Shared.Next(0, 26 * 26 * 1000 * 26 * 26);
        static char L(int i) => (char)('A' + (i % 26));
        return $"{L(n)}{L(n / 26)}-{n / 676 % 1000:000}-{L(n / 676000)}{L(n / 17576000)}";
    }

    private static Uri ExpensesUri(Guid vehicleId) => new($"/api/vehicles/{vehicleId}/expenses", UriKind.Relative);

    private static Uri ExpenseUri(Guid vehicleId, Guid expenseId) => new($"/api/vehicles/{vehicleId}/expenses/{expenseId}", UriKind.Relative);

    private static async Task<string?> ErrorCodeAsync(HttpResponseMessage response)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return problem.RootElement.GetProperty("code").GetString();
    }

    private async Task<Guid> CreateVehicleAsync()
    {
        var response = await _client.PostAsJsonAsync(new Uri("/api/vehicles", UriKind.Relative), new { licensePlate = NewPlate(), details = new { brand = "Renault" } });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<IdDto>(Json))!.Id;
    }

    private async Task<CategoryDto> CreateCategoryAsync(bool active = true)
    {
        var uri = new Uri("/api/expense-categories", UriKind.Relative);
        var response = await _client.PostAsJsonAsync(uri, new { name = $"Catégorie {Guid.NewGuid():N}" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var category = (await response.Content.ReadFromJsonAsync<CategoryDto>(Json))!;
        if (!active)
        {
            var off = await _client.PostAsync(new Uri($"/api/expense-categories/{category.Id}/deactivate", UriKind.Relative), null);
            Assert.Equal(HttpStatusCode.OK, off.StatusCode);
        }

        return category;
    }

    private async Task<ExpenseDto> AddExpenseAsync(Guid vehicleId, object body)
    {
        var response = await _client.PostAsJsonAsync(ExpensesUri(vehicleId), body);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<ExpenseDto>(Json))!;
    }

    private async Task<List<ExpenseDto>> ListAsync(Guid vehicleId) =>
        (await _client.GetFromJsonAsync<List<ExpenseDto>>(ExpensesUri(vehicleId), Json))!;

    [Fact]
    public async Task Les_depenses_exigent_une_authentification()
    {
        var read = await _anonymous.GetAsync(ExpensesUri(Guid.NewGuid()));
        var write = await _anonymous.PostAsJsonAsync(ExpensesUri(Guid.NewGuid()), new { date = Offset(-1) });
        var delete = await _anonymous.DeleteAsync(ExpenseUri(Guid.NewGuid(), Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.Unauthorized, read.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, write.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, delete.StatusCode);
    }

    [Fact]
    public async Task Cree_une_depense_avec_le_nom_de_sa_categorie_et_la_relit()
    {
        var vehicleId = await CreateVehicleAsync();
        var category = await CreateCategoryAsync();
        var date = Offset(-2);

        var response = await _client.PostAsJsonAsync(ExpensesUri(vehicleId), new { date, categoryId = category.Id, amount = 12.34m, label = " Lavage " });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = (await response.Content.ReadFromJsonAsync<ExpenseDto>(Json))!;
        Assert.Equal($"/api/vehicles/{vehicleId}/expenses/{created.Id}", response.Headers.Location?.ToString());
        Assert.Equal(category.Name, created.CategoryName);
        Assert.Equal("OneOff", created.Family);
        Assert.Equal("Manual", created.Source);
        Assert.Equal("Lavage", created.Label);
        Assert.Equal(12.34m, created.Amount);

        var stored = Assert.Single(await ListAsync(vehicleId));
        Assert.Equal(created.Id, stored.Id);
        Assert.Equal(date, stored.Date);
        Assert.Equal(12.34m, stored.Amount);
        Assert.Equal(created.RecordedAt, stored.RecordedAt, TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public async Task Accepte_un_montant_nul_et_refuse_un_montant_negatif_ou_trop_precis()
    {
        var vehicleId = await CreateVehicleAsync();
        var category = await CreateCategoryAsync();

        var zero = await _client.PostAsJsonAsync(ExpensesUri(vehicleId), new { date = Offset(-1), categoryId = category.Id, amount = 0 });
        var negative = await _client.PostAsJsonAsync(ExpensesUri(vehicleId), new { date = Offset(-1), categoryId = category.Id, amount = -1 });
        var precise = await _client.PostAsJsonAsync(ExpensesUri(vehicleId), new { date = Offset(-1), categoryId = category.Id, amount = 1.234m });
        var missing = await _client.PostAsJsonAsync(ExpensesUri(vehicleId), new { date = Offset(-1), categoryId = category.Id });

        Assert.Equal(HttpStatusCode.Created, zero.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, negative.StatusCode);
        Assert.Equal("expense.amount_negative", await ErrorCodeAsync(negative));
        Assert.Equal(HttpStatusCode.BadRequest, precise.StatusCode);
        Assert.Equal("expense.amount_too_precise", await ErrorCodeAsync(precise));
        Assert.Equal(HttpStatusCode.BadRequest, missing.StatusCode);
        Assert.Equal("expense.amount_required", await ErrorCodeAsync(missing));
        Assert.Single(await ListAsync(vehicleId));
    }

    [Fact]
    public async Task Refuse_une_date_absente_ou_future()
    {
        var vehicleId = await CreateVehicleAsync();
        var category = await CreateCategoryAsync();

        var future = await _client.PostAsJsonAsync(ExpensesUri(vehicleId), new { date = Offset(2), categoryId = category.Id, amount = 1 });
        var missing = await _client.PostAsJsonAsync(ExpensesUri(vehicleId), new { categoryId = category.Id, amount = 1 });

        Assert.Equal(HttpStatusCode.BadRequest, future.StatusCode);
        Assert.Equal("expense.date_in_future", await ErrorCodeAsync(future));
        Assert.Equal(HttpStatusCode.BadRequest, missing.StatusCode);
        Assert.Equal("expense.date_required", await ErrorCodeAsync(missing));
    }

    [Fact]
    public async Task Refuse_une_categorie_inexistante_inactive_ou_absente()
    {
        var vehicleId = await CreateVehicleAsync();
        var inactive = await CreateCategoryAsync(active: false);

        var unknown = await _client.PostAsJsonAsync(ExpensesUri(vehicleId), new { date = Offset(-1), categoryId = Guid.NewGuid(), amount = 1 });
        var disabled = await _client.PostAsJsonAsync(ExpensesUri(vehicleId), new { date = Offset(-1), categoryId = inactive.Id, amount = 1 });
        var missing = await _client.PostAsJsonAsync(ExpensesUri(vehicleId), new { date = Offset(-1), amount = 1 });

        Assert.Equal(HttpStatusCode.BadRequest, unknown.StatusCode);
        Assert.Equal("expense_category.unknown", await ErrorCodeAsync(unknown));
        Assert.Equal(HttpStatusCode.BadRequest, disabled.StatusCode);
        Assert.Equal("expense_category.inactive", await ErrorCodeAsync(disabled));
        Assert.Equal(HttpStatusCode.BadRequest, missing.StatusCode);
        Assert.Equal("expense.category_required", await ErrorCodeAsync(missing));
        Assert.Empty(await ListAsync(vehicleId));
    }

    [Fact]
    public async Task Refuse_la_saisie_manuelle_de_la_famille_utilisation_courante()
    {
        var vehicleId = await CreateVehicleAsync();
        var category = await CreateCategoryAsync();

        var response = await _client.PostAsJsonAsync(ExpensesUri(vehicleId), new { date = Offset(-1), categoryId = category.Id, amount = 1, family = "Running" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("expense.family_not_manual", await ErrorCodeAsync(response));
        Assert.Empty(await ListAsync(vehicleId));
    }

    [Fact]
    public async Task Refuse_un_libelle_trop_long()
    {
        var vehicleId = await CreateVehicleAsync();
        var category = await CreateCategoryAsync();

        var response = await _client.PostAsJsonAsync(
            ExpensesUri(vehicleId), new { date = Offset(-1), categoryId = category.Id, amount = 1, label = new string('x', 201) });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("expense.label_too_long", await ErrorCodeAsync(response));
    }

    [Fact]
    public async Task Vehicule_inconnu_ou_sorti_du_parc()
    {
        var category = await CreateCategoryAsync();
        var body = new { date = Offset(-1), categoryId = category.Id, amount = 1 };
        var unknownRead = await _client.GetAsync(ExpensesUri(Guid.NewGuid()));
        var unknownWrite = await _client.PostAsJsonAsync(ExpensesUri(Guid.NewGuid()), body);
        var vehicleId = await CreateVehicleAsync();
        var created = await AddExpenseAsync(vehicleId, body);
        var retired = await _client.PutAsJsonAsync(new Uri($"/api/vehicles/{vehicleId}/status", UriKind.Relative), new { status = "Retired" });
        Assert.Equal(HttpStatusCode.OK, retired.StatusCode);

        var afterAdd = await _client.PostAsJsonAsync(ExpensesUri(vehicleId), body);
        var afterDelete = await _client.DeleteAsync(ExpenseUri(vehicleId, created.Id));
        var read = await _client.GetAsync(ExpensesUri(vehicleId));

        Assert.Equal(HttpStatusCode.NotFound, unknownRead.StatusCode);
        Assert.Equal("vehicle.not_found", await ErrorCodeAsync(unknownRead));
        Assert.Equal(HttpStatusCode.NotFound, unknownWrite.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, afterAdd.StatusCode);
        Assert.Equal("vehicle.retired", await ErrorCodeAsync(afterAdd));
        Assert.Equal(HttpStatusCode.Conflict, afterDelete.StatusCode);
        Assert.Equal(HttpStatusCode.OK, read.StatusCode);
    }

    [Fact]
    public async Task La_liste_ne_renvoie_que_le_vehicule_demande_par_date_decroissante()
    {
        var vehicleId = await CreateVehicleAsync();
        var otherId = await CreateVehicleAsync();
        var category = await CreateCategoryAsync();
        var old = await AddExpenseAsync(vehicleId, new { date = Offset(-10), categoryId = category.Id, amount = 1, label = "ancienne" });
        var recent = await AddExpenseAsync(vehicleId, new { date = Offset(-1), categoryId = category.Id, amount = 2, label = "récente" });
        await AddExpenseAsync(otherId, new { date = Offset(-5), categoryId = category.Id, amount = 3 });

        var list = await ListAsync(vehicleId);

        Assert.Equal(2, list.Count);
        Assert.Equal(recent.Id, list[0].Id);
        Assert.Equal(old.Id, list[1].Id);
    }

    [Fact]
    public async Task Modifie_puis_supprime_une_depense()
    {
        var vehicleId = await CreateVehicleAsync();
        var category = await CreateCategoryAsync();
        var other = await CreateCategoryAsync();
        var created = await AddExpenseAsync(vehicleId, new { date = Offset(-3), categoryId = category.Id, amount = 5, label = "avant" });

        var update = await _client.PutAsJsonAsync(
            ExpenseUri(vehicleId, created.Id), new { date = Offset(-2), categoryId = other.Id, amount = 0, label = (string?)null });
        var invalid = await _client.PutAsJsonAsync(ExpenseUri(vehicleId, created.Id), new { date = Offset(-2), categoryId = other.Id, amount = -5 });

        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        var updated = (await update.Content.ReadFromJsonAsync<ExpenseDto>(Json))!;
        Assert.Equal(other.Name, updated.CategoryName);
        Assert.Equal(0m, updated.Amount);
        Assert.Null(updated.Label);
        Assert.Equal(created.RecordedAt, updated.RecordedAt, TimeSpan.FromMilliseconds(1));
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.Equal("expense.amount_negative", await ErrorCodeAsync(invalid));
        Assert.Equal(0m, Assert.Single(await ListAsync(vehicleId)).Amount);

        var delete = await _client.DeleteAsync(ExpenseUri(vehicleId, created.Id));
        var again = await _client.DeleteAsync(ExpenseUri(vehicleId, created.Id));

        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, again.StatusCode);
        Assert.Equal("expense.not_found", await ErrorCodeAsync(again));
        Assert.Empty(await ListAsync(vehicleId));
    }

    [Fact]
    public async Task Une_depense_d_un_autre_vehicule_est_introuvable()
    {
        var vehicleId = await CreateVehicleAsync();
        var otherId = await CreateVehicleAsync();
        var category = await CreateCategoryAsync();
        var created = await AddExpenseAsync(otherId, new { date = Offset(-1), categoryId = category.Id, amount = 1 });

        var update = await _client.PutAsJsonAsync(ExpenseUri(vehicleId, created.Id), new { date = Offset(-1), categoryId = category.Id, amount = 2 });
        var delete = await _client.DeleteAsync(ExpenseUri(vehicleId, created.Id));

        Assert.Equal(HttpStatusCode.NotFound, update.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);
        Assert.Equal(1m, Assert.Single(await ListAsync(otherId)).Amount);
    }

    [Fact]
    public async Task Une_depense_issue_d_un_connecteur_n_est_ni_modifiable_ni_supprimable()
    {
        var vehicleId = await CreateVehicleAsync();
        var category = await CreateCategoryAsync();
        var expenseId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<UsMecanicDbContext>();
            await db.Database.ExecuteSqlAsync(
                $"""
                INSERT INTO expenses (id, vehicle_id, family, date, category_id, amount, label, source, recorded_at)
                VALUES ({expenseId}, {vehicleId}, 'Running', {Offset(-1)}, {category.Id}, 80.5, 'Plein', 'Total', {now})
                """);
        }

        var update = await _client.PutAsJsonAsync(ExpenseUri(vehicleId, expenseId), new { date = Offset(-1), categoryId = category.Id, amount = 1, family = "Running" });
        var delete = await _client.DeleteAsync(ExpenseUri(vehicleId, expenseId));

        Assert.Equal(HttpStatusCode.Conflict, update.StatusCode);
        Assert.Equal("expense.not_manual", await ErrorCodeAsync(update));
        Assert.Equal(HttpStatusCode.Conflict, delete.StatusCode);
        Assert.Equal("expense.not_manual", await ErrorCodeAsync(delete));
        var stored = Assert.Single(await ListAsync(vehicleId));
        Assert.Equal(80.5m, stored.Amount);
        Assert.Equal("Total", stored.Source);
        Assert.Equal("Running", stored.Family);
    }
}
