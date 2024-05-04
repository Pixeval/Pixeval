#region Copyright
// GPL v3 License
// 
// Pixeval/Pixeval.SourceGen
// Copyright (c) 2024 Pixeval.SourceGen/ReadonlyInterfaceGenerator.cs
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

using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Pixeval.SourceGen;

[Generator]
public class ReadonlyInterfaceGenerator : IIncrementalGenerator
{
    private const string AttributeName = "ReadonlyInterfaceAttribute";

    private const string AttributeNamespace = nameof(Pixeval) + ".Attributes";

    private const string AttributeFullName = AttributeNamespace + "." + AttributeName;

    internal string? TypeWithAttribute(INamedTypeSymbol typeSymbol, ImmutableArray<AttributeData> attributeList)
    {
        if (attributeList is not [{ ConstructorArguments: [{ Value: string ns }, ..] }])
            return null;

        var getter = AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

        var list = typeSymbol.GetProperties(attributeList[0].AttributeClass!)
            // .Where(symbol => !symbol.IsReadOnly)
            .Select(symbol => (MemberDeclarationSyntax)PropertyDeclaration(symbol.Type.GetTypeSyntax(false), symbol.Name)
                .AddAccessorListAccessors(getter));

        var member = InterfaceDeclaration("I" + typeSymbol.Name)
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
            .WithMembers(List(list));

        var generatedNamespace = SyntaxHelper.GetFileScopedNamespaceDeclaration(ns, member, true);
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
        ).Combine(context.CompilationProvider);

        context.RegisterSourceOutput(generatorAttributes, (spc, tuple) =>
        {
            var (ga, compilation) = tuple;

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
