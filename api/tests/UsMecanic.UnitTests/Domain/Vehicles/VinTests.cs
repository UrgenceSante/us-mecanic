using UsMecanic.Domain.Vehicles;

namespace UsMecanic.UnitTests.Domain.Vehicles;

public class VinTests
{
    [Theory]
    [InlineData("VF1MA000012345678", "VF1MA000012345678")]
    [InlineData("vf1ma 0000 1234 5678", "VF1MA000012345678")]
    public void Create_normalise_les_saisies_valides(string input, string expected)
    {
        var result = Vin.Create(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("VF1MA00001234567")] // 16 caractères
    [InlineData("VF1MA0000123456789")] // 18 caractères
    [InlineData("VF1MO000012345678")] // lettre O interdite
    [InlineData("VF1MA00001234567-")]
    public void Create_refuse_les_formats_invalides(string? input)
    {
        Assert.Equal(Vin.InvalidFormat, Vin.Create(input).Error);
    }
}
