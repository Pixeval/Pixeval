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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Pixeval.SourceGen.Utils;

namespace Pixeval.SourceGen;

internal static partial class TypeWithAttributeDelegates
{
    public static string GenerateConstructor(TypeDeclarationSyntax typeDeclaration, INamedTypeSymbol typeSymbol, List<AttributeData> attributeList)
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