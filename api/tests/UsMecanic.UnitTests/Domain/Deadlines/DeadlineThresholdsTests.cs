using UsMecanic.Domain.Deadlines;

namespace UsMecanic.UnitTests.Domain.Deadlines;

public class DeadlineThresholdsTests
{
    [Fact]
    public void Les_valeurs_par_defaut_sont_30_jours_et_1000_km()
    {
        var thresholds = DeadlineThresholds.CreateDefault();

        Assert.Equal(DeadlineThresholds.SingletonId, thresholds.Id);
        Assert.Equal(30, thresholds.DueSoonDays);
        Assert.Equal(1000, thresholds.DueSoonKm);
    }

    [Fact]
    public void Update_modifie_les_deux_seuils()
    {
        var thresholds = DeadlineThresholds.CreateDefault();

        var result = thresholds.Update(15, 500);

        Assert.True(result.IsSuccess);
        Assert.Equal(15, thresholds.DueSoonDays);
        Assert.Equal(500, thresholds.DueSoonKm);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(-5)]
    public void Update_refuse_des_jours_absents_nuls_ou_negatifs(int? days)
    {
        var thresholds = DeadlineThresholds.CreateDefault();

        var result = thresholds.Update(days, 500);

        Assert.Equal(DeadlineThresholdsErrors.DaysInvalid, result.Error);
        Assert.Equal(30, thresholds.DueSoonDays);
        Assert.Equal(1000, thresholds.DueSoonKm);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(-5)]
    public void Update_refuse_des_km_absents_nuls_ou_negatifs(int? km)
    {
        var thresholds = DeadlineThresholds.CreateDefault();

        var result = thresholds.Update(15, km);

        Assert.Equal(DeadlineThresholdsErrors.KmInvalid, result.Error);
        Assert.Equal(30, thresholds.DueSoonDays);
        Assert.Equal(1000, thresholds.DueSoonKm);
    }
}
