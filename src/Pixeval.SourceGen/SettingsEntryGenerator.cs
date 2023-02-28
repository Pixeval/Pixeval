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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace Pixeval.SourceGen
{
    [Generator]
    public class SettingsEntryGenerator : IIncrementalGenerator
    {
        private const string SettingPOCOAttributeFqName = "Pixeval.Attributes.SettingPOCO";
        private const string SyntheticSettingAttributeFqName = "Pixeval.Attributes.SyntheticSetting";
        private const string SettingMetadataAttributeFqName = "Pixeval.Attributes.SettingMetadata";
        private const string SettingEntryFqName = "global::Pixeval.SettingEntry";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var classDeclaration = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 } or RecordDeclarationSyntax { AttributeLists.Count: > 0 },
                transform: static (ctx, _) =>
                {
                    var tds = (TypeDeclarationSyntax) ctx.Node;
                    (TypeDeclarationSyntax, IEnumerable<(PropertyDeclarationSyntax, AttributeSyntax?)>)? tuple = tds.HasAttribute(ctx.SemanticModel, SettingPOCOAttributeFqName)
                        ? (tds, tds.Members.OfType<PropertyDeclarationSyntax>().Where(pds => !pds.HasAttribute(ctx.SemanticModel, SyntheticSettingAttributeFqName) && pds.HasAttribute(ctx.SemanticModel, SettingMetadataAttributeFqName))
                            .Select(property => (property, property.GetAttribute(ctx.SemanticModel, SettingMetadataAttributeFqName)))
                            .Where(tuple => tuple.Item2 is not null))
                        : null;
                    return tuple;
                }).Where(s => s.HasValue).Select((n, _) => n!.Value).Collect();
            context.RegisterSourceOutput(classDeclaration, (ctx, array) =>
            {
                switch (array)
                {
                    case [var (_, attributeDeclarationSyntaxList)]:
                        const string classBegin = """
                            using Pixeval;

                            namespace Pixeval;

                            public partial record SettingEntry  
                            {   

                            """;

                        const string classEnd = """

                            }
                            """;
                        var whitespaceTrivia = SyntaxTriviaList.Create(SyntaxTrivia(WhitespaceTrivia, " "));
                        var entries = attributeDeclarationSyntaxList.Select(tuple =>
                        {
                            var (property, attribute) = tuple;
                            var ctor = ImplicitObjectCreationExpression(ArgumentList(
                                SeparatedList(attribute!.ArgumentList!.Arguments.Indexed().Select(t => t is (var arg, 0) 
                                    ? Argument(arg.Expression) 
                                    : Argument(t.Item1.Expression).WithLeadingTrivia(whitespaceTrivia)))), null);
                            return FieldDeclaration(
                                new SyntaxList<AttributeListSyntax>(),
                                new SyntaxTokenList(
                                    Token(PublicKeyword),
                                    Token(whitespaceTrivia, StaticKeyword, SyntaxTriviaList.Empty),
                                    Token(whitespaceTrivia, ReadOnlyKeyword, whitespaceTrivia)),
                                VariableDeclaration(
                                    ParseTypeName(SettingEntryFqName),
                                    SeparatedList(new[] { VariableDeclarator(property.Identifier, null, EqualsValueClause(Token(SyntaxTriviaList.Empty, EqualsToken, whitespaceTrivia), ctor))
                                        .WithLeadingTrivia(whitespaceTrivia) })));
                        });
                        var str = classBegin + string.Join("\n\n", entries.Select(entry => $"    {entry.GetText()}")) + classEnd;
                        ctx.AddSource("SettingEntry.g.cs", str);
                        break;
                    case [ { }, ..] arr:
                        foreach (var (typeDeclarationSyntax, _) in arr)
                        {
                            ctx.ReportDiagnostic(
                                Diagnostic.Create(new DiagnosticDescriptor(
                                    "PSG0001",
                                    "There should be only one [SettingPOCO] in an assembly",
                                    "There should be only one [SettingPOCO] in an assembly",
                                    "SourceGen",
                                    DiagnosticSeverity.Error,
                                    true), typeDeclarationSyntax.GetLocation()));
                        }

                        break;
                }
            });
        }
    }
}