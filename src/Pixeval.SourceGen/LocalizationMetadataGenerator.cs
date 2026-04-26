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
using Father = (string AttachedType, string ResourceType);
using Son = (string FieldName, string ResourceName);

namespace Pixeval.SourceGen;

public abstract class LocalizationMetadataGeneratorBase
{
    public static ClassDeclarationSyntax GetClassDeclaration(string typeFullName, string className, string resourceTypeName, Dictionary<string, List<Son>> members, bool isPartial)
    {
        const string enumStringPairName = "global::Pixeval.Controls.StringPair";
        const string getLocalizationKey = "GetLocalizationKey";
        var distinctMembers = members.SelectMany(kvp => kvp.Value).Distinct().ToList();
        const string getResource = "GetResource";
        return ClassDeclaration(className)
            .AddModifiers(Token(SyntaxKind.StaticKeyword),
                isPartial ? Token(SyntaxKind.PartialKeyword) : Token(SyntaxKind.PublicKeyword))
            .WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken))
            .WithCloseBraceToken(Token(SyntaxKind.CloseBraceToken))
            .AddMembers(ExtensionBlockDeclaration()
                    .AddParameterListParameters(Parameter(Identifier(resourceTypeName)))
                    .WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken))
                    .WithCloseBraceToken(Token(SyntaxKind.CloseBraceToken))
                    .AddMembers(
                        MethodDeclaration(ParseTypeName("string"), getResource)
                            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
                            .AddParameterListParameters(
                                Parameter(Identifier("value")).WithType(ParseTypeName(typeFullName)))
                            .WithExpressionBody(
                                ArrowExpressionClause(
                                    InvocationExpression(
                                            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                IdentifierName(resourceTypeName), IdentifierName(getResource)))
                                        .AddArgumentListArguments(
                                            Argument(InvocationExpression(
                                                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                        IdentifierName(typeFullName),
                                                        IdentifierName(getLocalizationKey)))
                                                .AddArgumentListArguments(Argument(IdentifierName("value")))))))
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))),
                ExtensionBlockDeclaration()
                    .AddParameterListParameters(Parameter(Identifier(typeFullName)))
                    .WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken))
                    .WithCloseBraceToken(Token(SyntaxKind.CloseBraceToken))
                    .AddMembers(
                        MethodDeclaration(ParseTypeName("string"), getLocalizationKey)
                            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
                            .AddParameterListParameters(
                                Parameter(Identifier("value")).WithType(ParseTypeName(typeFullName)))
                            .WithExpressionBody(ArrowExpressionClause(SwitchExpression(IdentifierName("value"))
                                .AddArms(
                                [
                                    .. distinctMembers.Select(member =>
                                        SwitchExpressionArm(ConstantPattern(IdentifierName(member.FieldName)),
                                            NameOfExpression(MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                IdentifierName(resourceTypeName),
                                                IdentifierName(member.ResourceName))))),
                                    SwitchExpressionArm(DiscardPattern(),
                                        ThrowExpression(
                                            ObjectCreationExpression(
                                                    ParseTypeName("global::System.ArgumentOutOfRangeException"))
                                                .AddArgumentListArguments(Argument(NameOfExpression("value")))))
                                ])))
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)))
                    .AddMembers([
                        .. members.Select(member =>
                            PropertyDeclaration(
                                    ParseTypeName(
                                        $"global::System.Collections.Generic.IReadOnlyList<global::AutoSettingsPage.IReadOnlyStringPair<object>>"),
                                    member.Key + "Pairs")
                                .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
                                .WithExpressionBody(ArrowExpressionClause(CollectionExpression()
                                    .WithOpenBracketToken(Token(SyntaxKind.OpenBracketToken))
                                    .AddElements(
                                    [
                                        .. member.Value.Select(CollectionElementSyntax (t) => ExpressionElement(
                                            ObjectCreationExpression(ParseTypeName(enumStringPairName))
                                                .AddArgumentListArguments(Argument(IdentifierName(t.FieldName)),
                                                    Argument(
                                                        InvocationExpression(MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                IdentifierName(resourceTypeName),
                                                                IdentifierName(getResource)))
                                                            .AddArgumentListArguments(
                                                                Argument(IdentifierName(t.FieldName)))))))
                                    ])))
                                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)))
                    ]));
    }

    protected abstract string AttributeName { get; }

    protected abstract string SubAttributeName { get; }

    protected string AttributeFullName => AttributeNamespace + "." + AttributeName;

    protected string SubAttributeFullName => AttributeNamespace + "." + SubAttributeName;

    protected const string AttributeNamespace = nameof(Pixeval) + ".Attributes";
}

