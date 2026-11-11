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
    [DataRow("Anita  Semi-squre")]
    [DataRow("Anita Semi-squre  ")]
    [DataRow("Anita\tSemi-squre")]
    public void FontFamilyWithWhitespace_IsUsable(string name)
    {
        // Avalonia accepts repeated and trailing whitespace when normalizing font names.
        Assert.IsTrue(FontFamilyHelper.IsUsable(new FontFamily(name)));
    }

    [TestMethod]
    public void FontFamilyWithRepeatedSeparator_IsPreservedInFallbackList()
    {
        var fontFamily = FontFamilyHelper.Create(["Anita  Semi-squre", "Arial"]);

        Assert.IsNotNull(fontFamily);
        Assert.AreEqual(new FontFamily("Anita  Semi-squre,Arial"), fontFamily);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(" \t ")]
    public void EmptyFontFamily_IsRemovedFromFallbackList(string name)
    {
        var fontFamily = FontFamilyHelper.Create([name, "Arial"]);

        Assert.IsNotNull(fontFamily);
        Assert.AreEqual("Arial", fontFamily.Name);
    }
}
