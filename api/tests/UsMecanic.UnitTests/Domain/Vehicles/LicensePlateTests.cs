using UsMecanic.Domain.Vehicles;

namespace UsMecanic.UnitTests.Domain.Vehicles;

public class LicensePlateTests
{
    [Theory]
    [InlineData("FS-414-ZX")]
    [InlineData("FS414ZX")]
    [InlineData("fs 414 zx")]
    [InlineData(" fs-414 zx ")]
    public void Create_normalise_les_saisies_valides(string input)
    {
        var result = LicensePlate.Create(input);

        Assert.True(result.IsSuccess);
        Assert.Equal("FS-414-ZX", result.Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1234 AB 56")]
    [InlineData("FS-41-ZX")]
    [InlineData("F5-414-ZX")]
    public void Create_refuse_les_formats_invalides(string? input)
    {
        var result = LicensePlate.Create(input);

        Assert.True(result.IsFailure);
        Assert.Equal(LicensePlate.InvalidFormat, result.Error);
    }

    [Fact]
    public void Deux_immatriculations_equivalentes_sont_egales()
    {
        Assert.Equal(LicensePlate.Create("fs414zx").Value, LicensePlate.Create("FS-414-ZX").Value);
    }
}
