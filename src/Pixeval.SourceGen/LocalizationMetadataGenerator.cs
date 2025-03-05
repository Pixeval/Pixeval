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

public abstract class LocalizationMetadataGeneratorBase
{
    public static ClassDeclarationSyntax GetClassDeclaration(string typeFullName, string name, string resourceTypeName, List<(string Name, string ResourceName)> members, bool isPartial)
    {
        const string stringRepresentableItemName = "global::Pixeval.Controls.StringRepresentableItem";
        const string valuesTable = "ValuesTable";
        const string getResource = "GetResource";
        return ClassDeclaration(name)
            .AddModifiers(Token(SyntaxKind.StaticKeyword),
                isPartial ? Token(SyntaxKind.PartialKeyword) : Token(SyntaxKind.PublicKeyword))
            .WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken))
            .AddMembers(
                MethodDeclaration(
                        ParseTypeName(
                            $"global::System.Collections.Generic.IReadOnlyList<{stringRepresentableItemName}>"),
                        "GetItems")
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                    .WithBody(
                        Block(List<StatementSyntax>([
                            LocalDeclarationStatement(
                                VariableDeclaration(ParseTypeName("var"), SeparatedList([
                                    VariableDeclarator("array").WithInitializer(EqualsValueClause(
                                        ArrayCreationExpression(
                                            ArrayType(
                                                ParseTypeName(stringRepresentableItemName),
                                                List([
                                                    ArrayRankSpecifier(SeparatedList([
                                                        (ExpressionSyntax)MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            IdentifierName(valuesTable),
                                                            IdentifierName("Count"))
                                                    ]))
                                                ])))))
                                ]))),
                            LocalDeclarationStatement(
                                VariableDeclaration(ParseTypeName("var"), SeparatedList([
                                    VariableDeclarator("i").WithInitializer(EqualsValueClause(
                                        LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0))))
                                ]))),
                            ForEachStatement(ParseTypeName("var"), Identifier("t"), IdentifierName(valuesTable),
                                Block(List<StatementSyntax>([
                                    ExpressionStatement(
                                        AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                            ElementAccessExpression(IdentifierName("array"),
                                                BracketedArgumentList(SeparatedList([Argument(IdentifierName("i"))]))),
                                            ObjectCreationExpression(ParseTypeName(stringRepresentableItemName),
                                                ArgumentList(SeparatedList([
                                                    Argument(MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        IdentifierName("t"),
                                                        IdentifierName("Key"))),
                                                    Argument(InvocationExpression(
                                                        MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            IdentifierName(resourceTypeName),
                                                            IdentifierName(getResource)),
                                                        ArgumentList(SeparatedList([
                                                            Argument(MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                IdentifierName("t"),
                                                                IdentifierName("Value")))
                                                        ]))))
                                                ])), null))),
                                    ExpressionStatement(PrefixUnaryExpression(SyntaxKind.PreIncrementExpression,
                                        IdentifierName("i")))
                                ]))),
                            ReturnStatement(IdentifierName("array"))
                        ]))),
                MethodDeclaration(ParseTypeName("string"), getResource)
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                    .WithParameterList(ParameterList(SeparatedList([
                        Parameter(Identifier("id")).WithType(ParseTypeName("string"))
                    ])))
                    .WithExpressionBody(
                        ArrowExpressionClause(
                            InvocationExpression(
                                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName(resourceTypeName), IdentifierName(getResource)),
                                ArgumentList(SeparatedList([
                                    Argument(IdentifierName("id"))
                                ])))))
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                MethodDeclaration(ParseTypeName("string"), getResource)
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                    .WithParameterList(ParameterList(SeparatedList([
                        Parameter(Identifier("value")).WithType(ParseTypeName(typeFullName))
                    ])))
                    .WithExpressionBody(
                        ArrowExpressionClause(
                            InvocationExpression(
                                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName(resourceTypeName), IdentifierName(getResource)),
                                ArgumentList(SeparatedList([
                                    Argument(ElementAccessExpression(
                                        IdentifierName(valuesTable), BracketedArgumentList(SeparatedList([
                                            Argument(IdentifierName("value"))
                                        ]))))
                                ])))))
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                PropertyDeclaration(
                        ParseTypeName("global::System.Collections.Frozen.FrozenDictionary<string, string>"),
                        "NamesTable")
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                    .WithExpressionBody(ArrowExpressionClause(InvocationExpression(MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("global::System.Collections.Frozen.FrozenDictionary"),
                            IdentifierName("ToFrozenDictionary")),
                        ArgumentList(SeparatedList([
                            Argument(
                                ObjectCreationExpression(
                                    ParseTypeName("global::System.Collections.Generic.Dictionary<string, string>"),
                                    null,
                                    InitializerExpression(SyntaxKind.ObjectInitializerExpression,
                                        SeparatedList(members.Select(member =>
                                            (ExpressionSyntax)AssignmentExpression(
                                                SyntaxKind.SimpleAssignmentExpression,
                                                ImplicitElementAccess(BracketedArgumentList(SeparatedList(
                                                    [Argument(NameOfExpression(member.Name))]
                                                ))),
                                                NameOfExpression(MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    IdentifierName(resourceTypeName),
                                                    IdentifierName(member.ResourceName)))))))))
                        ])))))
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                PropertyDeclaration(
                        ParseTypeName($"{FrozenDictionaryTypeName}<{typeFullName}, string>"),
                        valuesTable)
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                    .WithExpressionBody(ArrowExpressionClause(InvocationExpression(MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName(FrozenDictionaryTypeName),
                            IdentifierName("ToFrozenDictionary")),
                        ArgumentList(SeparatedList([
                            Argument(
                                ObjectCreationExpression(
                                    ParseTypeName(
                                        $"global::System.Collections.Generic.Dictionary<{typeFullName}, string>"),
                                    null,
                                    InitializerExpression(SyntaxKind.ObjectInitializerExpression,
                                        SeparatedList(members.Select(member =>
                                            (ExpressionSyntax)AssignmentExpression(
                                                SyntaxKind.SimpleAssignmentExpression,
                                                ImplicitElementAccess(BracketedArgumentList(SeparatedList(
                                                    [Argument(IdentifierName(member.Name))]
                                                ))),
                                                NameOfExpression(MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    IdentifierName(resourceTypeName),
                                                    IdentifierName(member.ResourceName)))))))))
                        ])))))
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)))
            .WithCloseBraceToken(Token(SyntaxKind.CloseBraceToken));
    }
}

