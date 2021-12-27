#region Copyright (c) Pixeval/Pixeval.SourceGen
// GPL v3 License
// 
// Pixeval/Pixeval.SourceGen
// Copyright (c) 2021 Pixeval.SourceGen/GetAttributeGenerator.cs
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

namespace Pixeval.SourceGen.Utilities;

internal abstract class GetAttributeGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new AttributeReceiver(AttributePath));
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.Compilation.GetTypeByMetadataName(AttributePath) is not { } attributeType)
        {
            return;
        }

        foreach (var typeDeclaration in ((AttributeReceiver)context.SyntaxContextReceiver!).CandidateTypes)
        {
            var semanticModel = context.Compilation.GetSemanticModel(typeDeclaration.SyntaxTree);

            if (semanticModel.GetDeclaredSymbol(typeDeclaration) is not { } specificType)
            {
                continue;
            }

            ExecuteForEach(context, attributeType, typeDeclaration, specificType);
        }
    }

    protected abstract string AttributePathGetter();

    protected abstract void ExecuteForEach(GeneratorExecutionContext context, INamedTypeSymbol attributeType, TypeDeclarationSyntax typeDeclaration, INamedTypeSymbol specificType);

    private string AttributePath => AttributePathGetter();
}