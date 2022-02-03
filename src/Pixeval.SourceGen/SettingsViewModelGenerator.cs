#region Copyright (c) Pixeval/Pixeval.SourceGen
// GPL v3 License
// 
// Pixeval/Pixeval.SourceGen
// Copyright (c) 2021 Pixeval.SourceGen/LoadSaveConfigurationGenerator.cs
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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pixeval.SourceGen.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static Pixeval.SourceGen.Utilities.GeneratorHelpers;

namespace Pixeval.SourceGen;

[Generator]
internal class SettingsViewModelGenerator : GetAttributeGenerator
{
    protected override string AttributePathGetter() => "Pixeval.Attributes.SettingsViewModelAttribute";

    protected override void ExecuteForEach(GeneratorExecutionContext context, INamedTypeSymbol attributeType,
        TypeDeclarationSyntax typeDeclaration, INamedTypeSymbol specificType)
    {
        foreach (var typeAttribute in specificType.GetAttributes().Where(attribute =>
                     SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, attributeType)))
        {
            if (typeAttribute.ConstructorArguments[0].Value is not INamedTypeSymbol type)
                continue;
            if (typeAttribute.ConstructorArguments[1].Value is not string settingName)
                continue;

            var name = specificType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            var namespaces = new HashSet<string> { specificType.ContainingNamespace.ToDisplayString() };
            var usedTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            const string nullable = "#nullable enable\n";
            var classBegin = @$"namespace {specificType.ContainingNamespace.ToDisplayString()};

partial class {name}
{{";
            var propertySentences = new List<string>();
            const string classEnd = @"}";

            foreach (var property in type.GetMembers().Where(property =>
                             property is { Kind: SymbolKind.Property } and not { Name: "EqualityContract" }
                             && !property.GetAttributes().Any(propertyAttribute => propertyAttribute.AttributeClass!.Name is "SettingsViewModelExclusionAttribute"))
                         .Cast<IPropertySymbol>())
            {
                namespaces.UseNamespace(usedTypes, property.Type);
                foreach (var propertyAttribute in property.GetAttributes())
                {
                    namespaces.UseNamespace(usedTypes, propertyAttribute.AttributeClass!);
                    foreach (var attrConstructorArgument in propertyAttribute.ConstructorArguments.Where(arg => arg.Value is INamedTypeSymbol))
                    {
                        namespaces.UseNamespace(usedTypes, (ITypeSymbol) attrConstructorArgument.Value!);
                    }
                }

                propertySentences.Add(Spacing(1) + Regex.Replace(property.DeclaringSyntaxReferences[0].GetSyntax().ToString(), @"{[\s\S]+}",
                    $@"
    {{
        get => {settingName}.{property.Name};
        set => SetProperty({settingName}.{property.Name}, value, {settingName}, (setting, value) => setting.{property.Name} = value);
    }}"));
            }


            var namespaceNames = namespaces.Skip(1).Aggregate("", (current, ns) => current + $"using {ns};\n");
            var allPropertySentences = propertySentences.Aggregate("\n", (current, ps) => current + $"{ps}\n\n");
            allPropertySentences = allPropertySentences.Substring(0, allPropertySentences.Length - 1);
            var fileName = specificType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat
                .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)) + ".g.cs";
            var compilationUnit = nullable + namespaceNames + classBegin + allPropertySentences + classEnd;
            context.AddSource(fileName, compilationUnit);
            break;
        }
    }
}