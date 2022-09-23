#region Copyright (c) Pixeval/Pixeval.SourceGen

// GPL v3 License
// 
// Pixeval/Pixeval.SourceGen
// Copyright (c) 2021 Pixeval.SourceGen/LoadSaveConfigurationGenerator.cs
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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Pixeval.SourceGen;

internal static class Utils
{
    internal static INamedTypeSymbol ObjectSymbol { get; set; } = null!;

    #region Abstract Syntax Tree Generate

    /// <summary>
    /// Generate the following code
    /// <code>
    /// public <paramref name="ctor" /> (...<paramref name="property" />.Type variable...)
    /// {
    ///     <paramref name="property" />.Name = variable;
    /// }
    /// </code>
    /// </summary>
    /// <returns></returns>
    internal static ConstructorDeclarationSyntax GetDeclaration(IPropertySymbol property, ConstructorDeclarationSyntax ctor)
    {
        var newName = property.Name.Substring(0, 1).ToLower() + property.Name.Substring(1);
        return ctor.AddParameterListParameters(
                Parameter(Identifier(newName)).WithType(property.Type.GetTypeSyntax(false)))
            .AddBodyStatements(ExpressionStatement(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName(property.Name),
                    IdentifierName(newName))));
    }

    /// <summary>
    /// Generate the following code
    /// <code>
    /// partial <paramref name="typeDeclaration" /> <paramref name="name" />
    /// {
    ///     <paramref name="member" />
    /// }
    /// </code>
    /// </summary>
    /// <returns>TypeDeclaration</returns>
    internal static TypeDeclarationSyntax GetDeclaration(string name, TypeDeclarationSyntax typeDeclaration, MemberDeclarationSyntax member)
    {
        TypeDeclarationSyntax typeDeclarationTemp = typeDeclaration switch
        {
            ClassDeclarationSyntax => ClassDeclaration(name),
            StructDeclarationSyntax => StructDeclaration(name),
            RecordDeclarationSyntax => RecordDeclaration(Token(SyntaxKind.RecordKeyword), name),
            _ => throw new ArgumentOutOfRangeException(nameof(typeDeclaration))
        };
        return typeDeclarationTemp.AddModifiers(Token(SyntaxKind.PartialKeyword))
            .WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken))
            .AddMembers(member)
            .WithCloseBraceToken(Token(SyntaxKind.CloseBraceToken));
    }

    /// <summary>
    /// Generate the following code
    /// <code>
    /// new PropertyMetadata(<paramref name="defaultValueExpression" />);
    /// </code>
    /// </summary>
    /// <returns>ObjectCreationExpression</returns>
    internal static ObjectCreationExpressionSyntax GetObjectCreationExpression(ExpressionSyntax defaultValueExpression)
    {
        return ObjectCreationExpression(IdentifierName("PropertyMetadata"))
            .AddArgumentListArguments(Argument(defaultValueExpression));
    }

    /// <summary>
    /// Generate the following code
    /// <code>
    /// new PropertyMetadata(<paramref name="metadataCreation" />, <paramref name="partialMethodName" />)
    /// </code>
    /// </summary>
    /// <returns>MetadataCreation</returns>
    internal static ObjectCreationExpressionSyntax GetMetadataCreation(ObjectCreationExpressionSyntax metadataCreation, string partialMethodName)
    {
        return metadataCreation.AddArgumentListArguments(Argument(IdentifierName(partialMethodName)));
    }

    /// <summary>
    /// Generate the following code
    /// <code>
    /// DependencyProperty.Register("<paramref name="propertyName" />", typeof(<paramref name="type" />), typeof(<paramref name="specificClass" />), <paramref name="metadataCreation" />);
    /// </code>
    /// </summary>
    /// <returns>Registration</returns>
    internal static InvocationExpressionSyntax GetRegistration(string propertyName, ITypeSymbol type, ITypeSymbol specificClass, ExpressionSyntax metadataCreation)
    {
        return InvocationExpression(MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression, IdentifierName("DependencyProperty"), IdentifierName("Register")))
            .AddArgumentListArguments(Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(propertyName))), Argument(TypeOfExpression(type.GetTypeSyntax(false))), Argument(TypeOfExpression(specificClass.GetTypeSyntax(false))), Argument(metadataCreation));
    }

    /// <summary>
    /// Generate the following code
    /// <code>
    /// public static readonly DependencyProperty <paramref name="fieldName" /> = <paramref name="registration" />;
    /// </code>
    /// </summary>
    /// <returns>StaticFieldDeclaration</returns>
    internal static FieldDeclarationSyntax GetStaticFieldDeclaration(string fieldName, ExpressionSyntax registration)
    {
        return FieldDeclaration(VariableDeclaration(IdentifierName("DependencyProperty")))
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword), Token(SyntaxKind.ReadOnlyKeyword))
            .AddDeclarationVariables(VariableDeclarator(fieldName).WithInitializer(EqualsValueClause(registration)));
    }

    /// <summary>
    /// Generate the following code
    /// <code>
    /// get => (<paramref name="type" />&lt;<paramref name="isNullable" />&gt;)GetValue(<paramref name="fieldName" />);
    /// </code>
    /// </summary>
    /// <returns>Getter</returns>
    internal static AccessorDeclarationSyntax GetGetter(string fieldName, bool isNullable, ITypeSymbol type)
    {
        ExpressionSyntax getProperty = InvocationExpression(IdentifierName("GetValue"))
            .AddArgumentListArguments(Argument(IdentifierName(fieldName)));
        if (!SymbolEqualityComparer.Default.Equals(type, ObjectSymbol))
        {
            getProperty = CastExpression(type.GetTypeSyntax(isNullable), getProperty);
        }

        return AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
            .WithExpressionBody(ArrowExpressionClause(getProperty))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
    }

    /// <summary>
    /// Generate the following code
    /// <code>
    /// &lt;<paramref name="isSetterPublic" />&gt; set => SetValue(<paramref name="fieldName" />, value);
    /// </code>
    /// </summary>
    /// <returns>Setter</returns>
    internal static AccessorDeclarationSyntax GetSetter(string fieldName, bool isSetterPublic)
    {
        ExpressionSyntax setProperty = InvocationExpression(IdentifierName("SetValue"))
            .AddArgumentListArguments(Argument(IdentifierName(fieldName)), Argument(IdentifierName("value")));
        var setter = AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
            .WithExpressionBody(ArrowExpressionClause(setProperty))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        return !isSetterPublic ? setter.AddModifiers(Token(SyntaxKind.PrivateKeyword)) : setter;
    }

    /// <summary>
    /// Generate the following code
    /// <code>
    /// public <paramref name="type" />&lt;<paramref name="isNullable" />&gt; <paramref name="propertyName" /> { <paramref name="getter" />; <paramref name="setter" />; }
    /// </code>
    /// </summary>
    /// <returns>PropertyDeclaration</returns>
    internal static PropertyDeclarationSyntax GetPropertyDeclaration(string propertyName, bool isNullable, ITypeSymbol type, AccessorDeclarationSyntax getter, AccessorDeclarationSyntax setter)
    {
        return PropertyDeclaration(type.GetTypeSyntax(isNullable), propertyName)
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddAccessorListAccessors(getter, setter);
    }

    /// <summary>
    /// Generate the following code
    /// <code>
    /// partial class <paramref name="specificClass" /><br/>
    /// {
    ///     <paramref name="members" /><br/>
    /// }
    /// </code>
    /// </summary>
    /// <returns>ClassDeclaration</returns>
    internal static ClassDeclarationSyntax GetClassDeclaration(ISymbol specificClass, IEnumerable<MemberDeclarationSyntax> members)
    {
        return ClassDeclaration(specificClass.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat))
            .AddModifiers(Token(SyntaxKind.PartialKeyword))
            .AddMembers(members.ToArray());
    }

    /// <summary>
    /// Generate the following code
    /// <code>
    /// namespace <paramref name="specificClass" />.ContainingNamespace
    /// {
    ///     <paramref name="generatedClass" /><br/>
    /// }
    /// </code>
    /// </summary>
    /// <returns>NamespaceDeclaration</returns>
    internal static NamespaceDeclarationSyntax GetNamespaceDeclaration(ISymbol specificClass, MemberDeclarationSyntax generatedClass)
    {
        return NamespaceDeclaration(ParseName(specificClass.ContainingNamespace.ToDisplayString()))
            .AddMembers(generatedClass)
            .WithNamespaceKeyword(Token(SyntaxKind.NamespaceKeyword)
                .WithLeadingTrivia(Trivia(NullableDirectiveTrivia(Token(SyntaxKind.EnableKeyword), true))));
    }

    /// <summary>
    /// Generate the following code
    /// <code>
    /// using Microsoft.UI.Xaml;
    /// ...
    /// <br/><paramref name="generatedNamespace" />
    /// </code>
    /// </summary>
    /// <returns>CompilationUnit</returns>
    internal static CompilationUnitSyntax GetCompilationUnit(MemberDeclarationSyntax generatedNamespace, IEnumerable<string> namespaces)
    {
        return CompilationUnit()
            .AddMembers(generatedNamespace)
            .AddUsings(namespaces.Select(ns => UsingDirective(ParseName(ns))).ToArray())
            .NormalizeWhitespace();
    }

    /// <summary>
    /// Generate the following code
    /// <code>
    /// <paramref name="typeSymbol" />&lt;<paramref name="isNullable" />&gt;
    /// </code>
    /// </summary>
    /// <returns>CompilationUnit</returns>
    private static TypeSyntax GetTypeSyntax(this ITypeSymbol typeSymbol, bool isNullable)
    {
        var typeName = ParseTypeName(typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
        return isNullable ? NullableType(typeName) : typeName;
    }

    #endregion

    /// <summary>
    /// 缩进（伪tab）
    /// </summary>
    /// <param name="n">tab数量</param>
    /// <returns>4n个space</returns>
    internal static string Spacing(int n)
    {
        var temp = "";
        for (var i = 0; i < n; i++)
            temp += "    ";
        return temp;
    }

    /// <summary>
    /// 获取某type的namespace并加入namespaces集合
    /// </summary>
    /// <param name="namespaces">namespaces集合</param>
    /// <param name="usedTypes">已判断过的types</param>
    /// <param name="baseClass">type的基类</param>
    /// <param name="symbol">type的symbol</param>
    internal static void UseNamespace(this HashSet<string> namespaces, HashSet<ITypeSymbol> usedTypes, INamedTypeSymbol baseClass, ITypeSymbol symbol)
    {
        if (usedTypes.Contains(symbol))
        {
            return;
        }

        usedTypes.Add(symbol);

        var ns = symbol.ContainingNamespace;
        if (!SymbolEqualityComparer.Default.Equals(ns, baseClass.ContainingNamespace))
        {
            namespaces.Add(ns.ToDisplayString());
        }

        if (symbol is INamedTypeSymbol { IsGenericType: true } genericSymbol)
        {
            foreach (var a in genericSymbol.TypeArguments)
            {
                UseNamespace(namespaces, usedTypes, baseClass, a);
            }
        }
    }
}