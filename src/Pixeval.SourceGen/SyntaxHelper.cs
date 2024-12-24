#region Copyright (c) Pixeval/Pixeval.SourceGen
// GPL v3 License
// 
// Pixeval/Pixeval.SourceGen
// Copyright (c) 2023 Pixeval.SourceGen/SyntaxHelper.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System;

namespace Pixeval.SourceGen;

public static class SyntaxHelper
{
    internal const string AttributeNamespace = "WinUI3Utilities.Attributes.";
    internal const string DisableSourceGeneratorAttribute = AttributeNamespace + "DisableSourceGeneratorAttribute";
    internal const string FrozenDictionaryTypeName = "global::System.Collections.Frozen.FrozenDictionary";

    public static bool HasAttribute(this ISymbol s, string attributeFqName)
    {
        return s.GetAttributes().Any(als => als.AttributeClass?.ToDisplayString() == attributeFqName);
    }

    public static AttributeData? GetAttribute(this ISymbol mds, string attributeFqName)
    {
        return mds.GetAttributes().FirstOrDefault(attr => attr?.AttributeClass?.ToDisplayString() == attributeFqName);
    }

    /// <summary>
    /// Generate the following code
    /// <code>
    /// <paramref name="typeSymbol" />&lt;<paramref name="isNullable" />&gt;
    /// </code>
    /// </summary>
    /// <returns>CompilationUnit</returns>
    internal static TypeSyntax GetTypeSyntax(this ITypeSymbol typeSymbol, bool isNullable)
    {
        var typeName = ParseTypeName(typeSymbol.ToDisplayString());
        return isNullable ? NullableType(typeName) : typeName;
    }

    /// <summary>
    /// Generate the following code
    /// <code>
    /// nameof(<paramref name="name" />)
    /// </code>
    /// </summary>
    /// <returns>NameOfExpression</returns>
    internal static InvocationExpressionSyntax NameOfExpression(string name) => NameOfExpression(IdentifierName(name));

    /// <summary>
    /// Generate the following code
    /// <code>
    /// nameof(<paramref name="expressionSyntax" />)
    /// </code>
    /// </summary>
    /// <returns>NameOfExpression</returns>
    internal static InvocationExpressionSyntax NameOfExpression(ExpressionSyntax expressionSyntax) => InvocationExpression(IdentifierName("nameof"), ArgumentList().AddArguments(Argument(expressionSyntax)));

    internal static IEnumerable<IPropertySymbol> GetProperties(this ITypeSymbol typeSymbol, INamedTypeSymbol attribute)
    {
        var symbol = typeSymbol;
        do
        {
            foreach (var member in symbol.GetMembers())
            {
                if (member is not IPropertySymbol { Name: not "EqualityContract", IsAbstract: false } property)
                    continue;

                if (IgnoreAttribute(property, attribute))
                    continue;

                yield return property;
            }

            symbol = symbol.BaseType;
        } while (symbol is not null);
    }

    internal static bool IgnoreAttribute(ISymbol symbol, INamedTypeSymbol attribute)
    {
        attribute = attribute is { IsGenericType: true, IsUnboundGenericType: false } ? attribute.ConstructUnboundGenericType() : attribute;
        if (symbol.GetAttributes()
                .FirstOrDefault(propertyAttribute => propertyAttribute.AttributeClass!.ToDisplayString() is AttributeNamespace + "AttributeIgnoreAttribute")
            is { ConstructorArguments: [{ Kind: TypedConstantKind.Array }] args })
            if (args[0].Values.Any(t =>
                {
                    if (t.Value is not INamedTypeSymbol type)
                        return false;
                    type = type is { IsGenericType: true, IsUnboundGenericType: false } ? type.ConstructUnboundGenericType() : type;
                    return SymbolEqualityComparer.Default.Equals(type, attribute);
                }))
                return true;
        return false;
    }

    /// <summary>
    /// Generate the following code
    /// <code>
    /// #nullable enable
    /// namespace <paramref name="specificClass" />.ContainingNamespace;<br/>
    /// <paramref name="generatedClass" />
    /// </code>
    /// </summary>
    /// <returns>FileScopedNamespaceDeclaration</returns>
    internal static FileScopedNamespaceDeclarationSyntax GetFileScopedNamespaceDeclaration(ISymbol specificClass, MemberDeclarationSyntax generatedClass, bool nullableEnable)
        => FileScopedNamespaceDeclaration(ParseName(specificClass.ContainingNamespace.ToDisplayString()))
            .AddMembers(generatedClass)
            .WithNamespaceKeyword(Token(SyntaxKind.NamespaceKeyword))
            .WithLeadingTrivia(Trivia(NullableDirectiveTrivia(Token(SyntaxKind.EnableKeyword), nullableEnable)));

    internal static FileScopedNamespaceDeclarationSyntax GetFileScopedNamespaceDeclaration(string namespaceName, MemberDeclarationSyntax generatedClass, bool nullableEnable)
        => FileScopedNamespaceDeclaration(ParseName(namespaceName))
            .AddMembers(generatedClass)
            .WithNamespaceKeyword(Token(SyntaxKind.NamespaceKeyword))
            .WithLeadingTrivia(Trivia(NullableDirectiveTrivia(Token(SyntaxKind.EnableKeyword), nullableEnable)));

    /// <summary>
    /// Generate the following code
    /// <code>
    /// partial <paramref name="originalSymbol" /> <paramref name="name" /><br/>
    /// {<br/>
    ///     <paramref name="member" /><br/>
    /// }
    /// </code>
    /// </summary>
    /// <returns>TypeDeclaration</returns>
    internal static TypeDeclarationSyntax GetDeclaration(string name, INamedTypeSymbol originalSymbol, params MemberDeclarationSyntax[] member)
    {
        TypeDeclarationSyntax typeDeclarationTemp = originalSymbol.TypeKind switch
        {
            TypeKind.Class when !originalSymbol.IsRecord => ClassDeclaration(name),
            TypeKind.Struct when !originalSymbol.IsRecord => StructDeclaration(name),
            TypeKind.Class or TypeKind.Struct when originalSymbol.IsRecord => RecordDeclaration(Token(SyntaxKind.RecordKeyword), name),
            _ => throw new ArgumentOutOfRangeException(nameof(originalSymbol.TypeKind))
        };
        return typeDeclarationTemp.AddModifiers(Token(SyntaxKind.PartialKeyword))
            .WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken))
            .AddMembers(member)
            .WithCloseBraceToken(Token(SyntaxKind.CloseBraceToken));
    }
}
