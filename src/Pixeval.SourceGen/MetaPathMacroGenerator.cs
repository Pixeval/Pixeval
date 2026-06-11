// Copyright (c) Pixeval.SourceGen.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Pixeval.SourceGen.SyntaxHelper;

namespace Pixeval.SourceGen;

[Generator]
public class MetaPathMacroGenerator : IIncrementalGenerator
{
    private const string AttributeName = "MetaPathMacroAttribute";

    private const string AttributeNamespace = nameof(Pixeval) + ".Download.Macros";

    private const string AttributeFullName = AttributeNamespace + "." + AttributeName;

    internal string TypeWithAttribute(ImmutableArray<GeneratorAttributeSyntaxContext> gas, out string fileName)
    {
        var list = new List<INamedTypeSymbol>();
        foreach (var ga in gas)
        {
            if (ga is { TargetSymbol: INamedTypeSymbol symbol })
            {
                list.Add(symbol);
            }
        }

        fileName = "DownloadPathMacroParser.g.cs";
        var generatedType = ClassDeclaration("DownloadPathMacroParser")
            .AddModifiers(Token(SyntaxKind.PartialKeyword))
            .WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken))
            .AddMembers(
                PropertyDeclaration(
                        ParseTypeName(
                            "global::System.Collections.Generic.IReadOnlyList<global::Pixeval.Download.MacroParser.IMacro>"),
                        "MacroProvider")
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                    .WithAccessorList(AccessorList([
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                    ]))
                    .WithInitializer(EqualsValueClause(CollectionExpression(SeparatedList(
                        list.Select(CollectionElementSyntax (v) =>
                            ExpressionElement(ObjectCreationExpression(v.GetTypeSyntax(false))
                                .WithArgumentList(ArgumentList()))))
                    )))
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
            )
            .WithCloseBraceToken(Token(SyntaxKind.CloseBraceToken));
        var generatedNamespace = GetFileScopedNamespaceDeclaration("Pixeval.Models.Download", generatedType, true);
        var compilationUnit = CompilationUnit()
            .AddMembers(generatedNamespace)
            .NormalizeWhitespace();
        return SyntaxTree(compilationUnit, encoding: Encoding.UTF8).GetText().ToString();
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var generatorAttributes = context.SyntaxProvider.ForAttributeWithMetadataName(
            AttributeFullName,
            (_, _) => true,
            (syntaxContext, _) => syntaxContext
        ).Collect();

        context.RegisterSourceOutput(generatorAttributes, (spc, gas) =>
        {
            if (gas.Length > 0)
                if (TypeWithAttribute(gas, out var name) is { } source)
                    spc.AddSource(name, source);
        });
    }
}
