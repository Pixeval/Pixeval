using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
namespace Pixeval.SourceGen;

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