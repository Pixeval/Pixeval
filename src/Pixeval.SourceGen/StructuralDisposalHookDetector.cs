using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Pixeval.SourceGen
{
    [Generator(LanguageNames.CSharp)]
    public class StructuralDisposalHookDetector : IIncrementalGenerator
    {
        public const string DiagnosticId = "PE1001";

        private const string TargetInterfaceName = "Pixeval.Controls.IStructuralDisposalCompleter";
        private const string TargetMethodName = "Hook";

        private const string Category = "Design";

        private const string Title = "{0} does not call Hook() function";
        private const string Message = "The class {0} should call its Hook() function in order for the structural control of disposal to be chained";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var enableDetector = context.AnalyzerConfigOptionsProvider.Select(static (options, _) =>
                options.GlobalOptions.TryGetValue("structural_disposal_hook_detection", out var enableDetector) && enableDetector.Equals("true", StringComparison.InvariantCultureIgnoreCase));

            var compilation = context.CompilationProvider;

            var typeDeclarations = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) => node is TypeDeclarationSyntax,
                transform: static (syntaxContext, _) => syntaxContext);

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
                compilation1.GetTypeByMetadataName(TargetInterfaceName)));

            var typeSymbolsImplementTargetInterface = typeSymbols.Combine(targetInterface)
                .Where(tuple => tuple.Right is not null && tuple.Left.AllInterfaces.Contains(tuple.Right, SymbolEqualityComparer.Default)).Select((tuple, _) => tuple.Left);

            var typesImplementTargetInterface = typeDeclarations.Combine(targetInterface).Select(
                (tuple, _) =>
                {
                    var (syntaxContext, targetInterface1) = tuple;
                    var typeDeclarationSyntax = (TypeDeclarationSyntax) syntaxContext.Node;
                    var typeSymbol = syntaxContext.SemanticModel.GetDeclaredSymbol(typeDeclarationSyntax);
                    return (typeDeclarationSyntax, typeSymbol, targetInterface1);
                }).Where(tuple =>
            {

                if (tuple.typeSymbol is null || tuple.targetInterface1 is null)
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
                    transform: static (syntaxContext, _) => syntaxContext);

            var targetMethod =
                targetInterface.Select((interface1, _) => interface1?.GetMembers(TargetMethodName).SingleOrDefault());

            var invocationsWithTargetMethod = invocations.Combine(targetMethod).Where(tuple =>
            {
                var (syntaxContext, targetMethod1) = tuple;
                if (targetMethod1 is null)
                {
                    return false;
                }
                var invocation = (InvocationExpressionSyntax) syntaxContext.Node;
                var symbolInfo = syntaxContext.SemanticModel.GetSymbolInfo(invocation);
                if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
                {
                    return SymbolEqualityComparer.Default.Equals(methodSymbol, targetMethod1);
                }

                return false;
            }).Select((tuple, _) => tuple.Left.Node);

            var typesWithoutTargetMethodInvocation = typeSymbolsImplementTargetInterface
                .Combine((typesImplementTargetInterface.Collect().Combine(invocationsWithTargetMethod.Collect())))
                .Where(tuple =>
                {
                    var (typeSymbol, (types, invocations1)) = tuple;


                    var typeDeclarations1 =
                        types.Where(tuple1 => SymbolEqualityComparer.Default.Equals(tuple1.typeSymbol, typeSymbol))
                            .Select(tuple1 => tuple1.typeDeclarationSyntax).ToImmutableArray();
                    if (typeDeclarations1.IsDefaultOrEmpty)
                    {
                        return false;
                    }
                    if (invocations1.IsDefaultOrEmpty)
                    {
                        return true;
                    }
                    return !typeDeclarations1.Any(typeDeclaration => invocations1.Any(invocation =>
                        invocation.FirstAncestorOrSelf<TypeDeclarationSyntax>(node => node == typeDeclaration) is not
                            null));
                }).Select((tuple, _) => tuple.Left);

            context.RegisterSourceOutput(typesWithoutTargetMethodInvocation.Combine(enableDetector), (spc, source) =>
            {
                if (!source.Right)
                {
                    return;
                }
                var type = source.Left;
                var diagnostic = Diagnostic.Create(
                    new DiagnosticDescriptor(
                        id: DiagnosticId,
                        title: Title,
                        messageFormat: Message,
                        category: Category,
                        defaultSeverity: DiagnosticSeverity.Error,
                        isEnabledByDefault: true),
                    Location.None, type.Name);

                spc.ReportDiagnostic(diagnostic);
            });
        }
    }
}
