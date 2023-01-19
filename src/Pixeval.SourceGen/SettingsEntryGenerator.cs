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

using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Pixeval.SourceGen
{
    [Generator]
    public class SettingsEntryGenerator : IIncrementalGenerator
    {
        private const string SettingPOCOAttributeFqName = "Pixeval.Attributes.SettingPOCO";
        private const string SyntheticSettingAttributeFqName = "Pixeval.Attributes.SyntheticSetting";
        private const string SettingMetadataAttributeFqName = "Pixeval.Attributes.SettingMetadata";
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var classDeclaration = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) => node is (ClassDeclarationSyntax { AttributeLists.Count: > 0 } or RecordDeclarationSyntax { AttributeLists.Count: > 0 }),
                transform: static (ctx, _) =>
                {
                    var rds = (TypeDeclarationSyntax) ctx.Node;
                    return rds.TakeIf(r => r.HasAttribute(ctx.SemanticModel, SettingPOCOAttributeFqName));
                }).Where(s => s is not null).Collect();
            context.RegisterSourceOutput(classDeclaration, (ctx, array) =>
            {
                if (array is [{ } tds])
                {
                    const string classBegin = """
                    public partial record SettingEntry  
                    {
                    """;
                    const string classEnd = "}";

                    var members = tds.Members.OfType<PropertyDeclarationSyntax>().Where(pds => !pds.HasAttribute());
                    Debugger.Break();
                } 
                else if (array.Where(c => c is not null).ToArray() is [ { } _, ..] arr)
                {
                    foreach (var typeDeclarationSyntax in arr)
                    {
                        ctx.ReportDiagnostic(
                            Diagnostic.Create(new DiagnosticDescriptor(
                                "PSG0001",
                                "There should be only one [SettingPOCO] in an assembly",
                                "There should be only one [SettingPOCO] in an assembly",
                                "SourceGen",
                                DiagnosticSeverity.Error,
                                true), typeDeclarationSyntax!.GetLocation()));
                    }
                }
            });
        }
    }
}