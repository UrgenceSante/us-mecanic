using UsMecanic.Domain.Costs;

namespace UsMecanic.UnitTests.Domain.Costs;

public class ExpenseCategoryTests
{
    [Fact]
    public void Create_nettoie_le_nom_et_cree_une_categorie_active()
    {
        var result = ExpenseCategory.Create("  Nettoyage ");

        Assert.True(result.IsSuccess);
        Assert.Equal("Nettoyage", result.Value.Name);
        Assert.True(result.Value.IsActive);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_refuse_un_nom_vide(string? name)
    {
        Assert.Equal(ExpenseCategoryErrors.NameRequired, ExpenseCategory.Create(name).Error);
    }

    [Fact]
    public void Create_refuse_un_nom_trop_long()
    {
        var result = ExpenseCategory.Create(new string('x', ExpenseCategory.MaxNameLength + 1));

        Assert.Equal(ExpenseCategoryErrors.NameTooLong, result.Error);
    }

    [Fact]
    public void Create_accepte_un_nom_a_la_longueur_maximale()
    {
        Assert.True(ExpenseCategory.Create(new string('x', ExpenseCategory.MaxNameLength)).IsSuccess);
    }

    [Fact]
    public void Rename_change_le_nom_et_conserve_l_identifiant()
    {
        var expenseCategory = ExpenseCategory.Create("Nettoyage").Value;
        var id = expenseCategory.Id;

        var result = expenseCategory.Rename(" Équipement ");

        Assert.True(result.IsSuccess);
        Assert.Equal("Équipement", expenseCategory.Name);
        Assert.Equal(id, expenseCategory.Id);
    }

    [Fact]
    public void Rename_refuse_un_nom_vide_sans_modifier_la_categorie()
    {
        var expenseCategory = ExpenseCategory.Create("Nettoyage").Value;

        var result = expenseCategory.Rename(" ");

        Assert.Equal(ExpenseCategoryErrors.NameRequired, result.Error);
        Assert.Equal("Nettoyage", expenseCategory.Name);
    }

    [Fact]
    public void Deactivate_desactive_la_categorie()
    {
        var expenseCategory = ExpenseCategory.Create("Nettoyage").Value;

        expenseCategory.Deactivate();

        Assert.False(expenseCategory.IsActive);
    }

    [Fact]
    public void GetSelectionError_accepte_une_categorie_active()
    {
        var category = ExpenseCategory.Create("Nettoyage").Value;

        Assert.Null(category.GetSelectionError());
    }

    [Fact]
    public void GetSelectionError_refuse_une_categorie_inactive()
    {
        var category = ExpenseCategory.Create("Nettoyage").Value;
        category.Deactivate();

        Assert.Equal(ExpenseCategoryErrors.Inactive, category.GetSelectionError());
    }
}
