using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Pixeval.SourceGen
{
    [Generator(LanguageNames.CSharp)]
    public class StructuralDisposalHookAnalyzer : IIncrementalGenerator
    {
        public const string DiagnosticId = "PE1001";

        private const string TargetInterfaceName = "Pixeval.Controls.IStructuralDisposalCompleter";
        private const string TargetMethodName = "Hook";

        private const string Category = "Design";

        private const string Title = "Implementation of IStructuralDisposalCompleter does not call Hook() function";
        private const string Description = "The implementor of IStructuralDisposalCompleter must call its Hook() function in order for the structural control of disposal to be chained";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var compilation = context.CompilationProvider;

            var types = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax or StructDeclarationSyntax,
                transform: static (syntaxContext, _) => (TypeDeclarationSyntax) syntaxContext.Node);



            var targetInterface = compilation.Select(((compilation1, token) =>
                compilation1.GetTypeByMetadataName(TargetInterfaceName)!));

            var typesImplementTargetInterface = types.Combine(compilation.Combine(targetInterface)).Where(tuple =>
            {
                var typeDeclarationSyntax = tuple.Left;
                var semanticModel = tuple.Right.Left.GetSemanticModel(typeDeclarationSyntax.SyntaxTree);
                var targetInterface1 = tuple.Right.Right;
                var typeSymbol = semanticModel.GetDeclaredSymbol(typeDeclarationSyntax);
                if (typeSymbol is null)
                {
                    return false;
                }

                if (typeSymbol.AllInterfaces.Contains(targetInterface1, SymbolEqualityComparer.Default))
                {
                    return true;
                }

                return false;
            }).Select((tuple, _) => tuple.Left);

            var invocations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, _) => node is InvocationExpressionSyntax,
                    transform: static (syntaxContext, _) => (InvocationExpressionSyntax) syntaxContext.Node);

            var targetMethod = targetInterface.Select((interface1, _) => interface1.GetMembers(TargetMethodName).Single());

            var invocationsWithTargetMethod = invocations.Combine(compilation.Combine(targetMethod)).Where(tuple =>
            {
                var invocation = tuple.Left;
                var semanticModel = tuple.Right.Left.GetSemanticModel(invocation.SyntaxTree);
                var targetMethod1 = tuple.Right.Right;
                var symbolInfo = semanticModel.GetSymbolInfo(invocation);
                if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
                {
                    return SymbolEqualityComparer.Default.Equals(methodSymbol, targetMethod1);
                }

                return false;
            }).Select((tuple, _) => tuple.Left);

            var typesWithTargetMethodInvocation = typesImplementTargetInterface.Combine(invocationsWithTargetMethod.Collect()).Where(tuple =>
            {
                var typeDeclarationSyntax = tuple.Left;
                var invocations1 = tuple.Right;
                return invocations1.IsEmpty || !invocations1.Any(invocation =>
                    invocation.FirstAncestorOrSelf<TypeDeclarationSyntax>(node => node == typeDeclarationSyntax) is not null);
            }).Select((tuple, _) => tuple.Left).Combine(compilation);

            // 生成诊断信息
            context.RegisterSourceOutput(typesWithTargetMethodInvocation, (spc, tuple) =>
            {
                var typeDeclarationSyntax = tuple.Left;
                var semanticModel = tuple.Right.GetSemanticModel(typeDeclarationSyntax.SyntaxTree);
                var diagnostic = Diagnostic.Create(
                    new DiagnosticDescriptor(
                        id: DiagnosticId,
                        title: Title,
                        messageFormat: Description,
                        category: Category,
                        defaultSeverity: DiagnosticSeverity.Error,
                        isEnabledByDefault: true),
                    typeDeclarationSyntax.GetLocation(), DiagnosticSeverity.Error);

                spc.ReportDiagnostic(diagnostic);
            });
        }

        private record MethodCallContext(InvocationExpressionSyntax Invocation, IMethodSymbol MethodSymbol)
        {
            public SemanticModel SemanticModel { get; init; } = null!;
        }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSemanticModelAction(AnalyzeDocument);
        }

        private static void AnalyzeDocument(SemanticModelAnalysisContext context)
        {
            var descendantTypes = context.FilterTree.GetRoot().DescendantNodes().Where(node => node.IsKind(SyntaxKind.ClassDeclaration)
                                                                                                             || node.IsKind(SyntaxKind.RecordDeclaration)
                                                                                                             || node.IsKind(SyntaxKind.StructDeclaration)
                                                                                                             || node.IsKind(SyntaxKind.RecordStructDeclaration))
                .Where(typeDecl => context.SemanticModel.GetDeclaredSymbol(typeDecl) is INamedTypeSymbol symbol &&
                                   symbol.Interfaces.Any(sym => sym.ToString() == TargetInterfaceName))
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
                        if (typeInfo.Type?.OriginalDefinition.GetFullMetadataName() == TargetInterfaceName
                            && expr.ArgumentList.Arguments.Count == 0
                            && rhs.Identifier.Text == TargetMethodName)
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
