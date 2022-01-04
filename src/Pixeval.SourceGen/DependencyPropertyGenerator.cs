#region Copyright (c) Pixeval/Pixeval.SourceGen
// GPL v3 License
// 
// Pixeval/Pixeval.SourceGen
// Copyright (c) 2021 Pixeval.SourceGen/DependencyPropertyGenerator.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pixeval.SourceGen.Utilities;

namespace Pixeval.SourceGen;

[Generator]
internal class DependencyPropertyGenerator : GetAttributeGenerator
{
    protected override string AttributePathGetter() => "Pixeval.Attributes.DependencyPropertyAttribute";

    protected override void ExecuteForEach(GeneratorExecutionContext context, INamedTypeSymbol attributeType, TypeDeclarationSyntax typeDeclaration, INamedTypeSymbol specificClass)
    {

        var members = new List<MemberDeclarationSyntax>();
        var namespaces = new HashSet<string> { "Microsoft.UI.Xaml" };
        var usedTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

        foreach (var attribute in specificClass.GetAttributes().Where(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, attributeType)))
        {
            if (attribute.ConstructorArguments[0].Value is not string propertyName || attribute.ConstructorArguments[1].Value is not INamedTypeSymbol type)
            {
                continue;
            }

            if (attribute.ConstructorArguments.Length < 3 || attribute.ConstructorArguments[2].Value is not string propertyChanged)
            {
                continue;
            }

            var isSetterPublic = true;
            var defaultValue = "DependencyProperty.UnsetValue";
            var isNullable = false;

            foreach (var namedArgument in attribute.NamedArguments)
            {
                if (namedArgument.Value.Value is { } value)
                {
                    switch (namedArgument.Key)
                    {
                        case "IsSetterPublic":
                            isSetterPublic = (bool)value;
                            break;
                        case "DefaultValue":
                            defaultValue = (string)value;
                            break;
                        case "IsNullable":
                            isNullable = (bool)value;
                            break;
                    }
                }
            }

            var fieldName = propertyName + "Property";

            namespaces.UseNamespace(usedTypes, specificClass, type);
            var defaultValueExpression = SyntaxFactory.ParseExpression(defaultValue);
            var metadataCreation = GetObjectCreationExpression(defaultValueExpression);
            if (propertyChanged is not "")
            {
                metadataCreation = GetMetadataCreation(metadataCreation, propertyChanged);
            }

            var registration = GetRegistration(propertyName, type, specificClass, metadataCreation);
            var staticFieldDeclaration = GetStaticFieldDeclaration(fieldName, registration);
            var getter = GetGetter(fieldName, isNullable, type, context);
            var setter = GetSetter(fieldName, isSetterPublic);
            var propertyDeclaration = GetPropertyDeclaration(propertyName, isNullable, type, getter, setter);

            members.Add(staticFieldDeclaration);
            members.Add(propertyDeclaration);
        }

