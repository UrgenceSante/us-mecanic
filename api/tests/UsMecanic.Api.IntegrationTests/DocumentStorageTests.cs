using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UsMecanic.Application.Documents;
using UsMecanic.Domain.Documents;
using UsMecanic.Infrastructure.Persistence;

namespace UsMecanic.Api.IntegrationTests;

/// <summary>Adaptateur PostgreSQL du stockage des documents, sur la base jetable de l'usine d'API.</summary>
public class DocumentStorageTests : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly byte[] Pdf = [.. "%PDF-1.7\n"u8, 0x00, 0xFF, 0x10];

    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public DocumentStorageTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
    }

    private sealed record IdDto(Guid Id);

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

    private Task<Guid> CreateDocumentCategoryAsync() =>
        PostIdAsync("/api/document-categories", new { name = $"Catégorie {Guid.NewGuid():N}" });

    private Task<Guid> CreateInterventionAsync(Guid vehicleId) =>
        PostIdAsync($"/api/vehicles/{vehicleId}/interventions", new { date = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1), work = "Vidange" });

    private async Task<Guid> CreateExpenseAsync(Guid vehicleId)
    {
        var categoryId = await PostIdAsync("/api/expense-categories", new { name = $"Catégorie {Guid.NewGuid():N}" });
        return await PostIdAsync(
            $"/api/vehicles/{vehicleId}/expenses",
            new { date = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1), categoryId, amount = 10 });
    }

    private static DocumentFile PdfFile() => new("facture.pdf", DocumentContentType.Pdf, Pdf.Length);

    /// <summary>Ajoute les métadonnées puis le contenu : une seule validation, dans la même transaction.</summary>
    private async Task<Document> StoreAsync(Document document, byte[] content)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<IDocumentRepository>().Add(document);
        await scope.ServiceProvider.GetRequiredService<IDocumentStorage>().SaveAsync(document.Id, content, CancellationToken.None);
        return document;
    }

    private async Task<int> CountAsync(string table, Guid documentId)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<UsMecanicDbContext>();
        return table == "documents"
            ? await db.Database.SqlQuery<int>($"SELECT count(*)::int AS \"Value\" FROM documents WHERE id = {documentId}").SingleAsync()
            : await db.Database.SqlQuery<int>($"SELECT count(*)::int AS \"Value\" FROM document_contents WHERE document_id = {documentId}").SingleAsync();
    }

    [Fact]
    public async Task Enregistre_puis_relit_le_contenu_et_les_metadonnees()
    {
        var vehicleId = await CreateVehicleAsync();
        var categoryId = await CreateDocumentCategoryAsync();
        var now = DateTimeOffset.UtcNow;
        var document = await StoreAsync(Document.CreateForVehicle(vehicleId, categoryId, "Carte", PdfFile(), now).Value, Pdf);

        await using var scope = _factory.Services.CreateAsyncScope();
        var content = await scope.ServiceProvider.GetRequiredService<IDocumentStorage>().ReadAsync(document.Id, CancellationToken.None);
        var stored = await scope.ServiceProvider.GetRequiredService<IDocumentRepository>().GetByIdAsync(document.Id, CancellationToken.None);

        Assert.Equal(Pdf, content);
        Assert.NotNull(stored);
        Assert.Equal(categoryId, stored.CategoryId);
        Assert.Equal("facture.pdf", stored.FileName);
        Assert.Equal(DocumentContentType.Pdf, stored.ContentType);
        Assert.Equal(Pdf.Length, stored.SizeBytes);
        Assert.InRange((stored.AddedAt - now).Duration(), TimeSpan.Zero, TimeSpan.FromMilliseconds(1));
        Assert.Single(await scope.ServiceProvider.GetRequiredService<IDocumentRepository>().ListByVehicleAsync(vehicleId, CancellationToken.None));
    }

    [Fact]
    public async Task Un_contenu_inexistant_se_lit_null_et_se_supprime_sans_erreur()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var storage = scope.ServiceProvider.GetRequiredService<IDocumentStorage>();

        Assert.Null(await storage.ReadAsync(Guid.NewGuid(), CancellationToken.None));
        await storage.DeleteAsync(Guid.NewGuid(), CancellationToken.None);
    }

    [Fact]
    public async Task Supprime_le_contenu_d_un_document()
    {
        var vehicleId = await CreateVehicleAsync();
        var document = await StoreAsync(Document.CreateForVehicle(vehicleId, await CreateDocumentCategoryAsync(), null, PdfFile(), DateTimeOffset.UtcNow).Value, Pdf);

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<IDocumentStorage>().DeleteAsync(document.Id, CancellationToken.None);
        }

        Assert.Equal(0, await CountAsync("document_contents", document.Id));
        Assert.Equal(1, await CountAsync("documents", document.Id));
    }

    [Fact]
    public async Task Les_metadonnees_n_exposent_ni_ne_chargent_le_contenu()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<UsMecanicDbContext>();

        var documentColumns = db.Model.FindEntityType(typeof(Document))!.GetProperties().Select(p => p.ClrType).ToList();

        Assert.DoesNotContain(typeof(byte[]), documentColumns);
        Assert.Equal("documents", db.Model.FindEntityType(typeof(Document))!.GetTableName());
    }

    [Fact]
    public async Task La_requete_de_liste_des_metadonnees_ne_touche_pas_a_la_table_de_contenu()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<UsMecanicDbContext>();

        var sql = db.Documents.AsNoTracking()
            .Where(d => d.VehicleId == Guid.NewGuid())
            .OrderByDescending(d => d.AddedAt)
            .ToQueryString();

        Assert.Contains("documents", sql, StringComparison.Ordinal);
        Assert.DoesNotContain("document_contents", sql, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Supprimer_une_intervention_supprime_ses_documents_et_leur_contenu()
    {
        var vehicleId = await CreateVehicleAsync();
        var interventionId = await CreateInterventionAsync(vehicleId);
        var document = await StoreAsync(Document.CreateInvoice(vehicleId, interventionId, null, null, PdfFile(), DateTimeOffset.UtcNow).Value, Pdf);

        var response = await _client.DeleteAsync(new Uri($"/api/vehicles/{vehicleId}/interventions/{interventionId}", UriKind.Relative));

        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(0, await CountAsync("documents", document.Id));
        Assert.Equal(0, await CountAsync("document_contents", document.Id));
    }

    [Fact]
    public async Task Supprimer_une_depense_supprime_ses_documents_et_leur_contenu()
    {
        var vehicleId = await CreateVehicleAsync();
        var expenseId = await CreateExpenseAsync(vehicleId);
        var document = await StoreAsync(Document.CreateInvoice(vehicleId, null, expenseId, null, PdfFile(), DateTimeOffset.UtcNow).Value, Pdf);

        var response = await _client.DeleteAsync(new Uri($"/api/vehicles/{vehicleId}/expenses/{expenseId}", UriKind.Relative));

        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(0, await CountAsync("documents", document.Id));
        Assert.Equal(0, await CountAsync("document_contents", document.Id));
    }

    [Fact]
    public async Task La_base_refuse_un_document_sans_rattachement_coherent()
    {
        var vehicleId = await CreateVehicleAsync();
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<UsMecanicDbContext>();

        var insert = () => db.Database.ExecuteSqlAsync(
            $"""
            INSERT INTO documents (id, vehicle_id, file_name, content_type, size_bytes, added_at)
            VALUES ({Guid.NewGuid()}, {vehicleId}, 'a.pdf', 'Pdf', 1, {DateTimeOffset.UtcNow})
            """);

        await Assert.ThrowsAnyAsync<Exception>(insert);
    }
}
