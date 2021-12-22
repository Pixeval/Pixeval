using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Pixeval.SourceGen.DependencyProperty
{
    internal static class GeneratorHelpers
    {
        public static TypeSyntax GetTypeSyntax(this ITypeSymbol typeSymbol, bool isNullable)
        {
            var typeName = SyntaxFactory.ParseTypeName(typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
            return isNullable ? SyntaxFactory.NullableType(typeName) : typeName;
        }

        public static void UseNamespace(this HashSet<string> namespaces, HashSet<ITypeSymbol> usedTypes, INamedTypeSymbol baseClass, ITypeSymbol symbol)
        {
            if (usedTypes.Contains(symbol))
                return;

            usedTypes.Add(symbol);

            var ns = symbol.ContainingNamespace;
            if (!SymbolEqualityComparer.Default.Equals(ns, baseClass.ContainingNamespace))
            {
                namespaces.Add(ns.ToDisplayString());
            }

            if (symbol is INamedTypeSymbol { IsGenericType: true } genericSymbol)
            {
                foreach (var a in genericSymbol.TypeArguments)
                {
                    UseNamespace(namespaces, usedTypes, baseClass, a);
                }
            }
        }
    }
}