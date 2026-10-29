using UsMecanic.Domain.Common;
using UsMecanic.Domain.Problems;

namespace UsMecanic.UnitTests.Domain.Problems;

public class ProblemReferenceTests
{
    private static void Check<T>(string expectedCode)
        where T : ProblemReference, IProblemReference<T>
    {
        Assert.Equal(expectedCode, T.Definition.Code);

        var created = T.Create("  Libellé ");
        Assert.True(created.IsSuccess);
        Assert.Equal("Libellé", created.Value.Name);
        Assert.True(created.Value.IsActive);
        Assert.NotEqual(Guid.Empty, created.Value.Id);

        Assert.Equal($"{expectedCode}.name_required", T.Create(" ").Error!.Code);
        Assert.Equal($"{expectedCode}.name_required", T.Create(null).Error!.Code);
        Assert.Equal($"{expectedCode}.name_too_long", T.Create(new string('x', ProblemReference.MaxNameLength + 1)).Error!.Code);
        Assert.True(T.Create(new string('x', ProblemReference.MaxNameLength)).IsSuccess);
    }

    [Fact]
    public void Create_nettoie_et_valide_le_libelle_pour_chaque_referentiel()
    {
        Check<ProblemNature>("problem_nature");
        Check<ProblemElement>("problem_element");
        Check<ProblemActor>("problem_actor");
        Check<ProblemActionType>("problem_action_type");
        Check<ProblemConstraint>("problem_constraint");
    }

    [Fact]
    public void Rename_change_le_libelle_et_conserve_l_identifiant()
    {
        var item = ProblemNature.Create("Mécanique").Value;
        var id = item.Id;

        var result = item.Rename(" Carrosserie ");

        Assert.True(result.IsSuccess);
        Assert.Equal("Carrosserie", item.Name);
        Assert.Equal(id, item.Id);
    }

    [Fact]
    public void Rename_refuse_un_libelle_vide_sans_modifier_la_valeur()
    {
        var item = ProblemActor.Create("Garage").Value;

        var result = item.Rename(" ");

        Assert.Equal(ProblemReferenceErrors.NameRequired(ProblemActor.Definition), result.Error);
        Assert.Equal("Garage", item.Name);
    }

    [Fact]
    public void Une_valeur_desactivee_est_refusee_a_la_saisie_mais_pas_une_valeur_active()
    {
        var item = ProblemConstraint.Create("Immédiat").Value;
        Assert.Null(item.GetSelectionError());

        item.Deactivate();

        Assert.False(item.IsActive);
        Assert.Equal(ProblemReferenceErrors.Inactive(ProblemConstraint.Definition), item.GetSelectionError());
        Assert.Equal("problem_constraint.inactive", item.GetSelectionError()!.Code);
    }

    [Fact]
    public void Les_erreurs_ont_le_bon_type_et_un_code_propre_au_referentiel()
    {
        var kind = ProblemElement.Definition;

        Assert.Equal(ErrorKind.NotFound, ProblemReferenceErrors.NotFound(kind).Kind);
        Assert.Equal(ErrorKind.Conflict, ProblemReferenceErrors.NameAlreadyExists(kind).Kind);
        Assert.Equal("problem_element.not_found", ProblemReferenceErrors.NotFound(kind).Code);
        Assert.Equal("problem_element.name_already_exists", ProblemReferenceErrors.NameAlreadyExists(kind).Code);
    }
}