[Generator]
public class LocalizationMetadataGenerator : LocalizationMetadataGeneratorBase, IIncrementalGenerator
{
    private const string AttributeName = "LocalizationMetadataAttribute";

    private const string SubAttributeName = "LocalizedResourceAttribute";

    private const string AttributeNamespace = nameof(Pixeval) + ".Attributes";

    private const string AttributeFullName = AttributeNamespace + "." + AttributeName;

    private const string SubAttributeFullName = AttributeNamespace + "." + SubAttributeName;

    internal string? TypeWithAttribute(INamedTypeSymbol typeSymbol, ImmutableArray<AttributeData> attributeList)
    {
        if (attributeList is not [{ ConstructorArguments: [{ Value: INamedTypeSymbol resourceType }, ..] }])
            return null;

        var isPartial = attributeList is [{ NamedArguments: [{ Key: "IsPartial", Value.Value: true }] }];

        List<(string Name, string ResourceName)> members = [];

        foreach (var member in typeSymbol.GetMembers())
        {
            if (member is not IFieldSymbol field)
                continue;

            if (field.GetAttribute(SubAttributeFullName) is not { ConstructorArguments: [_, { Value: string resourceName }, ..] })
                continue;

            members.Add((typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + "." + field.Name, resourceName));
        }

        var generatedType = GetClassDeclaration(typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), $"{typeSymbol.Name}Extension", resourceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), members, isPartial);

        var generatedNamespace = GetFileScopedNamespaceDeclaration(typeSymbol.ContainingNamespace.ToDisplayString(), generatedType, true);
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
        );

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
    private const string AttributeName = "AttachedLocalizationMetadataAttribute`1";

    private const string SubAttributeName = "AttachedLocalizedResourceAttribute";

    private const string AttributeNamespace = nameof(Pixeval) + ".Attributes";

    private const string AttributeFullName = AttributeNamespace + "." + AttributeName;

    private const string SubAttributeFullName = AttributeNamespace + "." + SubAttributeName;

    internal string? TypeWithAttribute(INamedTypeSymbol typeSymbol, ImmutableArray<AttributeData> attributeList)
    {
        if (attributeList is not [{ ConstructorArguments: [{ Value: INamedTypeSymbol resourceType }, ..], AttributeClass.TypeArguments: [INamedTypeSymbol attachedType, ..] }])
            return null;

        List<(string Name, string ResourceName)> members = [];

        foreach (var attribute in typeSymbol.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() is not SubAttributeFullName)
                continue;

            if (attribute is not { ConstructorArguments: [{ Value: string fieldName }, { Value: string resourceName }, ..] })
                continue;

            members.Add((attachedType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + "." + fieldName, resourceName));
        }

        var generatedType = GetClassDeclaration(attachedType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), typeSymbol.Name, resourceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), members, true);

        var generatedNamespace = GetFileScopedNamespaceDeclaration(typeSymbol.ContainingNamespace.ToDisplayString(), generatedType, true);
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
        );

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
