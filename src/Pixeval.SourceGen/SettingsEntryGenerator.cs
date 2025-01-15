// Copyright (c) Pixeval.SourceGen.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Pixeval.SourceGen.SyntaxHelper;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Pixeval.SourceGen;

// [Generator]
public class SettingsEntryGenerator : IIncrementalGenerator
{
    private const string AttributeName = "SettingsEntryAttribute";

    private const string AttributeNamespace = nameof(Pixeval) + ".Attributes";

    private const string AttributeFullName = AttributeNamespace + "." + AttributeName;

    internal string TypeWithAttribute(ImmutableArray<GeneratorAttributeSyntaxContext> gas, out string fileName)
    {
        var list = new List<(IPropertySymbol Property, (int, string, string) Attribute)>();
        foreach (var ga in gas)
        {
            if (ga is { TargetSymbol: IPropertySymbol symbol, Attributes: [{ ConstructorArguments: [{ Value: int a }, { Value: string b }, { Value: string c }] }] })
            {
                list.Add((symbol, (a, b, c)));
            }
        }

        const string metadataTable = "MetadataTable";

        fileName = "SettingsEntryAttribute.g.cs";
        var generatedType = ClassDeclaration("SettingsEntryAttribute")
            .AddModifiers(Token(SyntaxKind.PartialKeyword))
            .WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken))
            .AddMembers(
                PropertyDeclaration(
                        ParseTypeName($"{FrozenDictionaryTypeName}<string, global::{AttributeFullName}>"),
                        metadataTable)
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                    .WithExpressionBody(ArrowExpressionClause(InvocationExpression(MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName(FrozenDictionaryTypeName),
                            IdentifierName("ToFrozenDictionary")),
                        ArgumentList(SeparatedList([
                            Argument(
                                ObjectCreationExpression(
                                    ParseTypeName(
                                        $"global::System.Collections.Generic.Dictionary<string, global::{AttributeFullName}>"),
                                    null,
                                    InitializerExpression(SyntaxKind.ObjectInitializerExpression,
                                        SeparatedList(list.Select(ExpressionSyntax (elem) =>
                                            AssignmentExpression(
                                                SyntaxKind.SimpleAssignmentExpression,
                                                ImplicitElementAccess(BracketedArgumentList(SeparatedList(
                                                    [
                                                        Argument(NameOfExpression(
                                                            elem.Property.ContainingType.ToDisplayString(
                                                                SymbolDisplayFormat.FullyQualifiedFormat) + "." +
                                                            elem.Property.Name))
                                                    ]
                                                ))),
                                                ObjectCreationExpression(ParseTypeName("global::" + AttributeFullName))
                                                    .WithArgumentList(ArgumentList(SeparatedList([
                                                        Argument(LiteralExpression(SyntaxKind.NumericLiteralExpression,
                                                            Literal(elem.Attribute.Item1))),
                                                        Argument(LiteralExpression(SyntaxKind.StringLiteralExpression,
                                                            Literal(elem.Attribute.Item2))),
                                                        Argument(LiteralExpression(SyntaxKind.StringLiteralExpression,
                                                            Literal(elem.Attribute.Item3)))
                                                    ])))))))))
                        ])))))
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
            )
            .WithCloseBraceToken(Token(SyntaxKind.CloseBraceToken));
        var generatedNamespace = GetFileScopedNamespaceDeclaration(AttributeNamespace, generatedType, true);
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
