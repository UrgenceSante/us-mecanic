using UsMecanic.Domain.Deadlines;

namespace UsMecanic.UnitTests.Domain.Deadlines;

public class DeadlineTypeTests
{
    private static readonly DeadlineTypeSettings None = new(null, null, null, null);

    [Fact]
    public void Create_nettoie_le_nom_et_cree_un_type_actif()
    {
        var result = DeadlineType.Create("  Assurance ", new DeadlineTypeSettings(12, null, 45, 500));

        Assert.True(result.IsSuccess);
        Assert.Equal("Assurance", result.Value.Name);
        Assert.True(result.Value.IsActive);
        Assert.Equal(12, result.Value.DefaultIntervalMonths);
        Assert.Equal(45, result.Value.DueSoonDays);
        Assert.Equal(500, result.Value.DueSoonKm);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
    }

    [Fact]
    public void Un_type_sans_periodicite_est_ponctuel()
    {
        var oneOff = DeadlineType.Create("Agrément ARS", None).Value;
        var byMonths = DeadlineType.Create("Contrôle technique", new DeadlineTypeSettings(12, null, null, null)).Value;
        var byKm = DeadlineType.Create("Vidange", new DeadlineTypeSettings(null, 30000, null, null)).Value;

        Assert.True(oneOff.IsOneOff);
        Assert.False(byMonths.IsOneOff);
        Assert.False(byKm.IsOneOff);
    }

    [Fact]
    public void Create_sans_parametres_donne_un_type_ponctuel()
    {
        var result = DeadlineType.Create("Oxygène", null);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsOneOff);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_refuse_un_nom_vide(string? name)
    {
        Assert.Equal(DeadlineTypeErrors.NameRequired, DeadlineType.Create(name, None).Error);
    }

    [Fact]
    public void Create_refuse_un_nom_trop_long()
    {
        var result = DeadlineType.Create(new string('x', DeadlineType.MaxNameLength + 1), None);

        Assert.Equal(DeadlineTypeErrors.NameTooLong, result.Error);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_refuse_une_periodicite_ou_un_seuil_nul_ou_negatif(int value)
    {
        Assert.Equal(DeadlineTypeErrors.IntervalMonthsInvalid, DeadlineType.Create("T", new DeadlineTypeSettings(value, null, null, null)).Error);
        Assert.Equal(DeadlineTypeErrors.IntervalKmInvalid, DeadlineType.Create("T", new DeadlineTypeSettings(null, value, null, null)).Error);
        Assert.Equal(DeadlineTypeErrors.DueSoonDaysInvalid, DeadlineType.Create("T", new DeadlineTypeSettings(null, null, value, null)).Error);
        Assert.Equal(DeadlineTypeErrors.DueSoonKmInvalid, DeadlineType.Create("T", new DeadlineTypeSettings(null, null, null, value)).Error);
    }

    [Fact]
    public void Update_remplace_nom_periodicite_et_seuils()
    {
        var type = DeadlineType.Create("CT", new DeadlineTypeSettings(12, null, 20, null)).Value;
        var id = type.Id;

        var result = type.Update(" Contrôle technique ", new DeadlineTypeSettings(null, 10000, null, 300));

        Assert.True(result.IsSuccess);
        Assert.Equal("Contrôle technique", type.Name);
        Assert.Null(type.DefaultIntervalMonths);
        Assert.Equal(10000, type.DefaultIntervalKm);
        Assert.Null(type.DueSoonDays);
        Assert.Equal(300, type.DueSoonKm);
        Assert.Equal(id, type.Id);
    }

    [Fact]
    public void Update_invalide_ne_modifie_rien()
    {
        var type = DeadlineType.Create("CT", new DeadlineTypeSettings(12, null, null, null)).Value;

        var result = type.Update("Autre", new DeadlineTypeSettings(0, null, null, null));

        Assert.Equal(DeadlineTypeErrors.IntervalMonthsInvalid, result.Error);
        Assert.Equal("CT", type.Name);
        Assert.Equal(12, type.DefaultIntervalMonths);
    }

    [Fact]
    public void Deactivate_desactive_le_type()
    {
        var type = DeadlineType.Create("Extincteur", None).Value;

        type.Deactivate();

        Assert.False(type.IsActive);
    }
}
