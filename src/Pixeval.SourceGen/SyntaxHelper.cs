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

namespace Pixeval.SourceGen;

public static class SyntaxHelper
{
    internal const string AttributeNamespace = "WinUI3Utilities.Attributes.";
    internal const string DisableSourceGeneratorAttribute = AttributeNamespace + "DisableSourceGeneratorAttribute";

    public static bool HasAttribute(this ISymbol s, string attributeFqName)
    {
        return s.GetAttributes().Any(als => als.AttributeClass?.ToDisplayString() == attributeFqName);
    }

    public static AttributeData? GetAttribute(this ISymbol mds, string attributeFqName)
    {
        return mds.GetAttributes().FirstOrDefault(attr => attr?.AttributeClass?.ToDisplayString() == attributeFqName);
    }

    /// <summary>
    /// 缩进（伪tab）
    /// </summary>
    /// <param name="n">tab数量</param>
    /// <returns>4n个space</returns>
    internal static string Spacing(int n)
    {
        var temp = "";
        for (var i = 0; i < n; ++i)
            temp += "    ";
        return temp;
    }

    internal static IEnumerable<IPropertySymbol> GetProperties(this ITypeSymbol typeSymbol, INamedTypeSymbol attribute)
    {
        foreach (var member in typeSymbol.GetMembers())
        {
            if (member is not IPropertySymbol { Name: not "EqualityContract" } property)
                continue;

            if (IgnoreAttribute(property, attribute))
                continue;

            yield return property;
        }
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
}
