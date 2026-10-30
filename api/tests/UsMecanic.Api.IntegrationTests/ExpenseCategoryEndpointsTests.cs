using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace UsMecanic.Api.IntegrationTests;

public class ExpenseCategoryEndpointsTests : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly Uri ExpenseCategories = new("/api/expense-categories", UriKind.Relative);

    private readonly HttpClient _client;
    private readonly HttpClient _anonymous;

    public ExpenseCategoryEndpointsTests(ApiFactory factory)
    {
        _client = factory.CreateAuthenticatedClient();
        _anonymous = factory.CreateClient();
    }

    private sealed record ExpenseCategoryDto(Guid Id, string Name, bool IsActive);

    /// <summary>Nom unique : les tests d'une classe partagent la même base.</summary>
    private static string NewName(string prefix) => $"{prefix} {Guid.NewGuid():N}";

    private async Task<ExpenseCategoryDto> CreateExpenseCategoryAsync(string name)
    {
        var response = await _client.PostAsJsonAsync(ExpenseCategories, new { name });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<ExpenseCategoryDto>(Json))!;
    }

    private static async Task<string?> ErrorCodeAsync(HttpResponseMessage response)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return problem.RootElement.GetProperty("code").GetString();
    }

    [Fact]
    public async Task Les_categories_exigent_une_authentification()
    {
        var response = await _anonymous.GetAsync(ExpenseCategories);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Creation_puis_liste_et_renommage()
    {
        var created = await CreateExpenseCategoryAsync($"  {NewName("Nettoyage")} ");
        Assert.True(created.IsActive);
        Assert.StartsWith("Nettoyage ", created.Name, StringComparison.Ordinal);

        var list = await _client.GetFromJsonAsync<List<ExpenseCategoryDto>>(ExpenseCategories, Json);
        Assert.Contains(list!, g => g.Id == created.Id);

        var newName = NewName("Équipement");
        var renamed = await _client.PutAsJsonAsync(new Uri($"/api/expense-categories/{created.Id}", UriKind.Relative), new { name = newName });
        Assert.Equal(HttpStatusCode.OK, renamed.StatusCode);
        Assert.Equal(newName, (await renamed.Content.ReadFromJsonAsync<ExpenseCategoryDto>(Json))!.Name);
    }

    [Fact]
    public async Task Un_nom_vide_ou_en_double_renvoie_un_ProblemDetails()
    {
        var name = NewName("Catégorie");
        await CreateExpenseCategoryAsync(name);

        var empty = await _client.PostAsJsonAsync(ExpenseCategories, new { name = " " });
        var duplicate = await _client.PostAsJsonAsync(ExpenseCategories, new { name = name.ToUpperInvariant() });

        Assert.Equal(HttpStatusCode.BadRequest, empty.StatusCode);
        Assert.Equal("expense_category.name_required", await ErrorCodeAsync(empty));
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.Equal("expense_category.name_already_exists", await ErrorCodeAsync(duplicate));
    }

    [Fact]
    public async Task Un_nom_trop_long_renvoie_un_ProblemDetails()
    {
        var response = await _client.PostAsJsonAsync(ExpenseCategories, new { name = new string('x', 101) });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("expense_category.name_too_long", await ErrorCodeAsync(response));
    }

    [Fact]
    public async Task Renommer_vers_un_nom_existant_ou_une_categorie_inconnue_est_refuse()
    {
        var other = await CreateExpenseCategoryAsync(NewName("Autre"));
        var expenseCategory = await CreateExpenseCategoryAsync(NewName("Catégorie"));

        var duplicate = await _client.PutAsJsonAsync(new Uri($"/api/expense-categories/{expenseCategory.Id}", UriKind.Relative), new { name = other.Name });
        var unknown = await _client.PutAsJsonAsync(new Uri($"/api/expense-categories/{Guid.NewGuid()}", UriKind.Relative), new { name = NewName("X") });

        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.Equal("expense_category.name_already_exists", await ErrorCodeAsync(duplicate));
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
        Assert.Equal("expense_category.not_found", await ErrorCodeAsync(unknown));
    }

    [Fact]
    public async Task Une_categorie_desactivee_n_est_plus_proposee_mais_reste_liste_sur_demande()
    {
        var expenseCategory = await CreateExpenseCategoryAsync(NewName("Ancienne"));

        var deactivated = await _client.PostAsync(new Uri($"/api/expense-categories/{expenseCategory.Id}/deactivate", UriKind.Relative), null);
        Assert.Equal(HttpStatusCode.OK, deactivated.StatusCode);
        Assert.False((await deactivated.Content.ReadFromJsonAsync<ExpenseCategoryDto>(Json))!.IsActive);

        var proposed = await _client.GetFromJsonAsync<List<ExpenseCategoryDto>>(ExpenseCategories, Json);
        var all = await _client.GetFromJsonAsync<List<ExpenseCategoryDto>>(new Uri("/api/expense-categories?includeInactive=true", UriKind.Relative), Json);
        Assert.DoesNotContain(proposed!, g => g.Id == expenseCategory.Id);
        Assert.Contains(all!, g => g.Id == expenseCategory.Id && !g.IsActive);
    }

    [Fact]
    public async Task Desactiver_une_categorie_inconnue_renvoie_introuvable()
    {
        var response = await _client.PostAsync(new Uri($"/api/expense-categories/{Guid.NewGuid()}/deactivate", UriKind.Relative), null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("expense_category.not_found", await ErrorCodeAsync(response));
    }

    [Fact]
    public async Task Une_categorie_ne_peut_pas_etre_supprimee()
    {
        var expenseCategory = await CreateExpenseCategoryAsync(NewName("Permanente"));

        var response = await _client.DeleteAsync(new Uri($"/api/expense-categories/{expenseCategory.Id}", UriKind.Relative));

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public async Task Les_valeurs_de_depart_sont_presentes()
    {
        var list = await _client.GetFromJsonAsync<List<ExpenseCategoryDto>>(ExpenseCategories, Json);

        Assert.Contains(list!, c => c.Name == "Nettoyage");
        Assert.Contains(list!, c => c.Name == "Contrôle technique");
    }
}
