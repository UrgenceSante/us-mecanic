using UsMecanic.Application.Deadlines;
using UsMecanic.Domain.Deadlines;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Application.Deadlines;

public class DeadlineTypeUseCasesTests
{
    private readonly InMemoryDeadlineTypeRepository _types = new();
    private readonly InMemoryDeadlineThresholdsRepository _thresholds = new();

    [Fact]
    public async Task Cree_un_type_actif_avec_sa_periodicite()
    {
        var result = await new CreateDeadlineType(_types)
            .ExecuteAsync(new DeadlineTypeInput(" Assurance ", 12, null, 30, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Assurance", result.Value.Name);
        Assert.Equal(12, result.Value.DefaultIntervalMonths);
        Assert.False(result.Value.IsOneOff);
        Assert.True(result.Value.IsActive);
        Assert.Single(_types.Saved);
    }

    [Fact]
    public async Task Un_type_sans_periodicite_est_ponctuel()
    {
        var result = await new CreateDeadlineType(_types)
            .ExecuteAsync(new DeadlineTypeInput("Agrément ARS", null, null, null, null), CancellationToken.None);

        Assert.True(result.Value.IsOneOff);
    }

    [Fact]
    public async Task Refuse_un_nom_vide_en_double_ou_une_valeur_negative_sans_rien_enregistrer()
    {
        _types.Seed("Assurance");
        var useCase = new CreateDeadlineType(_types);

        var empty = await useCase.ExecuteAsync(new DeadlineTypeInput(" ", null, null, null, null), CancellationToken.None);
        var duplicate = await useCase.ExecuteAsync(new DeadlineTypeInput("assurance", null, null, null, null), CancellationToken.None);
        var negative = await useCase.ExecuteAsync(new DeadlineTypeInput("Autre", -1, null, null, null), CancellationToken.None);

        Assert.Equal(DeadlineTypeErrors.NameRequired, empty.Error);
        Assert.Equal(DeadlineTypeErrors.NameAlreadyExists, duplicate.Error);
        Assert.Equal(DeadlineTypeErrors.IntervalMonthsInvalid, negative.Error);
        Assert.Single(_types.Saved);
        Assert.Equal(0, _types.SaveCount);
    }

    [Fact]
    public async Task Modifie_un_type_y_compris_pour_changer_la_casse_de_son_nom()
    {
        var type = _types.Seed("extincteur");

        var result = await new UpdateDeadlineType(_types)
            .ExecuteAsync(type.Id, new DeadlineTypeInput("Extincteur", 12, null, null, 100), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Extincteur", type.Name);
        Assert.Equal(12, type.DefaultIntervalMonths);
        Assert.Equal(100, type.DueSoonKm);
    }

    [Fact]
    public async Task Refuse_de_modifier_vers_un_nom_existant_une_valeur_invalide_ou_un_type_inconnu()
    {
        _types.Seed("Assurance");
        var oxygen = _types.Seed("Oxygène");
        var useCase = new UpdateDeadlineType(_types);

        var duplicate = await useCase.ExecuteAsync(oxygen.Id, new DeadlineTypeInput("Assurance", null, null, null, null), CancellationToken.None);
        var invalid = await useCase.ExecuteAsync(oxygen.Id, new DeadlineTypeInput("Oxygène", null, 0, null, null), CancellationToken.None);
        var unknown = await useCase.ExecuteAsync(Guid.NewGuid(), new DeadlineTypeInput("Autre", null, null, null, null), CancellationToken.None);

        Assert.Equal(DeadlineTypeErrors.NameAlreadyExists, duplicate.Error);
        Assert.Equal(DeadlineTypeErrors.IntervalKmInvalid, invalid.Error);
        Assert.Equal(DeadlineTypeErrors.NotFound, unknown.Error);
        Assert.Equal("Oxygène", oxygen.Name);
        Assert.Equal(0, _types.SaveCount);
    }

    [Fact]
    public async Task Un_type_desactive_n_est_plus_propose_mais_reste_listable_sur_demande()
    {
        var type = _types.Seed("Ancien");

        var deactivated = await new DeactivateDeadlineType(_types).ExecuteAsync(type.Id, CancellationToken.None);
        var proposed = await new ListDeadlineTypes(_types).ExecuteAsync(false, CancellationToken.None);
        var all = await new ListDeadlineTypes(_types).ExecuteAsync(true, CancellationToken.None);

        Assert.False(deactivated.Value.IsActive);
        Assert.Empty(proposed);
        Assert.Single(all);
    }

    [Fact]
    public async Task Desactiver_un_type_inconnu_renvoie_introuvable()
    {
        var result = await new DeactivateDeadlineType(_types).ExecuteAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(DeadlineTypeErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task La_liste_est_triee_par_nom_sans_tenir_compte_de_la_casse()
    {
        _types.Seed("Oxygène");
        _types.Seed("assurance");

        var list = await new ListDeadlineTypes(_types).ExecuteAsync(false, CancellationToken.None);

        Assert.Equal("assurance", list[0].Name);
        Assert.Equal("Oxygène", list[1].Name);
    }

    [Fact]
    public async Task Les_seuils_globaux_sont_lus_et_modifies()
    {
        var initial = await new GetDeadlineThresholds(_thresholds).ExecuteAsync(CancellationToken.None);
        var updated = await new UpdateDeadlineThresholds(_thresholds).ExecuteAsync(60, 2000, CancellationToken.None);

        Assert.Equal(new DeadlineThresholdsDto(30, 1000), initial);
        Assert.Equal(new DeadlineThresholdsDto(60, 2000), updated.Value);
        Assert.Equal(1, _thresholds.SaveCount);
    }

    [Fact]
    public async Task Des_seuils_invalides_sont_refuses_sans_rien_enregistrer()
    {
        var useCase = new UpdateDeadlineThresholds(_thresholds);

        var negativeDays = await useCase.ExecuteAsync(-1, 1000, CancellationToken.None);
        var zeroKm = await useCase.ExecuteAsync(30, 0, CancellationToken.None);

        Assert.Equal(DeadlineThresholdsErrors.DaysInvalid, negativeDays.Error);
        Assert.Equal(DeadlineThresholdsErrors.KmInvalid, zeroKm.Error);
        Assert.Equal(0, _thresholds.SaveCount);
        Assert.Equal(30, _thresholds.Thresholds.DueSoonDays);
    }
}
