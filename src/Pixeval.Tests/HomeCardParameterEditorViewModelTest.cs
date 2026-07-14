using System.Collections.Generic;
using Mako.Global.Enum;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.Controls;
using Pixeval.ViewModels.Home;
using Pixeval.Views.Home;

namespace Pixeval.Tests;

[TestClass]
public sealed class HomeCardParameterEditorViewModelTest
{
    [TestMethod]
    public void Value_InvalidBindingWrite_KeepsLastValidChoice()
    {
        IReadOnlyList<SymbolComboBoxItem> items =
        [
            new(SimpleWorkType.Illustration, "Illustration", default),
            new(SimpleWorkType.Novel, "Novel", default)
        ];
        var editor = new HomeCardChoiceParameterEditorViewModel(
            HomeCardParameterKinds.SimpleWorkType,
            "Work type",
            items,
            SimpleWorkType.Illustration);
        var changeCount = 0;
        editor.ValueChanged += (_, _) => changeCount++;

        editor.Value = null!;
        editor.Value = WorkType.Novel;

        Assert.AreEqual(SimpleWorkType.Illustration, editor.GetValue<SimpleWorkType>());
        Assert.AreEqual(0, changeCount);

        editor.Value = SimpleWorkType.Novel;

        Assert.AreEqual(SimpleWorkType.Novel, editor.GetValue<SimpleWorkType>());
        Assert.AreEqual(1, changeCount);
    }
}
