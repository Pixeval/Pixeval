#region Copyright (c) Pixeval/Pixeval.SourceGen
// GPL v3 License
// 
// Pixeval/Pixeval.SourceGen
// Copyright (c) 2023 Pixeval.SourceGen/SettingsEntryGenerator.cs
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

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace Pixeval.SourceGen;

[Generator]
public class SettingsEntryGenerator : IIncrementalGenerator
{
    private const string SettingPocoAttributeFqName = "Pixeval.Attributes.SettingPocoAttribute";
    private const string SyntheticSettingAttributeFqName = "Pixeval.Attributes.SyntheticSettingAttribute";
    private const string SettingMetadataAttributeFqName = "Pixeval.Attributes.SettingMetadataAttribute";
    private const string SettingEntryFqName = $"global::{SettingEntryNamespace}.SettingEntry";
    private const string SettingEntryNamespace = "Pixeval.Misc";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classDeclaration = context.SyntaxProvider.ForAttributeWithMetadataName(
                SettingPocoAttributeFqName,
                (_, _) => true,
            (syntaxContext, _) => syntaxContext
        ).Combine(context.CompilationProvider);

        context.RegisterSourceOutput(classDeclaration, (spc, tuple) =>
        {
            var (ga, compilation) = tuple;

            if (compilation.Assembly.GetAttributes().Any(attrData => attrData.AttributeClass?.ToDisplayString() == SyntaxHelper.DisableSourceGeneratorAttribute))
                return;

            if (ga.TargetSymbol is not INamedTypeSymbol symbol)
                return;

            var properties = symbol.GetProperties(ga.Attributes[0].AttributeClass!)
                .Where(pds => !pds.HasAttribute(SyntheticSettingAttributeFqName)
                              && pds.HasAttribute(SettingMetadataAttributeFqName))
                .Select(property => (property, property.GetAttribute(SettingMetadataAttributeFqName)!))
                .ToImmutableArray();
            var whitespaceTrivia = SyntaxTriviaList.Create(SyntaxTrivia(WhitespaceTrivia, " "));
            var entries = properties.Select(t =>
            {
                var (property, attribute) = t;
                if (attribute.ApplicationSyntaxReference?.GetSyntax() is not AttributeSyntax { ArgumentList.Arguments: var arguments })
                    throw new();
                var ctor = ImplicitObjectCreationExpression(ArgumentList(
                    SeparatedList(arguments.Select(syntax => Argument(syntax.Expression).WithLeadingTrivia(whitespaceTrivia)))), null);
                return FieldDeclaration(
                    [],
                    new SyntaxTokenList(
                        Token(PublicKeyword),
                        Token(whitespaceTrivia, StaticKeyword, SyntaxTriviaList.Empty),
                        Token(whitespaceTrivia, ReadOnlyKeyword, whitespaceTrivia)
                    ),
                    VariableDeclaration(
                        ParseTypeName(SettingEntryFqName),
                        SeparatedList(new[] { VariableDeclarator(Identifier(property.Name), null, EqualsValueClause(Token(SyntaxTriviaList.Empty, EqualsToken, whitespaceTrivia), ctor))
                                    .WithLeadingTrivia(whitespaceTrivia) })));
            });
            var str =
                $$"""
                #nullable enable
                
                namespace {{SettingEntryNamespace}};

                public partial record SettingEntry
                {
                {{string.Join("\n\n", entries.Select(entry => $"    {entry.GetText()}"))}}
                }
                """;
            spc.AddSource("SettingEntry.g.cs", str);
        });

    }
}
