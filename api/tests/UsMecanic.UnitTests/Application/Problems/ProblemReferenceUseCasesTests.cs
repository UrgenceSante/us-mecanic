using UsMecanic.Application.Problems.References;
using UsMecanic.Domain.Problems;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Application.Problems;

public class ProblemReferenceUseCasesTests
{
    private readonly InMemoryProblemReferenceRepository<ProblemNature> _natures = new();

    private static ReferenceKind Kind => ProblemNature.Definition;

    [Fact]
    public async Task Cree_une_valeur_active()
    {
        var result = await new CreateProblemReference<ProblemNature>(_natures).ExecuteAsync(" Mécanique ", CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Mécanique", result.Value.Name);
        Assert.True(result.Value.IsActive);
        Assert.Single(_natures.Saved);
    }

    [Fact]
    public async Task Refuse_un_libelle_vide_ou_en_double_sans_tenir_compte_de_la_casse()
    {
        _natures.Seed("Mécanique");
        var useCase = new CreateProblemReference<ProblemNature>(_natures);

        var empty = await useCase.ExecuteAsync("  ", CancellationToken.None);
        var duplicate = await useCase.ExecuteAsync("MÉCANIQUE", CancellationToken.None);

        Assert.Equal(ProblemReferenceErrors.NameRequired(Kind), empty.Error);
        Assert.Equal(ProblemReferenceErrors.NameAlreadyExists(Kind), duplicate.Error);
        Assert.Single(_natures.Saved);
        Assert.Equal(0, _natures.SaveCount);
    }

    [Fact]
    public async Task Refuse_un_libelle_trop_long()
    {
        var result = await new CreateProblemReference<ProblemNature>(_natures)
            .ExecuteAsync(new string('x', ProblemReference.MaxNameLength + 1), CancellationToken.None);

        Assert.Equal(ProblemReferenceErrors.NameTooLong(Kind), result.Error);
        Assert.Empty(_natures.Saved);
    }

    [Fact]
    public async Task Le_meme_libelle_est_accepte_dans_deux_referentiels_differents()
    {
        var actors = new InMemoryProblemReferenceRepository<ProblemActor>();
        _natures.Seed("Garage");

        var result = await new CreateProblemReference<ProblemActor>(actors).ExecuteAsync("Garage", CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Renomme_une_valeur_y_compris_pour_changer_la_casse()
    {
        var item = _natures.Seed("mécanique");

        var result = await new RenameProblemReference<ProblemNature>(_natures).ExecuteAsync(item.Id, "Mécanique", CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Mécanique", item.Name);
    }

    [Fact]
    public async Task Refuse_de_renommer_vers_un_libelle_existant_vide_ou_une_valeur_inconnue()
    {
        _natures.Seed("Mécanique");
        var other = _natures.Seed("Carrosserie");
        var useCase = new RenameProblemReference<ProblemNature>(_natures);

        var duplicate = await useCase.ExecuteAsync(other.Id, "mécanique", CancellationToken.None);
        var unknown = await useCase.ExecuteAsync(Guid.NewGuid(), "Électrique", CancellationToken.None);
        var empty = await useCase.ExecuteAsync(other.Id, " ", CancellationToken.None);

        Assert.Equal(ProblemReferenceErrors.NameAlreadyExists(Kind), duplicate.Error);
        Assert.Equal(ProblemReferenceErrors.NotFound(Kind), unknown.Error);
        Assert.Equal(ProblemReferenceErrors.NameRequired(Kind), empty.Error);
        Assert.Equal("Carrosserie", other.Name);
    }

    [Fact]
    public async Task Une_valeur_desactivee_n_est_plus_proposee_mais_reste_listee_sur_demande()
    {
        var item = _natures.Seed("Mécanique");

        var deactivated = await new DeactivateProblemReference<ProblemNature>(_natures).ExecuteAsync(item.Id, CancellationToken.None);
        var proposed = await new ListProblemReferences<ProblemNature>(_natures).ExecuteAsync(false, CancellationToken.None);
        var all = await new ListProblemReferences<ProblemNature>(_natures).ExecuteAsync(true, CancellationToken.None);

        Assert.False(deactivated.Value.IsActive);
        Assert.Empty(proposed);
        Assert.Equal(item.Id, Assert.Single(all).Id);
    }

    [Fact]
    public async Task Desactiver_une_valeur_inconnue_renvoie_introuvable()
    {
        var result = await new DeactivateProblemReference<ProblemNature>(_natures).ExecuteAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(ProblemReferenceErrors.NotFound(Kind), result.Error);
    }

    [Fact]
    public async Task La_liste_est_triee_par_libelle_sans_tenir_compte_de_la_casse()
    {
        _natures.Seed("Électrique");
        _natures.Seed("carrosserie");

        var list = await new ListProblemReferences<ProblemNature>(_natures).ExecuteAsync(false, CancellationToken.None);

        Assert.Equal("carrosserie", list[0].Name);
        Assert.Equal("Électrique", list[1].Name);
    }
}
