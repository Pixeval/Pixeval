#region Copyright (c) Pixeval/Pixeval.SourceGen
// GPL v3 License
// 
// Pixeval/Pixeval.SourceGen
// Copyright (c) 2021 Pixeval.SourceGen/LocalizationResourcesGenerator.cs
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

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Pixeval.SourceGen;

[Generator]
public class LocalizationResourcesGenerator : IIncrementalGenerator
{
    private const string Namespace = "Pixeval";
    private const string LocalizedStringResourcesAttributeName = "LocalizedStringResourcesAttribute";
    private const string LocalizedStringResourcesAttributeText = $$"""
        using System;

        #nullable enable

        namespace {{Namespace}};

        [AttributeUsage(AttributeTargets.Class)]
        public sealed class {{LocalizedStringResourcesAttributeName}} : global::System.Attribute
        { 
            public {{LocalizedStringResourcesAttributeName}}(string? fileName = null)
            {
                FileName = fileName;
            }

            public string? FileName { get; set; }
        }
        """;

    private const string SourceItemGroupMetadata = "build_metadata.AdditionalFiles.SourceItemGroup";
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(output =>
        {
            output.AddSource("LocalizedStringResourcesAttribute.g.cs", LocalizedStringResourcesAttributeText);
        });


        var attributes = context.SyntaxProvider.ForAttributeWithMetadataName(
            $"{Namespace}.{LocalizedStringResourcesAttributeName}",
            (_, _) => true,
            (syntaxContext, _) => syntaxContext);

        var additionalTexts = context.AdditionalTextsProvider
            .Combine(context.AnalyzerConfigOptionsProvider)
            .Where(_ => _.Right.GetOptions(_.Left).TryGetValue(SourceItemGroupMetadata, out var sourceItemGroup) &&
                        sourceItemGroup == "PRIResource" && _.Left.Path.EndsWith(".resw"))
            .Select((tuple, _) => tuple.Left);

        context.RegisterSourceOutput(attributes.Combine(additionalTexts.Collect()), (spc, source) => Execute(spc, source.Left, source.Right));

    }

    public void Execute(SourceProductionContext spc, GeneratorAttributeSyntaxContext asc, ImmutableArray<AdditionalText> additionalTexts)
    {
        for (var i = 0; i < asc.Attributes.Length; i++)
        {
            var attribute = asc.Attributes[i];
            var fileName = Path.GetFileNameWithoutExtension(attribute.ConstructorArguments[0].Value as string) ??
                           asc.TargetSymbol.Name;
            var extension = Path.GetExtension(attribute.ConstructorArguments[0].Value as string) ?? string.Empty;
            var additionalText = additionalTexts.SingleOrDefault(_ =>
                Path.GetFileNameWithoutExtension(_.Path) == fileName ||
                Path.GetFileName(_.Path) == $"{fileName}.{extension}");
            if (additionalText is null)
            {
                continue;
            }
            var doc = XDocument.Parse(additionalText.GetText()?.ToString()!);
            var names = new List<string>();
            if (doc.XPathSelectElements("//data") is { } elements)
            {
                foreach (var node in elements)
                {
                    var name = node.Attribute("name");
                    if (name.Value.Contains("["))
                    {
                        continue;
                    }

                    names.Add(name.Value);
                }
            }

            var source = $$"""
                namespace {{(asc.TargetSymbol.ContainingNamespace is { } @namespace ? @namespace.ToDisplayString() : "Pixeval")}};
    
                partial class {{asc.TargetSymbol.Name}}
                {   
                    public StringResources SR { get; } = new();

                    public class StringResources
                    {
                        private static readonly global::Microsoft.Windows.ApplicationModel.Resources.ResourceLoader s_resourceLoader = new(global::Microsoft.Windows.ApplicationModel.Resources.ResourceLoader.GetDefaultResourceFilePath(),"{{Path.GetFileNameWithoutExtension(additionalText.Path)}}");

                        {{string.Join("\r\n        ", names.Select(_ => @$"public readonly string {Regex.Replace(_, "\\.|\\:|\\[|\\]", string.Empty)} = s_resourceLoader.GetString(""{_.Replace('.', '/')}"");"))}} 
                    }
                }
            
                """;
            spc.AddSource($"{asc.TargetSymbol.Name}{i}.SR.g", source);
        }
    }


}