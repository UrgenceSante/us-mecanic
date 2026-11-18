using UsMecanic.Application.Rentals;
using UsMecanic.Application.Vehicles;
using UsMecanic.Domain.Rentals;

namespace UsMecanic.UnitTests.Application.Rentals;

public class VehicleUserAtDateTests
{
    private static readonly Guid Owner = Guid.NewGuid();
    private static readonly Guid Tenant = Guid.NewGuid();

    private static Rental Make(DateOnly start, DateOnly? end) => Rental.Create(Guid.NewGuid(), new RentalEntry(Tenant, start, end)).Value;

    [Fact]
    public void Le_locataire_si_une_location_couvre_la_date()
    {
        var rentals = new[] { Make(new DateOnly(2026, 2, 1), new DateOnly(2026, 3, 31)) };

        var user = VehicleUserAtDate.Resolve(Owner, rentals, new DateOnly(2026, 3, 31));

        Assert.Equal(Tenant, user.CompanyId);
        Assert.Equal(VehicleUserSource.Tenant, user.Source);
    }

    [Fact]
    public void Le_proprietaire_le_lendemain_de_la_fin()
    {
        var rentals = new[] { Make(new DateOnly(2026, 2, 1), new DateOnly(2026, 3, 31)) };

        var user = VehicleUserAtDate.Resolve(Owner, rentals, new DateOnly(2026, 4, 1));

        Assert.Equal(Owner, user.CompanyId);
        Assert.Equal(VehicleUserSource.Owner, user.Source);
    }

    [Fact]
    public void Le_proprietaire_avant_une_location_prevue()
    {
        var rentals = new[] { Make(new DateOnly(2026, 2, 1), null) };

        var user = VehicleUserAtDate.Resolve(Owner, rentals, new DateOnly(2026, 1, 31));

        Assert.Equal(VehicleUserSource.Owner, user.Source);
    }

    [Fact]
    public void Non_renseigne_sans_location_ni_proprietaire()
    {
        var user = VehicleUserAtDate.Resolve(null, [], new DateOnly(2026, 4, 1));

        Assert.Null(user.CompanyId);
        Assert.Equal(VehicleUserSource.NotSet, user.Source);
    }

    [Fact]
    public void Retient_la_bonne_location_parmi_plusieurs()
    {
        var other = Guid.NewGuid();
        var first = Rental.Create(Guid.NewGuid(), new RentalEntry(other, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31))).Value;
        var second = Make(new DateOnly(2026, 2, 1), null);

        Assert.Equal(other, VehicleUserAtDate.Resolve(Owner, [first, second], new DateOnly(2026, 1, 15)).CompanyId);
        Assert.Equal(Tenant, VehicleUserAtDate.Resolve(Owner, [first, second], new DateOnly(2026, 6, 1)).CompanyId);
    }
}
