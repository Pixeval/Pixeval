#region Copyright (c) Pixeval/Pixeval.SourceGen
// GPL v3 License
// 
// Pixeval/Pixeval.SourceGen
// Copyright (c) 2021 Pixeval.SourceGen/GeneratorHelpers.cs
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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Pixeval.SourceGen;

internal static class GeneratorHelpers
{
    public static TypeSyntax GetTypeSyntax(this ITypeSymbol typeSymbol, bool isNullable)
    {
        var typeName = SyntaxFactory.ParseTypeName(typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
        return isNullable ? SyntaxFactory.NullableType(typeName) : typeName;
    }

    public static void UseNamespace(this HashSet<string> namespaces, HashSet<ITypeSymbol> usedTypes, INamedTypeSymbol baseClass, ITypeSymbol symbol)
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