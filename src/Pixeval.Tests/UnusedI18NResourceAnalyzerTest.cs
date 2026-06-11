using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.SourceGen;

namespace Pixeval.Tests;

[TestClass]
public sealed class UnusedI18NResourceAnalyzerTest
{
    private const string ResourceDefinitions =
        """
        namespace Pixeval;

        public static class TestResources
        {
            public const string Used = "Test.Used";
            public const string XamlUsed = "Test.XamlUsed";
            public const string Unused = "Test.Parent.Unused";
            public const string OtherLanguageUnused = "Test.Parent.Unused";
        }
        """;

    [TestMethod]
    public async Task CSharpReferenceMarksResourceAsUsed()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using Pixeval;

            _ = TestResources.Used;
            """,
            [
                Json("zh-Hans",
                    """
                    {
                      "Used": "used"
                    }
                    """)
            ]);

        Assert.IsEmpty(diagnostics);
    }

    [TestMethod]
    public async Task XamlStaticReferenceMarksResourceAsUsed()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            "",
            [
                Json("zh-Hans",
                    """
                    {
                      "XamlUsed": "used"
                    }
                    """),
                Xaml(
                    """
                    <TextBlock Text="{I18N {x:Static pixeval:TestResources.XamlUsed}}" />
                    """)
            ]);

        Assert.IsEmpty(diagnostics);
    }

    [TestMethod]
    public async Task UnusedLeafResourceReportsDiagnosticOnLeafKey()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            "",
            [Json("zh-Hans",
                """
                {
                  "Parent": {
                    "Unused": "unused"
                  }
                }
                """)]);

        Assert.HasCount(1, diagnostics);
        var diagnostic = diagnostics[0];
        Assert.AreEqual(UnusedI18NResourceAnalyzer.DiagnosticId, diagnostic.Id);
        Assert.AreEqual(@"C:\Project\i18n\zh-Hans\Test.json", diagnostic.Location.GetLineSpan().Path);
        Assert.AreEqual(2, diagnostic.Location.GetLineSpan().StartLinePosition.Line);
        Assert.AreEqual(5, diagnostic.Location.GetLineSpan().StartLinePosition.Character);
    }

    [TestMethod]
    public async Task ParentObjectDoesNotReportDiagnostic()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            "",
            [Json("zh-Hans",
                """
                {
                  "Parent": {
                    "Unused": "unused"
                  }
                }
                """)]);

        Assert.HasCount(1, diagnostics);
        Assert.AreEqual("Test.Parent.Unused", diagnostics[0].GetMessage().Split('\'')[1]);
    }

    [TestMethod]
    public async Task SameUnusedKeyReportsInEveryLanguageFile()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            "",
            [
                Json("zh-Hans",
                    """
                    {
                      "Parent": {
                        "Unused": "unused"
                      }
                    }
                    """),
                Json("en-US",
                    """
                    {
                      "Parent": {
                        "Unused": "unused"
                      }
                    }
                    """)
            ]);

        Assert.HasCount(2, diagnostics);
        CollectionAssert.AreEquivalent(
            (string[]) [@"C:\Project\i18n\zh-Hans\Test.json", @"C:\Project\i18n\en-US\Test.json"],
            diagnostics.Select(diagnostic => diagnostic.Location.GetLineSpan().Path).ToArray());
    }

    private static TestAdditionalText Json(string culture, string text) => new($@"C:\Project\i18n\{culture}\Test.json", text);

    private static TestAdditionalText Xaml(string text) => new(@"C:\Project\Views\Test.axaml", text);

    private static async Task<IReadOnlyList<Diagnostic>> GetAnalyzerDiagnosticsAsync(string userSource, IReadOnlyList<AdditionalText> additionalTexts)
    {
        var compilation = CSharpCompilation.Create(
            "Pixeval.Tests.SourceGen.Target",
            [
                CSharpSyntaxTree.ParseText(SourceText.From(ResourceDefinitions, Encoding.UTF8)),
                CSharpSyntaxTree.ParseText(SourceText.From(userSource, Encoding.UTF8))
            ],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var compilationWithAnalyzers = compilation.WithAnalyzers(
            [new UnusedI18NResourceAnalyzer()],
            new AnalyzerOptions([.. additionalTexts]));

        var diagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
        return [.. diagnostics.OrderBy(diagnostic => diagnostic.Location.GetLineSpan().Path, StringComparer.Ordinal)];
    }

    private sealed class TestAdditionalText(string path, string text) : AdditionalText
    {
        public override string Path { get; } = path;

        public override SourceText GetText(CancellationToken cancellationToken = default) => SourceText.From(text, Encoding.UTF8);
    }
}
