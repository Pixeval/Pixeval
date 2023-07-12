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
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Pixeval.SourceGen;

public static class SyntaxHelper
{
    public static string? FullQualifiedName(this SemanticModel semModel, AttributeSyntax syntax)
    {
        if (semModel.GetTypeInfo(syntax).Type is { ContainingNamespace: var ns, ContainingType: var ty, Name: var name })
        {
            return ty != null ? $"{semModel.FullQualifiedName(ty)}.{name}" : $"{ns}.{name}";
        }

        return null;
    }

    public static string FullQualifiedName(this SemanticModel semModel, INamedTypeSymbol sym)
    {
        return sym.ContainingType is null ? $"global::{sym.ContainingNamespace}.{sym.Name}" : $"{semModel.FullQualifiedName(sym.ContainingType)}.{sym.Name}";
    }

    public static bool HasAttribute(this MemberDeclarationSyntax mds, SemanticModel semanticModel, string attributeFqName)
    {
        return mds.AttributeLists.Any(als => als.Attributes.Any(attr => semanticModel.FullQualifiedName(attr) == attributeFqName));
    }

    public static AttributeSyntax? GetAttribute(this MemberDeclarationSyntax mds, SemanticModel semanticModel, string attributeFqName)
    {
        return mds.AttributeLists.SelectMany(t => t.Attributes).FirstOrDefault(attr => semanticModel.FullQualifiedName(attr) == attributeFqName);
    }

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
    /// 获取某<paramref name="symbol"/>的namespace并加入<paramref name="namespaces"/>集合
    /// </summary>
    /// <param name="namespaces">namespaces集合</param>
    /// <param name="usedTypes">已判断过的types</param>
    /// <param name="contextType">上下文所在的类</param>
    /// <param name="symbol">type的symbol</param>
    internal static void UseNamespace(this HashSet<string> namespaces, HashSet<ITypeSymbol> usedTypes, INamedTypeSymbol contextType, ITypeSymbol symbol)
    {
        if (usedTypes.Contains(symbol))
            return;

        _ = usedTypes.Add(symbol);

        var ns = symbol.ContainingNamespace;
        if (ns.IsGlobalNamespace)
        {
            _ = namespaces.Add(contextType.ContainingNamespace.ToDisplayString());
        }
        else if (!SymbolEqualityComparer.Default.Equals(ns, contextType.ContainingNamespace))
        {
            _ = namespaces.Add(ns.ToDisplayString());
        }

        if (symbol is INamedTypeSymbol { IsGenericType: true } genericSymbol)
            foreach (var a in genericSymbol.TypeArguments)
                UseNamespace(namespaces, usedTypes, contextType, a);
    }

    /// <summary>
    /// 生成nullable预处理语句和引用命名空间
    /// <br/>#nullable enable
    /// <br/><see langword="using"/> ...;
    /// <br/><see langword="using"/> ...;
    /// <br/>...
    /// </summary>
    /// <param name="namespaces">namespaces集合</param>
    internal static StringBuilder GenerateFileHeader(this HashSet<string> namespaces)
    {
        var stringBuilder = new StringBuilder().AppendLine("#nullable enable\n");
        foreach (var s in namespaces)
            _ = stringBuilder.AppendLine($"using {s};");
        return stringBuilder;
    }
}
