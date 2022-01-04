#region Copyright (c) Pixeval/Pixeval.SourceGen
// GPL v3 License
// 
// Pixeval/Pixeval.SourceGen
// Copyright (c) 2021 Pixeval.SourceGen/GenerateConstructorGenerator.cs
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
using System.Text;
using Pixeval.SourceGen.Utilities;

namespace Pixeval.SourceGen;

[Generator]
internal class GenerateConstructorGenerator : GetAttributeGenerator
{
    protected override string AttributePathGetter() => "Pixeval.Attributes.GenerateConstructorAttribute";

    protected override void ExecuteForEach(GeneratorExecutionContext context, INamedTypeSymbol attributeType, TypeDeclarationSyntax typeDeclaration, INamedTypeSymbol specificType)
    {
        var name = specificType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        var namespaces = new HashSet<string>();
        var usedTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        var ctor = SyntaxFactory.ConstructorDeclaration(name)
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));
        foreach (var member in specificType.GetMembers().Where(member =>
                         member is { Kind: SymbolKind.Property } and not { Name: "EqualityContract" })
                     .Cast<IPropertySymbol>())
        {
            ctor = GetDeclaration(member, ctor);
            namespaces.UseNamespace(usedTypes, specificType, member.Type);
        }

        var generatedType = GetDeclaration(name, typeDeclaration, ctor);
        var generatedNamespace = GetNamespaceDeclaration(specificType, generatedType);
        var compilationUnit = GetCompilationUnit(generatedNamespace, namespaces);
        var fileName = specificType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat
            .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)) + ".g.cs";
        context.AddSource(fileName, SyntaxFactory.SyntaxTree(compilationUnit, encoding: Encoding.UTF8).GetText());
    }

    /// <summary>
    ///     生成如下代码
    ///     <code>
    /// public <paramref name="ctor" /> (...<paramref name="property" />.Type variable...) <br />
    /// {<br /> <paramref name="property" />.Name = variable;<br />}
    /// </code>
    /// </summary>
    /// <returns></returns>
    private static ConstructorDeclarationSyntax GetDeclaration(IPropertySymbol property, ConstructorDeclarationSyntax ctor)
    {
        var newName = property.Name.Substring(0, 1).ToLower() + property.Name.Substring(1);
        return ctor.AddParameterListParameters(
                SyntaxFactory.Parameter(SyntaxFactory.Identifier(newName)).WithType(property.Type.GetTypeSyntax(false)))
            .AddBodyStatements(SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName(property.Name),
                    SyntaxFactory.IdentifierName(newName))));
    }

    /// <summary>
    ///     生成如下代码
    ///     <code>
    /// partial <paramref name="typeDeclaration" /> <paramref name="name" /> <br />
    /// {<br /> <paramref name="member" /><br />}
    /// </code>
    /// </summary>
    /// <returns>TypeDeclaration</returns>
    private static TypeDeclarationSyntax GetDeclaration(string name, TypeDeclarationSyntax typeDeclaration, MemberDeclarationSyntax member)
    {
        TypeDeclarationSyntax typeDeclarationTemp = typeDeclaration switch
        {
            ClassDeclarationSyntax => SyntaxFactory.ClassDeclaration(name),
            StructDeclarationSyntax => SyntaxFactory.StructDeclaration(name),
            RecordDeclarationSyntax => SyntaxFactory.RecordDeclaration(SyntaxFactory.Token(SyntaxKind.RecordKeyword), name),
            _ => throw new ArgumentOutOfRangeException(nameof(typeDeclaration))
        };
        return typeDeclarationTemp.AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword))
            .WithOpenBraceToken(SyntaxFactory.Token(SyntaxKind.OpenBraceToken))
            .AddMembers(member)
            .WithCloseBraceToken(SyntaxFactory.Token(SyntaxKind.CloseBraceToken));
    }

    /// <summary>
    ///     生成如下代码
    ///     <code>
    /// namespace <paramref name="specificClass" />.ContainingNamespace<br />
    /// {<br /> <paramref name="generatedClass" /><br />}
    /// </code>
    /// </summary>
    /// <returns>NamespaceDeclaration</returns>
    private static NamespaceDeclarationSyntax GetNamespaceDeclaration(ISymbol specificClass, MemberDeclarationSyntax generatedClass)
    {
        return SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(specificClass.ContainingNamespace.ToDisplayString()))
            .AddMembers(generatedClass)
            .WithNamespaceKeyword(SyntaxFactory.Token(SyntaxKind.NamespaceKeyword)
                .WithLeadingTrivia(SyntaxFactory.Trivia(SyntaxFactory.NullableDirectiveTrivia(SyntaxFactory.Token(SyntaxKind.EnableKeyword), true))));
    }

    /// <summary>
    ///     生成如下代码
    ///     <code>
    /// using Microsoft.UI.Xaml;<br />
    /// ...<br />
    /// <paramref name="generatedNamespace" />
    /// </code>
    /// </summary>
    /// <returns>CompilationUnit</returns>
    private static CompilationUnitSyntax GetCompilationUnit(MemberDeclarationSyntax generatedNamespace, IEnumerable<string> namespaces)
    {
        return SyntaxFactory.CompilationUnit()
            .AddMembers(generatedNamespace)
            .AddUsings(namespaces.Select(ns => SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(ns))).ToArray())
            .NormalizeWhitespace();
    }
}