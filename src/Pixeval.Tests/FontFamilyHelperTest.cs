using Avalonia.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.Utilities;

namespace Pixeval.Tests;

[TestClass]
public sealed class FontFamilyHelperTest
{
    [TestMethod]
    public void NormalFontFamily_IsUsable()
    {
        Assert.IsTrue(FontFamilyHelper.IsUsable(new FontFamily("Anita Semi-squre")));
    }

    [TestMethod]
    public void FontFamilyWithRepeatedSeparator_IsNotUsable()
    {
        Assert.IsFalse(FontFamilyHelper.IsUsable(new FontFamily("Anita  Semi-squre")));
    }

    [TestMethod]
    public void InvalidFontFamily_IsRemovedFromFallbackList()
    {
        var fontFamily = FontFamilyHelper.Create(["Anita  Semi-squre", "Arial"]);

        Assert.IsNotNull(fontFamily);
        Assert.AreEqual("Arial", fontFamily.Name);
    }
}
