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

using System.Linq;
using Microsoft.CodeAnalysis;
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
        return sym.ContainingType is null ? $"{sym.ContainingNamespace}.{sym.Name}" : $"{semModel.FullQualifiedName(sym.ContainingType)}.{sym.Name}";
    }

    public static bool HasAttribute(this MemberDeclarationSyntax mds, SemanticModel semanticModel, string attributeFqName)
    {
        return mds.AttributeLists.Any(als => als.Attributes.Any(attr => semanticModel.FullQualifiedName(attr) == attributeFqName));
    }
}