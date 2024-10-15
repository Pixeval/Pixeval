#region Copyright
// GPL v3 License
// 
// Pixeval/Pixeval.SourceGen
// Copyright (c) 2024 Pixeval.SourceGen/MetaPathMacroGenerator.cs
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
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using static Pixeval.SourceGen.SyntaxHelper;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Pixeval.SourceGen;

[Generator]
public class MetaPathMacroGenerator : IIncrementalGenerator
{
    private const string AttributeName = "MetaPathMacroAttribute`1";

    private const string AttributeNamespace = nameof(Pixeval) + ".Download.Macros";

    private const string AttributeFullName = AttributeNamespace + "." + AttributeName;

    internal string TypeWithAttribute(ImmutableArray<GeneratorAttributeSyntaxContext> gas, out string fileName)
    {
        var dictionary = new Dictionary<ITypeSymbol, List<INamedTypeSymbol>>(SymbolEqualityComparer.Default);
        foreach (var ga in gas)
        {
            if (ga is not { TargetSymbol: INamedTypeSymbol symbol, Attributes: var attributeList })
                continue;
            foreach (var attributeData in attributeList)
                if (attributeData.AttributeClass?.TypeArguments is [{ } type])
                    if (dictionary.TryGetValue(type, out var list))
                        list.Add(symbol);
                    else
                        dictionary[type] = [symbol];
        }

        const string getAttachedTypeInstances = "Get{0}Instances";

        fileName = "MetaPathMacroAttributeHelper.g.cs";
        var generatedType = ClassDeclaration("MetaPathMacroAttributeHelper")
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
            .WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken))
            .AddMembers(dictionary.Select(t =>
                    (MemberDeclarationSyntax)MethodDeclaration(
                            ParseTypeName("global::System.Collections.Generic.IReadOnlyList<global::Pixeval.Download.MacroParser.IMacro>"),
                            string.Format(getAttachedTypeInstances, t.Key.Name))
                        .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                        .WithExpressionBody(ArrowExpressionClause(CollectionExpression(SeparatedList(t.Value.Select(v =>
                            (CollectionElementSyntax)ExpressionElement(ObjectCreationExpression(v.GetTypeSyntax(false))
                                .WithArgumentList(ArgumentList()))))
                        )))
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))).ToArray()
            )
            .WithCloseBraceToken(Token(SyntaxKind.CloseBraceToken));
        var generatedNamespace = GetFileScopedNamespaceDeclaration(AttributeNamespace, generatedType, true);
        var compilationUnit = CompilationUnit()
            .AddMembers(generatedNamespace)
            .NormalizeWhitespace();
        return SyntaxTree(compilationUnit, encoding: Encoding.UTF8).GetText().ToString();
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var generatorAttributes = context.SyntaxProvider.ForAttributeWithMetadataName(
            AttributeFullName,
            (_, _) => true,
            (syntaxContext, _) => syntaxContext
        ).Collect();

        context.RegisterSourceOutput(generatorAttributes, (spc, gas) =>
        {
            if (gas.Length > 0)
                if (TypeWithAttribute(gas, out var name) is { } source)
                    spc.AddSource(name, source);
        });
    }
}
