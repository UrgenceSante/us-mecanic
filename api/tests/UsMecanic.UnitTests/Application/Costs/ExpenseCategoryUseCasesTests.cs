using UsMecanic.Application.Costs.ExpenseCategories;
using UsMecanic.Domain.Costs;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Application.Costs;

public class ExpenseCategoryUseCasesTests
{
    private readonly InMemoryExpenseCategoryRepository _expenseCategories = new();

    [Fact]
    public async Task Cree_une_categorie_active()
    {
        var result = await new CreateExpenseCategory(_expenseCategories).ExecuteAsync(" Nettoyage ", CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Nettoyage", result.Value.Name);
        Assert.True(result.Value.IsActive);
        Assert.Single(_expenseCategories.Saved);
    }

    [Fact]
    public async Task Refuse_un_nom_vide_ou_en_double_sans_tenir_compte_de_la_casse()
    {
        _expenseCategories.Seed("Nettoyage");
        var useCase = new CreateExpenseCategory(_expenseCategories);

        var empty = await useCase.ExecuteAsync("  ", CancellationToken.None);
        var duplicate = await useCase.ExecuteAsync("NETTOYAGE", CancellationToken.None);

        Assert.Equal(ExpenseCategoryErrors.NameRequired, empty.Error);
        Assert.Equal(ExpenseCategoryErrors.NameAlreadyExists, duplicate.Error);
        Assert.Single(_expenseCategories.Saved);
        Assert.Equal(0, _expenseCategories.SaveCount);
    }

    [Fact]
    public async Task Refuse_un_nom_trop_long()
    {
        var result = await new CreateExpenseCategory(_expenseCategories).ExecuteAsync(new string('x', ExpenseCategory.MaxNameLength + 1), CancellationToken.None);

        Assert.Equal(ExpenseCategoryErrors.NameTooLong, result.Error);
        Assert.Empty(_expenseCategories.Saved);
    }

    [Fact]
    public async Task Renomme_une_categorie_y_compris_pour_changer_la_casse()
    {
        var expenseCategory = _expenseCategories.Seed("nettoyage");

        var result = await new RenameExpenseCategory(_expenseCategories).ExecuteAsync(expenseCategory.Id, "Nettoyage", CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Nettoyage", expenseCategory.Name);
    }

    [Fact]
    public async Task Refuse_de_renommer_vers_un_nom_existant_ou_une_categorie_inconnue()
    {
        _expenseCategories.Seed("Nettoyage");
        var other = _expenseCategories.Seed("Équipement");
        var useCase = new RenameExpenseCategory(_expenseCategories);

        var duplicate = await useCase.ExecuteAsync(other.Id, "nettoyage", CancellationToken.None);
        var unknown = await useCase.ExecuteAsync(Guid.NewGuid(), "Divers", CancellationToken.None);
        var empty = await useCase.ExecuteAsync(other.Id, " ", CancellationToken.None);

        Assert.Equal(ExpenseCategoryErrors.NameAlreadyExists, duplicate.Error);
        Assert.Equal(ExpenseCategoryErrors.NotFound, unknown.Error);
        Assert.Equal(ExpenseCategoryErrors.NameRequired, empty.Error);
        Assert.Equal("Équipement", other.Name);
    }

    [Fact]
    public async Task Une_categorie_desactivee_n_est_plus_proposee_mais_reste_liste_sur_demande()
    {
        var expenseCategory = _expenseCategories.Seed("Nettoyage");

        var deactivated = await new DeactivateExpenseCategory(_expenseCategories).ExecuteAsync(expenseCategory.Id, CancellationToken.None);
        var proposed = await new ListExpenseCategories(_expenseCategories).ExecuteAsync(false, CancellationToken.None);
        var all = await new ListExpenseCategories(_expenseCategories).ExecuteAsync(true, CancellationToken.None);

        Assert.False(deactivated.Value.IsActive);
        Assert.Empty(proposed);
        Assert.Equal(expenseCategory.Id, Assert.Single(all).Id);
    }

    [Fact]
    public async Task Desactiver_une_categorie_inconnue_renvoie_introuvable()
    {
        var result = await new DeactivateExpenseCategory(_expenseCategories).ExecuteAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(ExpenseCategoryErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task La_liste_est_triee_par_nom_sans_tenir_compte_de_la_casse()
    {
        _expenseCategories.Seed("Divers");
        _expenseCategories.Seed("abonnement");

        var list = await new ListExpenseCategories(_expenseCategories).ExecuteAsync(false, CancellationToken.None);

        Assert.Equal("abonnement", list[0].Name);
        Assert.Equal("Divers", list[1].Name);
    }
}
