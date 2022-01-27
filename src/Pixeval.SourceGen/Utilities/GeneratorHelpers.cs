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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Pixeval.SourceGen.Utilities;

internal static class GeneratorHelpers
{
    public static TypeSyntax GetTypeSyntax(this ITypeSymbol typeSymbol, bool isNullable)
    {
        var typeName = SyntaxFactory.ParseTypeName(typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
        return isNullable ? SyntaxFactory.NullableType(typeName) : typeName;
    }

    /// <summary>
    /// 添加命名空间列表
    /// </summary>
    /// <param name="namespaces">已包含命名空间</param>
    /// <param name="usedTypes">已记录过的类型</param>
    /// <param name="symbol">判断是否为新类型</param>
    public static void UseNamespace(this HashSet<string> namespaces, HashSet<ITypeSymbol> usedTypes, ITypeSymbol symbol)
    {
        if (usedTypes.Contains(symbol))
        {
            return;
        }

        usedTypes.Add(symbol);

        _ = namespaces.Add(symbol.ContainingNamespace.ToDisplayString());

        if (symbol is INamedTypeSymbol { IsGenericType: true } genericSymbol)
        {
            foreach (var a in genericSymbol.TypeArguments)
            {
                UseNamespace(namespaces, usedTypes, a);
            }
        }
    }
    public static string Spacing(int n)
    {
        var temp = "";
        for (var i = 0; i < n; i++)
            temp += "    ";
        return temp;
    }
}