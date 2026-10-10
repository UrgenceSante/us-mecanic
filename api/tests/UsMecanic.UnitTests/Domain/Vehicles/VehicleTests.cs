using UsMecanic.Domain.Vehicles;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Domain.Vehicles;

public class VehicleTests
{
    private static readonly LicensePlate Plate = LicensePlate.Create("AB-123-CD").Value;
    private static readonly DateTimeOffset Now = FixedClock.Default;

    private static Vehicle Register(VehicleDetails? details = null) =>
        Vehicle.Register(Plate, details ?? new VehicleDetails("Renault", "Master", new DateOnly(2022, 3, 1), null, "AMB-12"), Now).Value;

    [Fact]
    public void Register_cree_un_vehicule_actif_aux_champs_nettoyes()
    {
        var vehicle = Vehicle.Register(Plate, new VehicleDetails("  Renault ", " ", null, null, null), Now).Value;

        Assert.Equal(VehicleStatus.Active, vehicle.Status);
        Assert.Equal("Renault", vehicle.Brand);
        Assert.Null(vehicle.Model);
        Assert.Equal(Now, vehicle.CreatedAt);
        Assert.Equal(Now, vehicle.UpdatedAt);
        Assert.NotEqual(Guid.Empty, vehicle.Id);
    }

    [Fact]
    public void Register_refuse_une_mise_en_circulation_future()
    {
        var details = new VehicleDetails(null, null, DateOnly.FromDateTime(Now.UtcDateTime).AddDays(1), null, null);

        Assert.Equal(VehicleErrors.FirstRegistrationInFuture, Vehicle.Register(Plate, details, Now).Error);
    }

    [Fact]
    public void Register_refuse_un_texte_trop_long()
    {
        var details = new VehicleDetails(new string('x', Vehicle.MaxTextLength + 1), null, null, null, null);

        Assert.Equal(VehicleErrors.FieldTooLong, Vehicle.Register(Plate, details, Now).Error);
    }

    [Fact]
    public void UpdateDetails_sans_changement_ne_touche_pas_la_date_de_modification()
    {
        var vehicle = Register();

        vehicle.UpdateDetails(vehicle.Details with { Brand = " Renault " }, Now.AddDays(1));

        Assert.Equal(Now, vehicle.UpdatedAt);
    }

    [Fact]
    public void UpdateDetails_modifie_les_caracteristiques()
    {
        var vehicle = Register();
        var later = Now.AddDays(1);

        var result = vehicle.UpdateDetails(vehicle.Details with { Model = "Trafic" }, later);

        Assert.True(result.IsSuccess);
        Assert.Equal("Trafic", vehicle.Model);
        Assert.Equal(later, vehicle.UpdatedAt);
    }

    [Fact]
    public void Un_vehicule_sorti_du_parc_ne_peut_plus_etre_modifie_ni_remis_en_service()
    {
        var vehicle = Register();
        vehicle.ChangeStatus(VehicleStatus.Retired, Now);

        Assert.Equal(VehicleErrors.Retired, vehicle.UpdateDetails(vehicle.Details with { Model = "X" }, Now).Error);
        Assert.Equal(VehicleErrors.Retired, vehicle.ChangeStatus(VehicleStatus.Active, Now).Error);
        Assert.True(vehicle.ChangeStatus(VehicleStatus.Retired, Now).IsSuccess);
    }

    [Fact]
    public void ChangeStatus_immobilise_puis_remet_en_service()
    {
        var vehicle = Register();

        vehicle.ChangeStatus(VehicleStatus.Immobilized, Now.AddHours(1));
        Assert.Equal(VehicleStatus.Immobilized, vehicle.Status);

        vehicle.ChangeStatus(VehicleStatus.Active, Now.AddHours(2));
        Assert.Equal(VehicleStatus.Active, vehicle.Status);
        Assert.Equal(Now.AddHours(2), vehicle.UpdatedAt);
    }
}