[Generator]
public class LocalizationMetadataGenerator : LocalizationMetadataGeneratorBase, IIncrementalGenerator
{
    protected override string AttributeName => "LocalizationMetadataAttribute";

    protected override string SubAttributeName => "LocalizedResourceAttribute";

    internal string? TypeWithAttribute(INamedTypeSymbol typeSymbol, ImmutableArray<AttributeData> attributeList)
    {
        if (attributeList is not [{ ConstructorArguments: [{ Value: INamedTypeSymbol resourceType }, ..] }])
            return null;

        var isPartial = attributeList is [{ NamedArguments: [{ Key: "IsPartial", Value.Value: true }] }];

        List<Son> members = [];

        var typeFullName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        foreach (var member in typeSymbol.GetMembers())
        {
            if (member is not IFieldSymbol field)
                continue;

            if (field.GetAttribute(SubAttributeFullName) is not { ConstructorArguments: [{ Value: string resourceName }, ..] })
                continue;

            members.Add((typeFullName + "." + field.Name, resourceName));
        }

        var generatedType = GetClassDeclaration(typeFullName, typeSymbol.Name + "Extension", resourceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), new() { [""] = members }, isPartial);

        var generatedNamespace = GetFileScopedNamespaceDeclaration(typeSymbol, generatedType, true);
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
            (syntaxContext, _) => syntaxContext);

        context.RegisterSourceOutput(generatorAttributes, (spc, ga) =>
        {
            if (ga.TargetSymbol is not INamedTypeSymbol symbol)
                return;

            if (TypeWithAttribute(symbol, ga.Attributes) is { } source)
                spc.AddSource(
                    // 不能重名
                    $"{symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted))}_{AttributeFullName}.g.cs",
                    source);
        });
    }
}

[Generator]
public class AttachedLocalizationMetadataGenerator : LocalizationMetadataGeneratorBase, IIncrementalGenerator
{
    protected override string AttributeName => "AttachedLocalizationMetadataAttribute`1";

    protected override string SubAttributeName => "AttachedLocalizedResourceAttribute";

    internal IEnumerable<(string Name, string Text)> TypeWithAttribute(INamedTypeSymbol typeSymbol, ImmutableArray<AttributeData> attributeList)
    {
        Dictionary<Father, Dictionary<string, List<Son>>> subAttributesMap = [];
        Father? currentFather = null;
        List<Son>? current = null;
        foreach (var attribute in typeSymbol.GetAttributes())
        {
            var attributeName = attribute.AttributeClass?.ToDisplayString();
            if (attribute is
                {
                    ConstructorArguments:
                    [{ Value: INamedTypeSymbol resourceType }, { Value: string distinctName }, ..],
                    AttributeClass.TypeArguments: [INamedTypeSymbol attachedType, ..]
                })
            {
                var f = (attachedType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), resourceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                currentFather = f;
                if (!subAttributesMap.TryGetValue(f, out var value))
                    subAttributesMap[f] = value = new();
                value[distinctName] = current = [];
                continue;
            }

            if (currentFather is not { } father || current is null)
                continue;

            if (attribute is
                {
                    ConstructorArguments: [{ Value: string fieldName }, { Value: string resourceName }, ..]
                } && attributeName == SubAttributeFullName)
                current.Add((
                    father.AttachedType + "." + fieldName,
                    resourceName));
        }

        foreach (var kvp in subAttributesMap)
        {
            var generatedType = GetClassDeclaration(kvp.Key.AttachedType, typeSymbol.Name, kvp.Key.ResourceType, kvp.Value, true);
            var generatedNamespace = GetFileScopedNamespaceDeclaration(typeSymbol, generatedType, true);
            var compilationUnit = CompilationUnit()
                .AddMembers(generatedNamespace)
                .NormalizeWhitespace();
            yield return (kvp.Key.AttachedType.Split('.')[^1],
                SyntaxTree(compilationUnit, encoding: Encoding.UTF8).GetText().ToString());
        }
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var generatorAttributes = context.SyntaxProvider.ForAttributeWithMetadataName(
            AttributeFullName,
            (_, _) => true,
            (syntaxContext, _) => syntaxContext);

        context.RegisterSourceOutput(generatorAttributes, (spc, ga) =>
        {
            if (ga.TargetSymbol is not INamedTypeSymbol symbol)
                return;

            foreach (var (name, text) in TypeWithAttribute(symbol, ga.Attributes))
            {
                spc.AddSource(
                    // 不能重名
                    $"{symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted))}.{name}_{AttributeFullName}.g.cs",
                    text);
            }
        });
    }
}
