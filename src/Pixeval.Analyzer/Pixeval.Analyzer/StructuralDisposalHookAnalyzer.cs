using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Pixeval.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class StructuralDisposalHookAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "PE1001";

        private const string InterfaceFqName = "Pixeval.Controls.IStructuralDisposalCompleter";
        private const string HookFunctionName = "Hook";

        private const string Category = "Design";

        private const string Title = "Implementation of IStructuralDisposalCompleter does not call Hook() function";
        private const string Description = "The implementor of IStructuralDisposalCompleter must call its Hook() function in order for the structural control of disposal to be chained";
        private static readonly DiagnosticDescriptor _Rule = new(DiagnosticId, Title, Description, Category, DiagnosticSeverity.Error, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [_Rule];

        public override void Initialize(AnalysisContext context)
        {
            // Debugger.Launch();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSemanticModelAction(AnalyzeDocument);
        }

        private static void AnalyzeDocument(SemanticModelAnalysisContext context)
        {
            var descendantTypes = context.SemanticModel.SyntaxTree.GetRoot().DescendantNodes().Where(node => node.IsKind(SyntaxKind.ClassDeclaration)
                                                                                                             || node.IsKind(SyntaxKind.RecordDeclaration)
                                                                                                             || node.IsKind(SyntaxKind.StructDeclaration)
                                                                                                             || node.IsKind(SyntaxKind.RecordStructDeclaration))
                .Where(typeDecl => context.SemanticModel.GetDeclaredSymbol(typeDecl) is INamedTypeSymbol symbol &&
                                   symbol.Interfaces.Any(sym => sym.GetFullMetadataName() == InterfaceFqName))
                .Cast<TypeDeclarationSyntax>();

            foreach (var typeDecl in descendantTypes)
            {
                var dictionary = new Dictionary<TypeDeclarationSyntax, (bool, Location)>();
                if (!dictionary.ContainsKey(typeDecl))
                {
                    dictionary[typeDecl] = (false, typeDecl.Identifier.GetLocation());
                }

                var invocationNodes = typeDecl.DescendantNodes()
                    .Where(node => node is InvocationExpressionSyntax)
                    .Cast<InvocationExpressionSyntax>();
                foreach (var expr in invocationNodes)
                {
                    if (expr.Expression is MemberAccessExpressionSyntax memberAccess)
                    {
                        var lhs = memberAccess.Expression;
                        var rhs = memberAccess.Name;
                        var typeInfo = context.SemanticModel.GetTypeInfo(lhs);
                        if (typeInfo.Type?.OriginalDefinition.GetFullMetadataName() == InterfaceFqName
                            && expr.ArgumentList.Arguments.Count == 0
                            && rhs.Identifier.Text == HookFunctionName)
                        {
                            dictionary[typeDecl] = (true, null);
                        }
                    }
                }

                foreach (var pair in dictionary.Where(tuple => !tuple.Value.Item1))
                {
                    context.ReportDiagnostic(Diagnostic.Create(_Rule, pair.Value.Item2));
                }
            }
        }
    }
}
