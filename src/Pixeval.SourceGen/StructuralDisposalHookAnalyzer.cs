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

        private const string Title = "{0} does not call Hook() function";
        private const string Message = "The class {0} should call its Hook() function in order for the structural control of disposal to be chained";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var compilation = context.CompilationProvider;

            var typeDeclarations = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax or StructDeclarationSyntax,
                transform: static (syntaxContext, _) => (TypeDeclarationSyntax) syntaxContext.Node);

            var typeSymbols = context.CompilationProvider.SelectMany(((compilation1, token) =>
            {
                return GetNamedTypeSymbols(compilation1);
                static IEnumerable<INamedTypeSymbol> GetNamedTypeSymbols(Compilation compilation)
                {
                    var stack = new Stack<INamespaceSymbol>();
                    stack.Push(compilation.GlobalNamespace);

                    while (stack.Count > 0)
                    {
                        var @namespace = stack.Pop();

                        foreach (var member in @namespace.GetMembers())
                        {
                            if (member is INamespaceSymbol memberAsNamespace)
                            {
                                stack.Push(memberAsNamespace);
                            }
                            else if (member is INamedTypeSymbol memberAsNamedTypeSymbol)
                            {
                                yield return memberAsNamedTypeSymbol;
                            }
                        }
                    }
                }
            }));

            var targetInterface = compilation.Select(((compilation1, _) =>
                compilation1.GetTypeByMetadataName(TargetInterfaceName)!));

            var typeSymbolsImplementTargetInterface = typeSymbols.Combine(targetInterface)
                .Where(tuple => tuple.Left.AllInterfaces.Contains(tuple.Right, SymbolEqualityComparer.Default)).Select((tuple, _) => tuple.Left);

            var typesImplementTargetInterface = typeDeclarations.Combine(compilation.Combine(targetInterface)).Select(
                (tuple, _) =>
                {
                    var typeDeclarationSyntax = tuple.Left;
                    var semanticModel = tuple.Right.Left.GetSemanticModel(typeDeclarationSyntax.SyntaxTree);
                    var targetInterface1 = tuple.Right.Right;
                    var typeSymbol = semanticModel.GetDeclaredSymbol(typeDeclarationSyntax);
                    return (typeDeclarationSyntax, typeSymbol, targetInterface1);
                }).Where(tuple =>
            {

                if (tuple.typeSymbol is null)
                {
                    return false;
                }

                if (tuple.typeSymbol.AllInterfaces.Contains(tuple.targetInterface1, SymbolEqualityComparer.Default))
                {
                    return true;
                }

                return false;
            }).Select((tuple, _) => (tuple.typeDeclarationSyntax, tuple.typeSymbol));

            var invocations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, _) => node is InvocationExpressionSyntax,
                    transform: static (syntaxContext, _) => (InvocationExpressionSyntax) syntaxContext.Node);

            var targetMethod =
                targetInterface.Select((interface1, _) => interface1.GetMembers(TargetMethodName).Single());

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

            var typesWithTargetMethodInvocation = typeSymbolsImplementTargetInterface
                .Combine((typesImplementTargetInterface.Collect().Combine(invocationsWithTargetMethod.Collect())))
                .Where(tuple =>
                {
                    var typeSymbol = tuple.Left;
                    var types = tuple.Right.Left;
                    var invocations1 = tuple.Right.Right;
                    if (invocations1.IsEmpty)
                    {
                        return true;
                    }

                    var typeDeclarations1 =
                        types.Where(tuple1 => SymbolEqualityComparer.Default.Equals(tuple1.typeSymbol, typeSymbol))
                            .Select(tuple1 => tuple1.typeDeclarationSyntax).ToImmutableArray();
                    return !typeDeclarations1.Any(typeDeclaration => invocations1.Any(invocation =>
                        invocation.FirstAncestorOrSelf<TypeDeclarationSyntax>(node => node == typeDeclaration) is not
                            null));
                }).Select((tuple, _) => tuple.Left).Combine(compilation);

            context.RegisterSourceOutput(typesWithTargetMethodInvocation, (spc, tuple) =>
            {
                var typeSymbol = tuple.Left;
                var diagnostic = Diagnostic.Create(
                    new DiagnosticDescriptor(
                        id: DiagnosticId,
                        title: Title,
                        messageFormat: Message,
                        category: Category,
                        defaultSeverity: DiagnosticSeverity.Error,
                        isEnabledByDefault: true),
                    Location.None, typeSymbol.Name);

                spc.ReportDiagnostic(diagnostic);
            });
        }
    }
}
