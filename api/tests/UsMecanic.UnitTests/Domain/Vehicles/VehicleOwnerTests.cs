using UsMecanic.Domain.Vehicles;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Domain.Vehicles;

public class VehicleOwnerTests
{
    private static readonly LicensePlate Plate = LicensePlate.Create("AB-123-CD").Value;
    private static readonly DateTimeOffset Now = FixedClock.Default;
    private static readonly DateOnly Today = FixedClock.DefaultToday;
    private static readonly Guid CompanyA = Guid.CreateVersion7();
    private static readonly Guid CompanyB = Guid.CreateVersion7();

    private static Vehicle Register(Guid? owner = null) =>
        Vehicle.Register(Plate, new VehicleDetails("Renault", null, null, null, null, null, owner), Today, Now).Value;

    [Fact]
    public void Un_vehicule_peut_etre_cree_sans_proprietaire()
    {
        Assert.Null(Register().OwnerCompanyId);
    }

    [Fact]
    public void Le_proprietaire_peut_etre_renseigne_puis_change()
    {
        var vehicle = Register();

        Assert.True(vehicle.UpdateDetails(vehicle.Details with { OwnerCompanyId = CompanyA }, Today, Now).IsSuccess);
        Assert.Equal(CompanyA, vehicle.OwnerCompanyId);
        Assert.True(vehicle.UpdateDetails(vehicle.Details with { OwnerCompanyId = CompanyB }, Today, Now).IsSuccess);
        Assert.Equal(CompanyB, vehicle.OwnerCompanyId);
    }

    [Fact]
    public void Le_proprietaire_renseigne_ne_peut_pas_etre_vide()
    {
        var vehicle = Register(CompanyA);

        var result = vehicle.UpdateDetails(vehicle.Details with { OwnerCompanyId = null }, Today, Now);

        Assert.Equal(VehicleErrors.OwnerCannotBeRemoved, result.Error);
        Assert.Equal(CompanyA, vehicle.OwnerCompanyId);
    }

    [Fact]
    public void Vehicule_sorti_du_parc_sans_proprietaire_accepte_la_saisie_du_proprietaire_seul()
    {
        var vehicle = Register();
        vehicle.ChangeStatus(VehicleStatus.Retired, Now);
        var later = Now.AddDays(1);

        var result = vehicle.UpdateDetails(vehicle.Details with { OwnerCompanyId = CompanyA }, Today, later);

        Assert.True(result.IsSuccess);
        Assert.Equal(CompanyA, vehicle.OwnerCompanyId);
        Assert.Equal(later, vehicle.UpdatedAt);
        Assert.Equal(VehicleStatus.Retired, vehicle.Status);
    }

    [Fact]
    public void Vehicule_sorti_du_parc_refuse_un_autre_champ_modifie_avec_le_proprietaire()
    {
        var vehicle = Register();
        vehicle.ChangeStatus(VehicleStatus.Retired, Now);

        var result = vehicle.UpdateDetails(vehicle.Details with { Model = "X", OwnerCompanyId = CompanyA }, Today, Now);

        Assert.Equal(VehicleErrors.Retired, result.Error);
        Assert.Null(vehicle.OwnerCompanyId);
    }

    [Fact]
    public void Vehicule_sorti_du_parc_avec_proprietaire_refuse_tout_changement()
    {
        var vehicle = Register(CompanyA);
        vehicle.ChangeStatus(VehicleStatus.Retired, Now);

        Assert.Equal(VehicleErrors.Retired, vehicle.UpdateDetails(vehicle.Details with { OwnerCompanyId = CompanyB }, Today, Now).Error);
        Assert.Equal(CompanyA, vehicle.OwnerCompanyId);
    }

    [Fact]
    public void Vehicule_sorti_du_parc_sans_proprietaire_refuse_de_rester_sans()
    {
        var vehicle = Register();
        vehicle.ChangeStatus(VehicleStatus.Retired, Now);

        Assert.Equal(VehicleErrors.Retired, vehicle.UpdateDetails(vehicle.Details, Today, Now).Error);
    }
}
