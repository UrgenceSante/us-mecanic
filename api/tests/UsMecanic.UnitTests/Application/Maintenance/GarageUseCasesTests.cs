using UsMecanic.Application.Maintenance.Garages;
using UsMecanic.Domain.Maintenance;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Application.Maintenance;

public class GarageUseCasesTests
{
    private readonly InMemoryGarageRepository _garages = new();

    [Fact]
    public async Task Cree_un_garage_actif()
    {
        var result = await new CreateGarage(_garages).ExecuteAsync(" Garage Martin ", CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Garage Martin", result.Value.Name);
        Assert.True(result.Value.IsActive);
        Assert.Single(_garages.Saved);
    }

    [Fact]
    public async Task Refuse_un_nom_vide_ou_en_double_sans_tenir_compte_de_la_casse()
    {
        _garages.Seed("Garage Martin");
        var useCase = new CreateGarage(_garages);

        var empty = await useCase.ExecuteAsync("  ", CancellationToken.None);
        var duplicate = await useCase.ExecuteAsync("garage MARTIN", CancellationToken.None);

        Assert.Equal(GarageErrors.NameRequired, empty.Error);
        Assert.Equal(GarageErrors.NameAlreadyExists, duplicate.Error);
        Assert.Single(_garages.Saved);
        Assert.Equal(0, _garages.SaveCount);
    }

    [Fact]
    public async Task Refuse_un_nom_trop_long()
    {
        var result = await new CreateGarage(_garages).ExecuteAsync(new string('x', Garage.MaxNameLength + 1), CancellationToken.None);

        Assert.Equal(GarageErrors.NameTooLong, result.Error);
        Assert.Empty(_garages.Saved);
    }

    [Fact]
    public async Task Renomme_un_garage_y_compris_pour_changer_la_casse()
    {
        var garage = _garages.Seed("garage martin");

        var result = await new RenameGarage(_garages).ExecuteAsync(garage.Id, "Garage Martin", CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Garage Martin", garage.Name);
    }

    [Fact]
    public async Task Refuse_de_renommer_vers_un_nom_existant_ou_un_garage_inconnu()
    {
        _garages.Seed("Garage Martin");
        var other = _garages.Seed("Garage Durand");
        var useCase = new RenameGarage(_garages);

        var duplicate = await useCase.ExecuteAsync(other.Id, "garage martin", CancellationToken.None);
        var unknown = await useCase.ExecuteAsync(Guid.NewGuid(), "Garage Leroy", CancellationToken.None);
        var empty = await useCase.ExecuteAsync(other.Id, " ", CancellationToken.None);

        Assert.Equal(GarageErrors.NameAlreadyExists, duplicate.Error);
        Assert.Equal(GarageErrors.NotFound, unknown.Error);
        Assert.Equal(GarageErrors.NameRequired, empty.Error);
        Assert.Equal("Garage Durand", other.Name);
    }

    [Fact]
    public async Task Un_garage_desactive_n_est_plus_propose_mais_reste_liste_sur_demande()
    {
        var garage = _garages.Seed("Garage Martin");

        var deactivated = await new DeactivateGarage(_garages).ExecuteAsync(garage.Id, CancellationToken.None);
        var proposed = await new ListGarages(_garages).ExecuteAsync(false, CancellationToken.None);
        var all = await new ListGarages(_garages).ExecuteAsync(true, CancellationToken.None);

        Assert.False(deactivated.Value.IsActive);
        Assert.Empty(proposed);
        Assert.Equal(garage.Id, Assert.Single(all).Id);
    }

    [Fact]
    public async Task Desactiver_un_garage_inconnu_renvoie_introuvable()
    {
        var result = await new DeactivateGarage(_garages).ExecuteAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(GarageErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task La_liste_est_triee_par_nom_sans_tenir_compte_de_la_casse()
    {
        _garages.Seed("Vidal");
        _garages.Seed("autos Martin");

        var list = await new ListGarages(_garages).ExecuteAsync(false, CancellationToken.None);

        Assert.Equal("autos Martin", list[0].Name);
        Assert.Equal("Vidal", list[1].Name);
    }
}
