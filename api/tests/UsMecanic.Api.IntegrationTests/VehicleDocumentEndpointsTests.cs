using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using UsMecanic.Application.Documents;

namespace UsMecanic.Api.IntegrationTests;

public class VehicleDocumentEndpointsTests : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly byte[] Pdf = [.. "%PDF-1.7\n"u8, 0x00, 0xFF, 0x10];
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x01];
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10];

    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public VehicleDocumentEndpointsTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
    }

    private sealed record IdDto(Guid Id);

    private sealed record DocumentDto(Guid Id, Guid VehicleId, Guid? CategoryId, string? Label, string FileName, string ContentType, long SizeBytes, DateTimeOffset AddedAt);

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

    private Task<Guid> CreateCategoryAsync() =>
        PostIdAsync("/api/document-categories", new { name = $"Catégorie {Guid.NewGuid():N}" });

    private static Uri Url(Guid vehicleId, string suffix = "") => new($"/api/vehicles/{vehicleId}/documents{suffix}", UriKind.Relative);

    private static MultipartFormDataContent Form(byte[]? content, string fileName, Guid? categoryId, string? label = null)
    {
        var form = new MultipartFormDataContent();
        if (content is not null)
        {
            form.Add(new ByteArrayContent(content), "file", fileName);
        }

        if (categoryId is not null)
        {
            form.Add(new StringContent(categoryId.Value.ToString()), "categoryId");
        }

        if (label is not null)
        {
            form.Add(new StringContent(label), "label");
        }

        return form;
    }

    private async Task<DocumentDto> UploadAsync(Guid vehicleId, Guid categoryId, byte[] content, string fileName, string? label = null)
    {
        using var form = Form(content, fileName, categoryId, label);
        var response = await _client.PostAsync(Url(vehicleId), form);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<DocumentDto>(Json))!;
    }

    private static async Task<string?> CodeAsync(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>(Json)).GetProperty("code").GetString();

    [Theory]
    [InlineData("pdf", "application/pdf")]
    [InlineData("png", "image/png")]
    [InlineData("jpg", "image/jpeg")]
    public async Task Ajoute_un_document_puis_relit_un_contenu_identique(string kind, string mime)
    {
        var content = kind switch { "pdf" => Pdf, "png" => Png, _ => Jpeg };
        var vehicleId = await CreateVehicleAsync();
        var categoryId = await CreateCategoryAsync();

        var created = await UploadAsync(vehicleId, categoryId, content, $"scan.{kind}", "Mon libellé");

        Assert.Equal(mime, created.ContentType);
        Assert.Equal(content.Length, created.SizeBytes);
        Assert.Equal("Mon libellé", created.Label);
        var read = await _client.GetAsync(Url(vehicleId, $"/{created.Id}/content"));
        Assert.Equal(HttpStatusCode.OK, read.StatusCode);
        Assert.Equal(mime, read.Content.Headers.ContentType?.MediaType);
        Assert.Equal(SHA256.HashData(content), SHA256.HashData(await read.Content.ReadAsByteArrayAsync()));
        Assert.Equal("nosniff", read.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.StartsWith("inline", read.Content.Headers.ContentDisposition?.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Telecharge_avec_le_nom_d_origine_encode()
    {
        var vehicleId = await CreateVehicleAsync();
        var created = await UploadAsync(vehicleId, await CreateCategoryAsync(), Pdf, "Carte grisé 2026.pdf");

        var read = await _client.GetAsync(Url(vehicleId, $"/{created.Id}/content?download=true"));

        var header = string.Join(';', read.Content.Headers.GetValues("Content-Disposition"));
        Assert.StartsWith("attachment", header, StringComparison.Ordinal);
        Assert.Contains("filename*=UTF-8''Carte%20gris%C3%A9%202026.pdf", header, StringComparison.Ordinal);
        Assert.Equal("Carte grisé 2026.pdf", read.Content.Headers.ContentDisposition?.FileNameStar);
    }

    [Fact]
    public async Task Liste_les_metadonnees_du_plus_recent_au_plus_ancien_avec_filtre()
    {
        var vehicleId = await CreateVehicleAsync();
        var cat1 = await CreateCategoryAsync();
        var cat2 = await CreateCategoryAsync();
        var first = await UploadAsync(vehicleId, cat1, Pdf, "un.pdf");
        await Task.Delay(20);
        var second = await UploadAsync(vehicleId, cat2, Png, "deux.png");

        var all = await _client.GetFromJsonAsync<DocumentDto[]>(Url(vehicleId), Json);
        var filtered = await _client.GetFromJsonAsync<DocumentDto[]>(Url(vehicleId, $"?categoryId={cat1}"), Json);

        Assert.Equal([second.Id, first.Id], all!.Select(d => d.Id));
        Assert.Equal([first.Id], filtered!.Select(d => d.Id));
    }

    [Fact]
    public async Task Refuse_un_vehicule_inconnu_en_404()
    {
        var categoryId = await CreateCategoryAsync();
        using var form = Form(Pdf, "a.pdf", categoryId);

        var response = await _client.PostAsync(Url(Guid.NewGuid()), form);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("vehicle.not_found", await CodeAsync(response));
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync(Url(Guid.NewGuid()))).StatusCode);
    }

    [Fact]
    public async Task Refuse_une_categorie_inconnue_inactive_ou_absente_et_un_libelle_trop_long()
    {
        var vehicleId = await CreateVehicleAsync();
        var inactive = await CreateCategoryAsync();
        Assert.True((await _client.PostAsync(new Uri($"/api/document-categories/{inactive}/deactivate", UriKind.Relative), null)).IsSuccessStatusCode);
        var active = await CreateCategoryAsync();

        using var unknown = Form(Pdf, "a.pdf", Guid.NewGuid());
        using var disabled = Form(Pdf, "a.pdf", inactive);
        using var missing = Form(Pdf, "a.pdf", null);
        using var longLabel = Form(Pdf, "a.pdf", active, new string('x', 201));

        Assert.Equal("document_category.unknown", await CodeAsync(await _client.PostAsync(Url(vehicleId), unknown)));
        Assert.Equal("document_category.inactive", await CodeAsync(await _client.PostAsync(Url(vehicleId), disabled)));
        Assert.Equal("document.category_required", await CodeAsync(await _client.PostAsync(Url(vehicleId), missing)));
        Assert.Equal("document.label_too_long", await CodeAsync(await _client.PostAsync(Url(vehicleId), longLabel)));
    }

    [Fact]
    public async Task Refuse_un_mauvais_type_un_fichier_vide_et_l_absence_de_fichier_en_400()
    {
        var vehicleId = await CreateVehicleAsync();
        var categoryId = await CreateCategoryAsync();

        using var zip = Form("PK\u0003\u0004 contenu"u8.ToArray(), "a.pdf", categoryId);
        using var empty = Form([], "a.pdf", categoryId);
        using var none = Form(null, "a.pdf", categoryId);

        var zipResponse = await _client.PostAsync(Url(vehicleId), zip);
        var emptyResponse = await _client.PostAsync(Url(vehicleId), empty);
        var noneResponse = await _client.PostAsync(Url(vehicleId), none);

        Assert.Equal(HttpStatusCode.BadRequest, zipResponse.StatusCode);
        Assert.Equal("document.file_type_unsupported", await CodeAsync(zipResponse));
        Assert.Equal(HttpStatusCode.BadRequest, emptyResponse.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, noneResponse.StatusCode);
        Assert.Empty((await _client.GetFromJsonAsync<DocumentDto[]>(Url(vehicleId), Json))!);
    }

    [Theory]
    [InlineData(10 * 1024 * 1024 + 1, 400)]
    [InlineData(15 * 1024 * 1024, 400)] // 413 sous Kestrel (limite de requête de l'endpoint), non appliquée par le serveur de test
    public async Task Refuse_un_fichier_trop_gros_sans_jamais_renvoyer_500(int size, int expected)
    {
        var vehicleId = await CreateVehicleAsync();
        var content = new byte[size];
        Pdf.CopyTo(content, 0);
        using var form = Form(content, "gros.pdf", await CreateCategoryAsync());

        var response = await _client.PostAsync(Url(vehicleId), form);

        Assert.True(expected == (int)response.StatusCode, await response.Content.ReadAsStringAsync());
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Accepte_un_fichier_de_10_mo_pile()
    {
        var vehicleId = await CreateVehicleAsync();
        var content = new byte[10 * 1024 * 1024];
        Pdf.CopyTo(content, 0);

        var created = await UploadAsync(vehicleId, await CreateCategoryAsync(), content, "limite.pdf");

        Assert.Equal(content.Length, created.SizeBytes);
    }

    [Fact]
    public async Task Accepte_l_ajout_la_lecture_et_la_suppression_sur_un_vehicule_sorti_du_parc()
    {
        var vehicleId = await CreateVehicleAsync();
        var categoryId = await CreateCategoryAsync();
        var before = await UploadAsync(vehicleId, categoryId, Pdf, "avant.pdf");
        var status = await _client.PutAsJsonAsync(new Uri($"/api/vehicles/{vehicleId}/status", UriKind.Relative), new { status = "Retired" });
        Assert.Equal(HttpStatusCode.OK, status.StatusCode);

        var after = await UploadAsync(vehicleId, categoryId, Pdf, "cession.pdf");

        Assert.Equal(2, (await _client.GetFromJsonAsync<DocumentDto[]>(Url(vehicleId), Json))!.Length);
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync(Url(vehicleId, $"/{after.Id}/content"))).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync(Url(vehicleId, $"/{before.Id}"))).StatusCode);
    }

    [Fact]
    public async Task Supprime_le_document_et_son_contenu()
    {
        var vehicleId = await CreateVehicleAsync();
        var created = await UploadAsync(vehicleId, await CreateCategoryAsync(), Pdf, "a.pdf");

        var deleted = await _client.DeleteAsync(Url(vehicleId, $"/{created.Id}"));

        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync(Url(vehicleId, $"/{created.Id}/content"))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.DeleteAsync(Url(vehicleId, $"/{created.Id}"))).StatusCode);
        Assert.Empty((await _client.GetFromJsonAsync<DocumentDto[]>(Url(vehicleId), Json))!);
        await using var scope = _factory.Services.CreateAsyncScope();
        var storage = scope.ServiceProvider.GetRequiredService<IDocumentStorage>();
        Assert.Null(await storage.ReadAsync(created.Id, CancellationToken.None));
    }

    [Fact]
    public async Task Un_document_n_est_pas_accessible_par_un_autre_vehicule()
    {
        var vehicleId = await CreateVehicleAsync();
        var otherId = await CreateVehicleAsync();
        var created = await UploadAsync(vehicleId, await CreateCategoryAsync(), Pdf, "a.pdf");

        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync(Url(otherId, $"/{created.Id}/content"))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.DeleteAsync(Url(otherId, $"/{created.Id}"))).StatusCode);
    }

    [Fact]
    public async Task Sert_le_contenu_avec_les_en_tetes_de_defense()
    {
        var vehicleId = await CreateVehicleAsync();
        var created = await UploadAsync(vehicleId, await CreateCategoryAsync(), Pdf, "a.pdf");

        var read = await _client.GetAsync(Url(vehicleId, $"/{created.Id}/content"));

        Assert.Equal("no-store, private", read.Headers.CacheControl?.ToString());
        Assert.Equal("default-src 'none'; style-src 'unsafe-inline'", read.Headers.GetValues("Content-Security-Policy").Single());
        Assert.Equal("nosniff", read.Headers.GetValues("X-Content-Type-Options").Single());
    }

    [Theory]
    [InlineData("a\"b.pdf", "a\"b.pdf")] // l'échappement \" de la quoted-string est décodé
    [InlineData("\"x\".pdf", "\"x\".pdf")]
    [InlineData("ligne\u0007\t.pdf", "ligne.pdf")] // caractères de contrôle retirés
    [InlineData("ligne\r\n.pdf", null)] // CRLF casse l'enveloppe multipart : 400 du framework, jamais 500
    public async Task Aller_retour_avec_des_noms_difficiles_sans_jamais_500(string rawName, string? expected)
    {
        var vehicleId = await CreateVehicleAsync();
        var categoryId = await CreateCategoryAsync();
        using var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(Pdf);
        file.Headers.TryAddWithoutValidation("Content-Disposition", $"form-data; name=\"file\"; filename=\"{rawName.Replace("\"", "\\\"", StringComparison.Ordinal)}\"");
        form.Add(file);
        form.Add(new StringContent(categoryId.ToString()), "categoryId");

        var upload = await _client.PostAsync(Url(vehicleId), form);
        if (expected is null)
        {
            Assert.Equal(HttpStatusCode.BadRequest, upload.StatusCode);
            return;
        }

        Assert.Equal(HttpStatusCode.Created, upload.StatusCode);
        var created = (await upload.Content.ReadFromJsonAsync<DocumentDto>(Json))!;
        Assert.Equal(expected, created.FileName);

        var read = await _client.GetAsync(Url(vehicleId, $"/{created.Id}/content?download=true"));

        Assert.Equal(HttpStatusCode.OK, read.StatusCode);
        var disposition = read.Content.Headers.ContentDisposition;
        Assert.Equal(expected, disposition?.FileNameStar ?? disposition?.FileName?.Trim('"'));
    }

    [Fact]
    public async Task Repond_401_sans_authentification()
    {
        using var anonymous = _factory.CreateClient();
        var id = Guid.NewGuid();
        using var form = Form(Pdf, "a.pdf", Guid.NewGuid());

        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync(Url(id))).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.PostAsync(Url(id), form)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync(Url(id, $"/{Guid.NewGuid()}/content"))).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.DeleteAsync(Url(id, $"/{Guid.NewGuid()}"))).StatusCode);
    }
}
