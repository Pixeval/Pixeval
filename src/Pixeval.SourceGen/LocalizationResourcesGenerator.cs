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

using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.CodeAnalysis;

namespace Pixeval.SourceGen;

// see https://platform.uno/blog/using-msbuild-items-and-properties-in-c-9-source-generators/
[Generator]
public class LocalizationResourcesGenerator : ISourceGenerator
{
    private const string SourceItemGroupMetadata = "build_metadata.AdditionalFiles.SourceItemGroup";

    public void Initialize(GeneratorInitializationContext context)
    {
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.AdditionalFiles.Where(
                f => context.AnalyzerConfigOptions.GetOptions(f).TryGetValue(SourceItemGroupMetadata, out var sourceItemGroup)
                     && sourceItemGroup == "PRIResource"
                     && f.Path.EndsWith(".resw")).Distinct(new AdditionalTextEqualityComparer()).ToImmutableArray() is var resources
            && resources.Any())
        {
            const char open = '{';
            const char close = '}';
            var stringBuilder = new IndentedStringBuilder(4);
            using (var namespaceBuilder = stringBuilder.Block("namespace Pixeval", open, close))
            {
                foreach (var additionalText in resources)
                {
                    var className = $"{Path.GetFileNameWithoutExtension(additionalText.Path)}Resources";
                    using var classBuilder = namespaceBuilder.Block($"public class {className}", open, close);
                    classBuilder.AppendLine(@$"public static readonly Microsoft.Windows.ApplicationModel.Resources.ResourceLoader ResourceLoader = new(Microsoft.Windows.ApplicationModel.Resources.ResourceLoader.GetDefaultResourceFilePath(), ""{Path.GetFileNameWithoutExtension(additionalText.Path)}"");");
                    var doc = new XmlDocument();
                    doc.Load(additionalText.Path);
                    if (doc.SelectNodes("//data") is { } nodes)
                    {
                        var elements = nodes.Cast<XmlElement>();
                        foreach (var node in elements)
                        {
                            var name = node.GetAttribute("name");
                            if (name.Contains("["))
                            {
                                continue;
                            }

                            classBuilder.AppendLine(@$"public static readonly string {Regex.Replace(name, "\\.|\\:|\\[|\\]", string.Empty)} = ResourceLoader.GetString(""{name.Replace('.', '/')}"");");
                        }
                    }
                }
            }

            context.AddSource("LocalizationResources.g", stringBuilder.ToString());
        }
    }
}