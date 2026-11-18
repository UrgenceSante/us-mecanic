using UsMecanic.Domain.Rentals;

namespace UsMecanic.UnitTests.Domain.Rentals;

public class RentalTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly DateOnly Feb1 = new(2026, 2, 1);
    private static readonly DateOnly Mar31 = new(2026, 3, 31);

    private static Rental Make(DateOnly start, DateOnly? end) => Rental.Create(Guid.NewGuid(), new RentalEntry(Tenant, start, end)).Value;

    [Fact]
    public void Une_location_d_un_seul_jour_est_acceptee()
    {
        var result = Rental.Create(Guid.NewGuid(), new RentalEntry(Tenant, Feb1, Feb1));

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.Covers(Feb1));
    }

    [Fact]
    public void Refuse_une_fin_anterieure_au_debut()
    {
        var result = Rental.Create(Guid.NewGuid(), new RentalEntry(Tenant, Mar31, Feb1));

        Assert.Equal(RentalErrors.EndBeforeStart, result.Error);
    }

    [Fact]
    public void Refuse_un_locataire_ou_un_debut_absents()
    {
        Assert.Equal(RentalErrors.TenantRequired, Rental.Validate(new RentalEntry(Guid.Empty, Feb1, null)));
        Assert.Equal(RentalErrors.StartDateRequired, Rental.Validate(new RentalEntry(Tenant, default, null)));
    }

    [Fact]
    public void Les_bornes_sont_incluses()
    {
        var rental = Make(Feb1, Mar31);

        Assert.False(rental.Covers(Feb1.AddDays(-1)));
        Assert.True(rental.Covers(Feb1));
        Assert.True(rental.Covers(Mar31));
        Assert.False(rental.Covers(Mar31.AddDays(1)));
    }

    [Fact]
    public void Sans_fin_la_location_couvre_toutes_les_dates_apres_le_debut()
    {
        var rental = Make(Feb1, null);

        Assert.False(rental.Covers(Feb1.AddDays(-1)));
        Assert.True(rental.Covers(new DateOnly(2030, 1, 1)));
    }

    [Fact]
    public void La_suivante_peut_commencer_le_lendemain_de_la_fin_pas_le_jour_meme()
    {
        var rental = Make(Feb1, Mar31);

        Assert.True(rental.Overlaps(Mar31, null));
        Assert.False(rental.Overlaps(Mar31.AddDays(1), null));
        Assert.True(rental.Overlaps(Feb1.AddDays(-10), Feb1));
        Assert.False(rental.Overlaps(Feb1.AddDays(-10), Feb1.AddDays(-1)));
    }

    [Fact]
    public void Une_location_sans_fin_chevauche_toute_periode_ulterieure()
    {
        var rental = Make(Feb1, null);

        Assert.True(rental.Overlaps(new DateOnly(2030, 1, 1), new DateOnly(2030, 1, 2)));
        Assert.False(rental.Overlaps(Feb1.AddDays(-5), Feb1.AddDays(-1)));
    }

    [Fact]
    public void Etat_prevue_en_cours_terminee()
    {
        var rental = Make(Feb1, Mar31);

        Assert.Equal(RentalState.Planned, rental.StateOn(Feb1.AddDays(-1)));
        Assert.Equal(RentalState.Ongoing, rental.StateOn(Feb1));
        Assert.Equal(RentalState.Ongoing, rental.StateOn(Mar31));
        Assert.Equal(RentalState.Ended, rental.StateOn(Mar31.AddDays(1)));
    }

    [Fact]
    public void Une_location_est_en_cours_ou_a_venir_jusqu_a_sa_fin_incluse()
    {
        var rental = Make(Feb1, Mar31);

        Assert.True(rental.IsCurrentOrUpcoming(Mar31));
        Assert.False(rental.IsCurrentOrUpcoming(Mar31.AddDays(1)));
        Assert.True(Make(Feb1, null).IsCurrentOrUpcoming(Mar31));
    }
}
