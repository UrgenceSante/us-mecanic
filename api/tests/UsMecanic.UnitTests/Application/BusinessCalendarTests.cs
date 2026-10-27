using UsMecanic.Application;
using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Mileage;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Application;

public class BusinessCalendarTests
{
    // 22 h 30 UTC le 30/09 (heure d'été) = 00 h 30 le 01/10 à Paris.
    private static readonly DateTimeOffset LateEvening = new(2026, 9, 30, 22, 30, 0, TimeSpan.Zero);
    private static readonly DateOnly LocalToday = new(2026, 10, 1);

    private readonly FixedClock _clock = new(LateEvening);

    private static TimeZoneInfo Paris => TimeZoneInfo.FindSystemTimeZoneById(BusinessCalendar.DefaultTimeZoneId);

    [Fact]
    public void La_date_du_jour_suit_le_fuseau_metier_et_non_l_UTC()
    {
        Assert.Equal(LocalToday, _clock.Calendar.Today);
        Assert.Equal(new DateOnly(2026, 9, 30), new BusinessCalendar(_clock, TimeZoneInfo.Utc).Today);
    }

    [Fact]
    public void L_heure_d_hiver_decale_d_une_heure()
    {
        // 23 h 30 UTC le 30/11 = 00 h 30 le 01/12 à Paris (UTC+1).
        var clock = new FixedClock(new DateTimeOffset(2026, 11, 30, 23, 30, 0, TimeSpan.Zero));

        Assert.Equal(new DateOnly(2026, 12, 1), new BusinessCalendar(clock, Paris).Today);
    }

    [Fact]
    public async Task Un_releve_date_du_jour_local_est_accepte_a_00h30_heure_de_Paris()
    {
        var readings = new InMemoryMileageReadingRepository();
        var vehicles = new InMemoryVehicleRepository();
        var vehicle = vehicles.Seed("AB-123-CD");
        var useCase = new AddMileageReading(new MileageVehicleGuard(vehicles), readings, _clock, _clock.Calendar);

        var result = await useCase.ExecuteAsync(vehicle.Id, new MileageReadingInput(LocalToday, 1_000), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Un_releve_date_du_lendemain_local_reste_refuse()
    {
        var readings = new InMemoryMileageReadingRepository();
        var vehicles = new InMemoryVehicleRepository();
        var vehicle = vehicles.Seed("AB-123-CD");
        var useCase = new AddMileageReading(new MileageVehicleGuard(vehicles), readings, _clock, _clock.Calendar);

        var result = await useCase.ExecuteAsync(vehicle.Id, new MileageReadingInput(LocalToday.AddDays(1), 1_000), CancellationToken.None);

        Assert.Equal(MileageReadingErrors.DateInFuture, result.Error);
    }
}
