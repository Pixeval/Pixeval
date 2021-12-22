using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Pixeval.SourceGen.DependencyProperty;

internal class AttributeReceiver : ISyntaxContextReceiver
{
    private readonly List<ClassDeclarationSyntax> _candidateClasses = new();
    private readonly string _attributeName;

    public IReadOnlyList<ClassDeclarationSyntax> CandidateClasses => _candidateClasses;

    private INamedTypeSymbol? _attributeSymbol;

    public AttributeReceiver(string attributeName) => _attributeName = attributeName;

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        _attributeSymbol ??= context.SemanticModel.Compilation.GetTypeByMetadataName(_attributeName);

        if (_attributeSymbol is null)
            return;

        if (context.Node is ClassDeclarationSyntax classDeclaration && (from l in classDeclaration.AttributeLists
                from attribute in l.Attributes
                select context.SemanticModel.GetSymbolInfo(attribute)).Any(symbolInfo => SymbolEqualityComparer.Default.Equals(symbolInfo.Symbol?.ContainingType, _attributeSymbol)))
        {
            _candidateClasses.Add(classDeclaration);
        }
    }
}