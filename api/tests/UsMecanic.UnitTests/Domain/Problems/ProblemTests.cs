using UsMecanic.Domain.Common;
using UsMecanic.Domain.Problems;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Domain.Problems;

public class ProblemTests
{
    private static readonly DateOnly Today = FixedClock.DefaultToday;
    private static readonly DateTimeOffset Now = FixedClock.Default;

    private static Result<Problem> Create(string? description = "Voyant moteur allumé", string? author = "D. Martin", DateOnly? date = null) =>
        Problem.Create(Guid.NewGuid(), date ?? Today, description, author, Today, Now);

    [Fact]
    public void Create_produit_un_probleme_signale_avec_texte_nettoye()
    {
        var vehicleId = Guid.NewGuid();

        var result = Problem.Create(vehicleId, Today.AddDays(-1), "  Fuite d'huile ", " D. Martin  ", Today, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(ProblemStatus.Reported, result.Value.Status);
        Assert.Equal(vehicleId, result.Value.VehicleId);
        Assert.Equal(Today.AddDays(-1), result.Value.Date);
        Assert.Equal("Fuite d'huile", result.Value.Description);
        Assert.Equal("D. Martin", result.Value.Author);
        Assert.Equal(Now, result.Value.RecordedAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_refuse_une_description_absente(string? description) =>
        Assert.Equal(ProblemErrors.DescriptionRequired, Create(description: description).Error);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_refuse_un_auteur_absent(string? author) =>
        Assert.Equal(ProblemErrors.AuthorRequired, Create(author: author).Error);

    [Fact]
    public void Create_borne_la_description_et_l_auteur()
    {
        Assert.True(Create(description: new string('a', Problem.MaxDescriptionLength)).IsSuccess);
        Assert.Equal(ProblemErrors.DescriptionTooLong, Create(description: new string('a', Problem.MaxDescriptionLength + 1)).Error);
        Assert.True(Create(author: new string('a', Problem.MaxAuthorLength)).IsSuccess);
        Assert.Equal(ProblemErrors.AuthorTooLong, Create(author: new string('a', Problem.MaxAuthorLength + 1)).Error);
    }

    [Fact]
    public void Create_refuse_une_date_future_mais_accepte_aujourd_hui()
    {
        Assert.True(Create(date: Today).IsSuccess);
        Assert.Equal(ProblemErrors.DateInFuture, Create(date: Today.AddDays(1)).Error);
    }
}
