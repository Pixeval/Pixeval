#region Copyright (c) Pixeval/Pixeval.SourceGen
// GPL v3 License
// 
// Pixeval/Pixeval.SourceGen
// Copyright (c) 2021 Pixeval.SourceGen/AttributeReceiver.cs
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

namespace Pixeval.SourceGen.Utilities;

internal class AttributeReceiver : ISyntaxContextReceiver
{
    private readonly string _attributeName;
    private readonly List<TypeDeclarationSyntax> _candidateTypes = new();

    private INamedTypeSymbol? _attributeSymbol;

    public AttributeReceiver(string attributeName)
    {
        _attributeName = attributeName;
    }

    public IReadOnlyList<TypeDeclarationSyntax> CandidateTypes => _candidateTypes;

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        _attributeSymbol ??= context.SemanticModel.Compilation.GetTypeByMetadataName(_attributeName);

        if (_attributeSymbol is null) return;

        if (context.Node is TypeDeclarationSyntax typeDeclaration && typeDeclaration.AttributeLists
                .SelectMany(l => l.Attributes, (_, attribute) => context.SemanticModel.GetSymbolInfo(attribute))
                .Any(symbolInfo => SymbolEqualityComparer.Default.Equals(symbolInfo.Symbol?.ContainingType, _attributeSymbol)))
            _candidateTypes.Add(typeDeclaration);
    }
}