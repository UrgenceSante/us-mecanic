using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using UsMecanic.Application.Documents;

namespace UsMecanic.Api.IntegrationTests;

public class InvoiceEndpointsTests : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly byte[] Pdf = [.. "%PDF-1.7\n"u8, 0x00, 0xFF, 0x10];
    private static readonly DateOnly Day = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-3);

    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public InvoiceEndpointsTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
    }

    private sealed record IdDto(Guid Id);

    private sealed record InvoiceDto(
        Guid Id, Guid? CategoryId, Guid? InterventionId, Guid? ExpenseId, string? Label, string FileName, string? AttachmentLabel, DateOnly? AttachmentDate);

    private sealed record CountDto(Guid Id, int DocumentCount);

    private static string NewPlate()
    {
        var n = Random.Shared.Next(0, 26 * 26 * 1000 * 26 * 26);
        static char L(int i) => (char)('A' + (i % 26));
        return $"{L(n)}{L(n / 26)}-{n / 676 % 1000:000}-{L(n / 676000)}{L(n / 17576000)}";
    }

    private async Task<Guid> PostIdAsync(string uri, object body)
    {
        var response = await _client.PostAsJsonAsync(new Uri(uri, UriKind.Relative), body);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<IdDto>(Json))!.Id;
    }

    private Task<Guid> CreateVehicleAsync() =>
        PostIdAsync("/api/vehicles", new { licensePlate = NewPlate(), details = new { brand = "Renault" } });

    private Task<Guid> CreateInterventionAsync(Guid vehicleId) =>
        PostIdAsync($"/api/vehicles/{vehicleId}/interventions", new { date = Day, work = "Vidange" });

    private async Task<Guid> CreateExpenseAsync(Guid vehicleId)
    {
        var categoryId = await PostIdAsync("/api/expense-categories", new { name = $"Catégorie {Guid.NewGuid():N}" });
        return await PostIdAsync($"/api/vehicles/{vehicleId}/expenses", new { date = Day, categoryId, amount = 42.5m });
    }

    private static Uri Invoices(Guid vehicleId, string kind, Guid parentId) =>
        new($"/api/vehicles/{vehicleId}/{kind}/{parentId}/invoices", UriKind.Relative);

    private static MultipartFormDataContent Form(byte[]? content, string fileName = "facture.pdf", string? label = null)
    {
        var form = new MultipartFormDataContent();
        if (content is not null)
        {
            form.Add(new ByteArrayContent(content), "file", fileName);
        }

        if (label is not null)
        {
            form.Add(new StringContent(label), "label");
        }

        return form;
    }

    private async Task<InvoiceDto> UploadAsync(Uri uri, byte[] content, string? label = null)
    {
        using var form = Form(content, label: label);
        var response = await _client.PostAsync(uri, form);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<InvoiceDto>(Json))!;
    }

    private static async Task<string?> CodeAsync(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>(Json)).GetProperty("code").GetString();

    private async Task<byte[]?> StoredContentAsync(Guid documentId)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IDocumentStorage>().ReadAsync(documentId, CancellationToken.None);
    }

    [Fact]
    public async Task Ajoute_plusieurs_factures_a_une_intervention_puis_les_liste_et_les_ouvre()
    {
        var vehicleId = await CreateVehicleAsync();
        var interventionId = await CreateInterventionAsync(vehicleId);
        var uri = Invoices(vehicleId, "interventions", interventionId);

        var first = await UploadAsync(uri, Pdf, " Facture garage ");
        var second = await UploadAsync(uri, Pdf);
        var list = (await _client.GetFromJsonAsync<InvoiceDto[]>(uri, Json))!;
        var content = await _client.GetAsync(new Uri($"/api/vehicles/{vehicleId}/documents/{first.Id}/content", UriKind.Relative));

        Assert.Equal(interventionId, first.InterventionId);
        Assert.Null(first.CategoryId);
        Assert.Equal("Facture garage", first.Label);
        Assert.Equal(2, list.Length);
        Assert.Contains(list, d => d.Id == second.Id);
        Assert.Equal(HttpStatusCode.OK, content.StatusCode);
        Assert.Equal(Pdf, await content.Content.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task Ajoute_une_facture_a_une_depense_ponctuelle()
    {
        var vehicleId = await CreateVehicleAsync();
        var expenseId = await CreateExpenseAsync(vehicleId);
        var uri = Invoices(vehicleId, "expenses", expenseId);

        var created = await UploadAsync(uri, Pdf);
        var list = (await _client.GetFromJsonAsync<InvoiceDto[]>(uri, Json))!;

        Assert.Equal(expenseId, created.ExpenseId);
        Assert.Single(list);
    }

    [Fact]
    public async Task Refuse_un_rattachement_inconnu_ou_d_un_autre_vehicule()
    {
        var vehicleId = await CreateVehicleAsync();
        var otherId = await CreateVehicleAsync();
        var interventionId = await CreateInterventionAsync(otherId);
        var expenseId = await CreateExpenseAsync(otherId);

        foreach (var (kind, id, code) in new[]
        {
            ("interventions", interventionId, "intervention.not_found"),
            ("interventions", Guid.NewGuid(), "intervention.not_found"),
            ("expenses", expenseId, "expense.not_found"),
            ("expenses", Guid.NewGuid(), "expense.not_found"),
        })
        {
            using var form = Form(Pdf);
            var post = await _client.PostAsync(Invoices(vehicleId, kind, id), form);
            var get = await _client.GetAsync(Invoices(vehicleId, kind, id));

            Assert.Equal(HttpStatusCode.NotFound, post.StatusCode);
            Assert.Equal(code, await CodeAsync(post));
            Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
        }
    }

    [Fact]
    public async Task Refuse_un_fichier_absent_vide_ou_d_un_format_non_accepte()
    {
        var vehicleId = await CreateVehicleAsync();
        var uri = Invoices(vehicleId, "interventions", await CreateInterventionAsync(vehicleId));

        using var missing = Form(null, label: "sans fichier");
        using var empty = Form([]);
        using var zip = Form([0x50, 0x4B, 0x03, 0x04]);
        using var big = Form(new byte[(10 * 1024 * 1024) + 1]);

        Assert.Equal("document.file_required", await CodeAsync(await _client.PostAsync(uri, missing)));
        Assert.Equal("document.file_empty", await CodeAsync(await _client.PostAsync(uri, empty)));
        Assert.Equal("document.file_type_unsupported", await CodeAsync(await _client.PostAsync(uri, zip)));
        Assert.Equal("document.file_too_large", await CodeAsync(await _client.PostAsync(uri, big)));
    }

    [Fact]
    public async Task Les_listes_exposent_le_nombre_de_documents()
    {
        var vehicleId = await CreateVehicleAsync();
        var withTwo = await CreateInterventionAsync(vehicleId);
        var without = await CreateInterventionAsync(vehicleId);
        var expenseId = await CreateExpenseAsync(vehicleId);
        await UploadAsync(Invoices(vehicleId, "interventions", withTwo), Pdf);
        await UploadAsync(Invoices(vehicleId, "interventions", withTwo), Pdf);
        await UploadAsync(Invoices(vehicleId, "expenses", expenseId), Pdf);

        var interventions = (await _client.GetFromJsonAsync<CountDto[]>(new Uri($"/api/vehicles/{vehicleId}/interventions", UriKind.Relative), Json))!
            .ToDictionary(d => d.Id, d => d.DocumentCount);
        var expenses = (await _client.GetFromJsonAsync<CountDto[]>(new Uri($"/api/vehicles/{vehicleId}/expenses", UriKind.Relative), Json))!
            .ToDictionary(d => d.Id, d => d.DocumentCount);

        Assert.Equal(2, interventions[withTwo]);
        Assert.Equal(0, interventions[without]);
        Assert.Equal(1, expenses[expenseId]);
    }

    [Fact]
    public async Task Les_documents_du_vehicule_incluent_les_factures_avec_leur_rattachement()
    {
        var vehicleId = await CreateVehicleAsync();
        var interventionId = await CreateInterventionAsync(vehicleId);
        var expenseId = await CreateExpenseAsync(vehicleId);
        var forIntervention = await UploadAsync(Invoices(vehicleId, "interventions", interventionId), Pdf);
        var forExpense = await UploadAsync(Invoices(vehicleId, "expenses", expenseId), Pdf);
        var date = Day.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

        var all = (await _client.GetFromJsonAsync<InvoiceDto[]>(new Uri($"/api/vehicles/{vehicleId}/documents", UriKind.Relative), Json))!;
        var invoicesOnly = (await _client.GetFromJsonAsync<InvoiceDto[]>(
            new Uri($"/api/vehicles/{vehicleId}/documents?invoicesOnly=true", UriKind.Relative), Json))!;

        Assert.Equal($"Facture — intervention du {date}", all.Single(d => d.Id == forIntervention.Id).AttachmentLabel);
        Assert.Equal($"Facture — dépense du {date}", all.Single(d => d.Id == forExpense.Id).AttachmentLabel);
        Assert.Equal(Day, all.Single(d => d.Id == forExpense.Id).AttachmentDate);
        Assert.Equal(2, invoicesOnly.Length);
    }

    [Fact]
    public async Task Supprimer_une_facture_passe_par_les_documents_du_vehicule()
    {
        var vehicleId = await CreateVehicleAsync();
        var interventionId = await CreateInterventionAsync(vehicleId);
        var created = await UploadAsync(Invoices(vehicleId, "interventions", interventionId), Pdf);

        var deleted = await _client.DeleteAsync(new Uri($"/api/vehicles/{vehicleId}/documents/{created.Id}", UriKind.Relative));

        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        Assert.Empty((await _client.GetFromJsonAsync<InvoiceDto[]>(Invoices(vehicleId, "interventions", interventionId), Json))!);
        Assert.Null(await StoredContentAsync(created.Id));
    }

    [Fact]
    public async Task Supprimer_une_intervention_supprime_ses_documents_et_leur_contenu()
    {
        var vehicleId = await CreateVehicleAsync();
        var interventionId = await CreateInterventionAsync(vehicleId);
        var kept = await CreateInterventionAsync(vehicleId);
        var first = await UploadAsync(Invoices(vehicleId, "interventions", interventionId), Pdf);
        var second = await UploadAsync(Invoices(vehicleId, "interventions", interventionId), Pdf);
        var survivor = await UploadAsync(Invoices(vehicleId, "interventions", kept), Pdf);

        var deleted = await _client.DeleteAsync(new Uri($"/api/vehicles/{vehicleId}/interventions/{interventionId}", UriKind.Relative));

        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        var remaining = (await _client.GetFromJsonAsync<InvoiceDto[]>(new Uri($"/api/vehicles/{vehicleId}/documents", UriKind.Relative), Json))!;
        Assert.Equal([survivor.Id], remaining.Select(d => d.Id));
        Assert.Null(await StoredContentAsync(first.Id));
        Assert.Null(await StoredContentAsync(second.Id));
        Assert.NotNull(await StoredContentAsync(survivor.Id));
    }

    [Fact]
    public async Task Supprimer_une_depense_supprime_ses_documents_et_leur_contenu()
    {
        var vehicleId = await CreateVehicleAsync();
        var expenseId = await CreateExpenseAsync(vehicleId);
        var invoice = await UploadAsync(Invoices(vehicleId, "expenses", expenseId), Pdf);

        var deleted = await _client.DeleteAsync(new Uri($"/api/vehicles/{vehicleId}/expenses/{expenseId}", UriKind.Relative));

        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        Assert.Empty((await _client.GetFromJsonAsync<InvoiceDto[]>(new Uri($"/api/vehicles/{vehicleId}/documents", UriKind.Relative), Json))!);
        Assert.Null(await StoredContentAsync(invoice.Id));
    }

    [Fact]
    public async Task Les_endpoints_de_factures_sont_authentifies()
    {
        using var anonymous = _factory.CreateClient();
        var uri = Invoices(Guid.NewGuid(), "interventions", Guid.NewGuid());
        using var form = Form(Pdf);

        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync(uri)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.PostAsync(uri, form)).StatusCode);
    }
}
