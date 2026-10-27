using UsMecanic.Domain.Mileage;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Domain.Mileage;

public class MileageReadingTests
{
    private static readonly Guid VehicleId = Guid.NewGuid();
    private static readonly DateTimeOffset Now = FixedClock.Default; // 2026-10-01
    private static readonly DateOnly Today = new(2026, 10, 1);

    private static MileageReading Existing(DateOnly date, int km) =>
        MileageReading.Create(VehicleId, new MileageReadingEntry(date, km), [], Now).Value;

    private static MileageReadingEntry Entry(DateOnly date, int km, string? reason = null, bool confirm = false) =>
        new(date, km, reason, confirm);

    [Fact]
    public void Le_premier_releve_est_accepte_et_conserve_ses_valeurs()
    {
        var result = MileageReading.Create(VehicleId, Entry(Today, 12_000, "  Contrôle technique "), [], Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(VehicleId, result.Value.VehicleId);
        Assert.Equal(Today, result.Value.Date);
        Assert.Equal(12_000, result.Value.Kilometers);
        Assert.Equal("Contrôle technique", result.Value.Reason);
        Assert.Equal(Now, result.Value.RecordedAt);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
    }

    [Fact]
    public void Un_premier_releve_tres_eleve_n_est_pas_un_avertissement()
    {
        var result = MileageReading.Create(VehicleId, Entry(Today, 900_000), [], Now);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Un_motif_vide_est_ignore()
    {
        var result = MileageReading.Create(VehicleId, Entry(Today, 10, "   "), [], Now);

        Assert.Null(result.Value.Reason);
    }

    [Fact]
    public void Refuse_un_kilometrage_negatif()
    {
        var result = MileageReading.Create(VehicleId, Entry(Today, -1), [], Now);

        Assert.Equal(MileageReadingErrors.NegativeKilometers, result.Error);
    }

    [Fact]
    public void Accepte_zero_kilometre()
    {
        Assert.True(MileageReading.Create(VehicleId, Entry(Today, 0), [], Now).IsSuccess);
    }

    [Fact]
    public void Refuse_une_date_dans_le_futur_mais_accepte_aujourd_hui()
    {
        var tomorrow = MileageReading.Create(VehicleId, Entry(Today.AddDays(1), 10), [], Now);
        var today = MileageReading.Create(VehicleId, Entry(Today, 10), [], Now);

        Assert.Equal(MileageReadingErrors.DateInFuture, tomorrow.Error);
        Assert.True(today.IsSuccess);
    }

    [Fact]
    public void Refuse_un_motif_trop_long()
    {
        var result = MileageReading.Create(VehicleId, Entry(Today, 10, new string('x', MileageReading.MaxReasonLength + 1)), [], Now);

        Assert.Equal(MileageReadingErrors.ReasonTooLong, result.Error);
    }

    [Fact]
    public void Un_kilometrage_inferieur_au_precedent_exige_un_motif()
    {
        var others = new[] { Existing(Today.AddDays(-10), 5_000) };

        var without = MileageReading.Create(VehicleId, Entry(Today, 4_000), others, Now);
        var with = MileageReading.Create(VehicleId, Entry(Today, 4_000, "Compteur remplacé"), others, Now);

        Assert.Equal(MileageReadingErrors.ReasonRequired, without.Error);
        Assert.True(with.IsSuccess);
    }

    [Fact]
    public void Un_kilometrage_egal_au_precedent_est_accepte_sans_motif()
    {
        var others = new[] { Existing(Today.AddDays(-10), 5_000) };

        Assert.True(MileageReading.Create(VehicleId, Entry(Today, 5_000), others, Now).IsSuccess);
    }

    [Fact]
    public void Un_kilometrage_superieur_au_suivant_exige_un_motif()
    {
        var others = new[] { Existing(Today.AddDays(-10), 5_000), Existing(Today.AddDays(-2), 5_500) };

        var without = MileageReading.Create(VehicleId, Entry(Today.AddDays(-5), 5_600), others, Now);
        var with = MileageReading.Create(VehicleId, Entry(Today.AddDays(-5), 5_600, "Erreur de saisie corrigée plus tard"), others, Now);

        Assert.Equal(MileageReadingErrors.ReasonRequired, without.Error);
        Assert.True(with.IsSuccess);
    }

    [Fact]
    public void Une_insertion_entre_deux_releves_coherents_est_acceptee()
    {
        var others = new[] { Existing(Today.AddDays(-10), 5_000), Existing(Today.AddDays(-2), 5_500) };

        var result = MileageReading.Create(VehicleId, Entry(Today.AddDays(-5), 5_200), others, Now);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Un_releve_ancien_se_compare_au_suivant_et_non_au_dernier_en_date()
    {
        var others = new[] { Existing(Today, 50_000) };

        var result = MileageReading.Create(VehicleId, Entry(Today.AddDays(-30), 40_000), others, Now);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Plus_de_1000_km_par_jour_exige_une_confirmation()
    {
        var others = new[] { Existing(Today.AddDays(-2), 10_000) };

        var refused = MileageReading.Create(VehicleId, Entry(Today, 12_001), others, Now);
        var confirmed = MileageReading.Create(VehicleId, Entry(Today, 12_001, confirm: true), others, Now);

        Assert.Equal(MileageReadingErrors.HighMileageConfirmationRequired, refused.Error);
        Assert.True(confirmed.IsSuccess);
    }

    [Fact]
    public void Exactement_1000_km_par_jour_est_accepte()
    {
        var others = new[] { Existing(Today.AddDays(-2), 10_000) };

        Assert.True(MileageReading.Create(VehicleId, Entry(Today, 12_000), others, Now).IsSuccess);
    }

    [Fact]
    public void A_la_meme_date_l_ecart_compte_pour_un_jour()
    {
        var others = new[] { Existing(Today, 10_000) };

        var within = MileageReading.Create(VehicleId, Entry(Today, 11_000), others, Now);
        var above = MileageReading.Create(VehicleId, Entry(Today, 11_001), others, Now);

        Assert.True(within.IsSuccess);
        Assert.Equal(MileageReadingErrors.HighMileageConfirmationRequired, above.Error);
    }

    [Fact]
    public void A_la_meme_date_un_kilometrage_inferieur_exige_un_motif()
    {
        var others = new[] { Existing(Today, 10_000) };

        var result = MileageReading.Create(VehicleId, Entry(Today, 9_000), others, Now);

        Assert.Equal(MileageReadingErrors.ReasonRequired, result.Error);
    }

    [Fact]
    public void Une_baisse_exige_un_motif_et_non_une_confirmation()
    {
        var others = new[] { Existing(Today.AddDays(-1), 20_000) };

        // 20 000 -> 5 000 : en baisse, donc motif (pas d'avertissement de vitesse).
        var result = MileageReading.Create(VehicleId, Entry(Today, 5_000), others, Now);

        Assert.Equal(MileageReadingErrors.ReasonRequired, result.Error);
    }

    [Fact]
    public void Un_motif_accepte_un_releve_incoherent_sans_demander_de_confirmation()
    {
        var others = new[] { Existing(Today.AddDays(-10), 10_000), Existing(Today.AddDays(-1), 10_500) };

        // Au-dessus du suivant ET à plus de 1 000 km/jour du précédent : le motif suffit pour l'incohérence d'ordre.
        var result = MileageReading.Create(VehicleId, Entry(Today.AddDays(-9), 13_000, "Relevé du tableau de bord"), others, Now);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void La_modification_exclut_le_releve_modifie_des_voisins()
    {
        var first = Existing(Today.AddDays(-10), 5_000);
        var second = Existing(Today.AddDays(-5), 5_300);
        var third = Existing(Today.AddDays(-1), 5_600);
        IReadOnlyList<MileageReading> all = [first, second, third];

        // Sans exclusion, le précédent serait le relevé lui-même (5 300, même date) et 5 100 exigerait un motif.
        var result = second.Update(Entry(Today.AddDays(-5), 5_100), all, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(5_100, second.Kilometers);
    }

    [Fact]
    public void La_modification_applique_les_memes_controles()
    {
        var first = Existing(Today.AddDays(-10), 5_000);
        var second = Existing(Today.AddDays(-5), 5_300);
        var third = Existing(Today.AddDays(-1), 5_600);
        IReadOnlyList<MileageReading> all = [first, second, third];

        var tooLow = second.Update(Entry(Today.AddDays(-5), 4_900), all, Now);
        var tooHigh = second.Update(Entry(Today.AddDays(-5), 5_700), all, Now);
        var future = second.Update(Entry(Today.AddDays(1), 5_300), all, Now);
        var fast = second.Update(Entry(Today.AddDays(-9), 7_000), all, Now);

        Assert.Equal(MileageReadingErrors.ReasonRequired, tooLow.Error);
        Assert.Equal(MileageReadingErrors.ReasonRequired, tooHigh.Error);
        Assert.Equal(MileageReadingErrors.DateInFuture, future.Error);
        Assert.Equal(5_300, second.Kilometers);
        Assert.Equal(Today.AddDays(-5), second.Date);
        Assert.True(fast.IsFailure);
    }

    [Fact]
    public void La_modification_deplace_le_releve_dans_l_ordre_des_dates()
    {
        var first = Existing(Today.AddDays(-10), 5_000);
        var second = Existing(Today.AddDays(-5), 5_300);
        IReadOnlyList<MileageReading> all = [first, second];

        // Déplacé avant le premier : 5 300 > 5 000 (suivant) => motif requis.
        var result = second.Update(Entry(Today.AddDays(-20), 5_300), all, Now);

        Assert.Equal(MileageReadingErrors.ReasonRequired, result.Error);
    }

    [Fact]
    public void La_modification_conserve_la_date_de_saisie()
    {
        var reading = Existing(Today.AddDays(-5), 5_300);
        var later = Now.AddDays(3);

        var result = reading.Update(Entry(Today, 5_400, "Précision"), [reading], later);

        Assert.True(result.IsSuccess);
        Assert.Equal(Now, reading.RecordedAt);
        Assert.Equal("Précision", reading.Reason);
        Assert.Equal(Today, reading.Date);
    }
}
