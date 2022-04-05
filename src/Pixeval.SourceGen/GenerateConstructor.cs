using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Pixeval.SourceGen.Utils;

namespace Pixeval.SourceGen;

internal static partial class TypeWithAttributeDelegates
{
    public static string GenerateConstructor(TypeDeclarationSyntax typeDeclaration, INamedTypeSymbol typeSymbol, Func<AttributeData, bool> attributeEqualityComparer)
    {
        var name = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        var namespaces = new HashSet<string>();
        var usedTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        var ctor = ConstructorDeclaration(name)
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)));
        foreach (var member in typeSymbol.GetMembers().Where(member =>
                         member is { Kind: SymbolKind.Property } and not { Name: "EqualityContract" })
                     .Cast<IPropertySymbol>())
        {
            ctor = GetDeclaration(member, ctor);
            namespaces.UseNamespace(usedTypes, typeSymbol, member.Type);
        }

        var generatedType = GetDeclaration(name, typeDeclaration, ctor);
        var generatedNamespace = GetNamespaceDeclaration(typeSymbol, generatedType);
        var compilationUnit = GetCompilationUnit(generatedNamespace, namespaces);
        return SyntaxTree(compilationUnit, encoding: Encoding.UTF8).GetText().ToString();
    }
}