using UsMecanic.Domain.Mileage;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Domain.Mileage;

public class CurrentMileageRuleTests
{
    private static readonly Guid VehicleId = Guid.NewGuid();

    private static MileageReading Reading(DateOnly date, int km) =>
        MileageReading.Create(VehicleId, new MileageReadingEntry(date, km, "test", ConfirmHighMileage: true), [], FixedClock.Default).Value;

    [Fact]
    public void Sans_releve_il_n_y_a_pas_de_km_actuel()
    {
        Assert.Null(CurrentMileageRule.Select([]));
    }

    [Fact]
    public void Le_releve_le_plus_recent_par_date_l_emporte_meme_avec_moins_de_km()
    {
        var older = Reading(new DateOnly(2026, 9, 1), 20_000);
        var newer = Reading(new DateOnly(2026, 9, 20), 19_000);

        Assert.Same(newer, CurrentMileageRule.Select([older, newer]));
        Assert.Same(newer, CurrentMileageRule.Select([newer, older]));
    }

    [Fact]
    public void A_date_egale_le_releve_le_plus_eleve_l_emporte()
    {
        var date = new DateOnly(2026, 9, 20);
        var low = Reading(date, 10_000);
        var high = Reading(date, 10_050);

        Assert.Same(high, CurrentMileageRule.Select([high, low]));
        Assert.Same(high, CurrentMileageRule.Select([low, high]));
    }
}
