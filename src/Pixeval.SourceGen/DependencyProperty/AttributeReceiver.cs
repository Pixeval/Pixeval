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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Pixeval.SourceGen.DependencyProperty;

internal class AttributeReceiver : ISyntaxContextReceiver
{
    private readonly string _attributeName;
    private readonly List<ClassDeclarationSyntax> _candidateClasses = new();

    private INamedTypeSymbol? _attributeSymbol;

    public AttributeReceiver(string attributeName)
    {
        _attributeName = attributeName;
    }

    public IReadOnlyList<ClassDeclarationSyntax> CandidateClasses => _candidateClasses;

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        _attributeSymbol ??= context.SemanticModel.Compilation.GetTypeByMetadataName(_attributeName);

        if (_attributeSymbol is null)
        {
            return;
        }

        if (context.Node is ClassDeclarationSyntax classDeclaration && (from l in classDeclaration.AttributeLists
                from attribute in l.Attributes
                select context.SemanticModel.GetSymbolInfo(attribute)).Any(symbolInfo => SymbolEqualityComparer.Default.Equals(symbolInfo.Symbol?.ContainingType, _attributeSymbol)))
        {
            _candidateClasses.Add(classDeclaration);
        }
    }
}