        if (members.Count > 0)
        {
            var generatedClass = GetClassDeclaration(specificClass, members);
            var generatedNamespace = GetNamespaceDeclaration(specificClass, generatedClass);
            var compilationUnit = GetCompilationUnit(generatedNamespace, namespaces);
            var fileName = specificClass.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat
                .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)) + ".g.cs";
            context.AddSource(fileName, SyntaxFactory.SyntaxTree(compilationUnit, encoding: Encoding.UTF8).GetText());
        }
    }


    /// <summary>
    ///     生成如下代码
    ///     <code>
    /// new PropertyMetadata(<paramref name="defaultValueExpression" />);
    /// </code>
    /// </summary>
    /// <returns>ObjectCreationExpression</returns>
    private static ObjectCreationExpressionSyntax GetObjectCreationExpression(ExpressionSyntax defaultValueExpression)
    {
        return SyntaxFactory.ObjectCreationExpression(SyntaxFactory.IdentifierName("PropertyMetadata"))
            .AddArgumentListArguments(SyntaxFactory.Argument(defaultValueExpression));
    }

    /// <summary>
    ///     生成如下代码
    ///     <code>
    /// new PropertyMetadata(<paramref name="metadataCreation" />, <paramref name="partialMethodName" />)
    /// </code>
    /// </summary>
    /// <returns>MetadataCreation</returns>
    private static ObjectCreationExpressionSyntax GetMetadataCreation(ObjectCreationExpressionSyntax metadataCreation, string partialMethodName)
    {
        return metadataCreation.AddArgumentListArguments(SyntaxFactory.Argument(SyntaxFactory.IdentifierName(partialMethodName)));
    }

    /// <summary>
    ///     生成如下代码
    ///     <code>
    /// DependencyProperty.Register("<paramref name="propertyName" />", typeof(<paramref name="type" />), typeof(<paramref
    ///             name="specificClass" />), <paramref name="metadataCreation" />);
    /// </code>
    /// </summary>
    /// <returns>Registration</returns>
    private static InvocationExpressionSyntax GetRegistration(string propertyName, ITypeSymbol type, ITypeSymbol specificClass, ExpressionSyntax metadataCreation)
    {
        return SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName("DependencyProperty"), SyntaxFactory.IdentifierName("Register")))
            .AddArgumentListArguments(SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(propertyName))), SyntaxFactory.Argument(SyntaxFactory.TypeOfExpression(type.GetTypeSyntax(false))), SyntaxFactory.Argument(SyntaxFactory.TypeOfExpression(specificClass.GetTypeSyntax(false))), SyntaxFactory.Argument(metadataCreation));
    }

    /// <summary>
    ///     生成如下代码
    ///     <code>
    /// public static readonly DependencyProperty <paramref name="fieldName" /> = <paramref name="registration" />;
    /// </code>
    /// </summary>
    /// <returns>StaticFieldDeclaration</returns>
    private static FieldDeclarationSyntax GetStaticFieldDeclaration(string fieldName, ExpressionSyntax registration)
    {
        return SyntaxFactory.FieldDeclaration(SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("DependencyProperty")))
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword), SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword))
            .AddDeclarationVariables(SyntaxFactory.VariableDeclarator(fieldName).WithInitializer(SyntaxFactory.EqualsValueClause(registration)));
    }

    /// <summary>
    ///     生成如下代码
    ///     <code>
    /// get => (<paramref name="type" />&lt;<paramref name="isNullable" />&gt;)GetValue(<paramref name="fieldName" />);
    /// </code>
    /// </summary>
    /// <returns>Getter</returns>
    private static AccessorDeclarationSyntax GetGetter(string fieldName, bool isNullable, ITypeSymbol type, GeneratorExecutionContext context)
    {
        ExpressionSyntax getProperty = SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName("GetValue"))
            .AddArgumentListArguments(SyntaxFactory.Argument(SyntaxFactory.IdentifierName(fieldName)));
        if (!SymbolEqualityComparer.Default.Equals(type, context.Compilation.GetSpecialType(SpecialType.System_Object)))
        {
            getProperty = SyntaxFactory.CastExpression(type.GetTypeSyntax(isNullable), getProperty);
        }

        return SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
            .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(getProperty))
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
    }

    /// <summary>
    ///     生成如下代码
    ///     <code>
    /// &lt;<paramref name="isSetterPublic" />&gt; set => SetValue(<paramref name="fieldName" />, value);
    /// </code>
    /// </summary>
    /// <returns>Setter</returns>
    private static AccessorDeclarationSyntax GetSetter(string fieldName, bool isSetterPublic)
    {
        ExpressionSyntax setProperty = SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName("SetValue"))
            .AddArgumentListArguments(SyntaxFactory.Argument(SyntaxFactory.IdentifierName(fieldName)), SyntaxFactory.Argument(SyntaxFactory.IdentifierName("value")));
        var setter = SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
            .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(setProperty))
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        return !isSetterPublic ? setter.AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword)) : setter;
    }

    /// <summary>
    ///     生成如下代码
    ///     <code>
    /// public <paramref name="type" />&lt;<paramref name="isNullable" />&gt; <paramref name="propertyName" /> { <paramref
    ///             name="getter" />; <paramref name="setter" />; }
    /// </code>
    /// </summary>
    /// <returns>PropertyDeclaration</returns>
    private static PropertyDeclarationSyntax GetPropertyDeclaration(string propertyName, bool isNullable, ITypeSymbol type, AccessorDeclarationSyntax getter, AccessorDeclarationSyntax setter)
    {
        return SyntaxFactory.PropertyDeclaration(type.GetTypeSyntax(isNullable), propertyName)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddAccessorListAccessors(getter, setter);
    }

    /// <summary>
    ///     生成如下代码
    ///     <code>
    /// partial class <paramref name="specificClass" /><br />
    /// {<br /> <paramref name="members" /><br />}
    /// </code>
    /// </summary>
    /// <returns>ClassDeclaration</returns>
    private static ClassDeclarationSyntax GetClassDeclaration(ISymbol specificClass, IEnumerable<MemberDeclarationSyntax> members)
    {
        return SyntaxFactory.ClassDeclaration(specificClass.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat))
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword))
            .AddMembers(members.ToArray());
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