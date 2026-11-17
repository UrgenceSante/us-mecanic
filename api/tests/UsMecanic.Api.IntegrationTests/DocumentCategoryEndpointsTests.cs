using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace UsMecanic.Api.IntegrationTests;

public class DocumentCategoryEndpointsTests : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly Uri DocumentCategories = new("/api/document-categories", UriKind.Relative);

    private readonly HttpClient _client;
    private readonly HttpClient _anonymous;

    public DocumentCategoryEndpointsTests(ApiFactory factory)
    {
        _client = factory.CreateAuthenticatedClient();
        _anonymous = factory.CreateClient();
    }

    private sealed record DocumentCategoryDto(Guid Id, string Name, bool IsActive);

    /// <summary>Nom unique : les tests d'une classe partagent la même base.</summary>
    private static string NewName(string prefix) => $"{prefix} {Guid.NewGuid():N}";

    private async Task<DocumentCategoryDto> CreateDocumentCategoryAsync(string name)
    {
        var response = await _client.PostAsJsonAsync(DocumentCategories, new { name });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<DocumentCategoryDto>(Json))!;
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
        var response = await _anonymous.GetAsync(DocumentCategories);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Creation_puis_liste_et_renommage()
    {
        var created = await CreateDocumentCategoryAsync($"  {NewName("Nettoyage")} ");
        Assert.True(created.IsActive);
        Assert.StartsWith("Nettoyage ", created.Name, StringComparison.Ordinal);

        var list = await _client.GetFromJsonAsync<List<DocumentCategoryDto>>(DocumentCategories, Json);
        Assert.Contains(list!, g => g.Id == created.Id);

        var newName = NewName("Équipement");
        var renamed = await _client.PutAsJsonAsync(new Uri($"/api/document-categories/{created.Id}", UriKind.Relative), new { name = newName });
        Assert.Equal(HttpStatusCode.OK, renamed.StatusCode);
        Assert.Equal(newName, (await renamed.Content.ReadFromJsonAsync<DocumentCategoryDto>(Json))!.Name);
    }

    [Fact]
    public async Task Un_nom_vide_ou_en_double_renvoie_un_ProblemDetails()
    {
        var name = NewName("Catégorie");
        await CreateDocumentCategoryAsync(name);

        var empty = await _client.PostAsJsonAsync(DocumentCategories, new { name = " " });
        var duplicate = await _client.PostAsJsonAsync(DocumentCategories, new { name = name.ToUpperInvariant() });

        Assert.Equal(HttpStatusCode.BadRequest, empty.StatusCode);
        Assert.Equal("document_category.name_required", await ErrorCodeAsync(empty));
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.Equal("document_category.name_already_exists", await ErrorCodeAsync(duplicate));
    }

    [Fact]
    public async Task Un_nom_trop_long_renvoie_un_ProblemDetails()
    {
        var response = await _client.PostAsJsonAsync(DocumentCategories, new { name = new string('x', 101) });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("document_category.name_too_long", await ErrorCodeAsync(response));
    }

    [Fact]
    public async Task Renommer_vers_un_nom_existant_ou_une_categorie_inconnue_est_refuse()
    {
        var other = await CreateDocumentCategoryAsync(NewName("Autre"));
        var documentCategory = await CreateDocumentCategoryAsync(NewName("Catégorie"));

        var duplicate = await _client.PutAsJsonAsync(new Uri($"/api/document-categories/{documentCategory.Id}", UriKind.Relative), new { name = other.Name });
        var unknown = await _client.PutAsJsonAsync(new Uri($"/api/document-categories/{Guid.NewGuid()}", UriKind.Relative), new { name = NewName("X") });

        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.Equal("document_category.name_already_exists", await ErrorCodeAsync(duplicate));
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
        Assert.Equal("document_category.not_found", await ErrorCodeAsync(unknown));
    }

    [Fact]
    public async Task Une_categorie_desactivee_n_est_plus_proposee_mais_reste_liste_sur_demande()
    {
        var documentCategory = await CreateDocumentCategoryAsync(NewName("Ancienne"));

        var deactivated = await _client.PostAsync(new Uri($"/api/document-categories/{documentCategory.Id}/deactivate", UriKind.Relative), null);
        Assert.Equal(HttpStatusCode.OK, deactivated.StatusCode);
        Assert.False((await deactivated.Content.ReadFromJsonAsync<DocumentCategoryDto>(Json))!.IsActive);

        var proposed = await _client.GetFromJsonAsync<List<DocumentCategoryDto>>(DocumentCategories, Json);
        var all = await _client.GetFromJsonAsync<List<DocumentCategoryDto>>(new Uri("/api/document-categories?includeInactive=true", UriKind.Relative), Json);
        Assert.DoesNotContain(proposed!, g => g.Id == documentCategory.Id);
        Assert.Contains(all!, g => g.Id == documentCategory.Id && !g.IsActive);
    }

    [Fact]
    public async Task Desactiver_une_categorie_inconnue_renvoie_introuvable()
    {
        var response = await _client.PostAsync(new Uri($"/api/document-categories/{Guid.NewGuid()}/deactivate", UriKind.Relative), null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("document_category.not_found", await ErrorCodeAsync(response));
    }

    [Fact]
    public async Task Une_categorie_ne_peut_pas_etre_supprimee()
    {
        var documentCategory = await CreateDocumentCategoryAsync(NewName("Permanente"));

        var response = await _client.DeleteAsync(new Uri($"/api/document-categories/{documentCategory.Id}", UriKind.Relative));

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public async Task Les_dix_valeurs_de_depart_sont_presentes()
    {
        var list = await _client.GetFromJsonAsync<List<DocumentCategoryDto>>(DocumentCategories, Json);

        string[] expected =
        [
            "Carte grise",
            "Attestation d'assurance",
            "Procès-verbal de contrôle technique",
            "Agrément ARS",
            "Attestation de désinfection",
            "Attestation de conformité funéraire",
            "Qualification thermique (PSL)",
            "Photo",
            "Contrat de location ou de crédit",
            "Autre",
        ];
        foreach (var name in expected)
        {
            Assert.Contains(list!, c => c.Name == name && c.IsActive);
        }
    }
}
