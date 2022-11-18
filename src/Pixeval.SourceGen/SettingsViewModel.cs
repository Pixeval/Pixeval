﻿#region Copyright (c) Pixeval/Pixeval.SourceGen

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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using static Pixeval.SourceGen.Utils;

namespace Pixeval.SourceGen;

internal static partial class TypeWithAttributeDelegates
{
    public static string? SettingsViewModel(INamedTypeSymbol typeSymbol, ImmutableArray<AttributeData> attributeList)
    {
        {
            var attribute = attributeList[0];
            // Generic Attribute code
            // if (attribute.ConstructorArguments[0].Value is not INamedTypeSymbol type)
            //     return null;
            // var type = attribute.AttributeClass.TypeArguments[0];
            // if (attribute.ConstructorArguments[0].Value is not string settingName)
            //     return null;
            if (attribute.ConstructorArguments[0].Value is not INamedTypeSymbol type)
                return null;
            if (attribute.ConstructorArguments[1].Value is not string settingName)
                return null;

            var name = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            var namespaces = new HashSet<string> { typeSymbol.ContainingNamespace.ToDisplayString() };
            var usedTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            const string nullable = "#nullable enable\n";
            var classBegin = @$"namespace {typeSymbol.ContainingNamespace.ToDisplayString()};

partial class {name}
{{";
            var propertySentences = new List<string>();
            const string classEnd = @"}";

            foreach (var property in type.GetMembers().Where(property =>
                             property is { Kind: SymbolKind.Property } and not { Name: "EqualityContract" }
                             && !property.GetAttributes().Any(propertyAttribute =>
                                 propertyAttribute.AttributeClass!.Name is "SettingsViewModelExclusionAttribute"))
                         .Cast<IPropertySymbol>())
            {
                namespaces.UseNamespace(usedTypes, typeSymbol, property.Type);
                foreach (var propertyAttribute in property.GetAttributes())
                {
                    namespaces.UseNamespace(usedTypes, typeSymbol, propertyAttribute.AttributeClass!);
                    foreach (var attrConstructorArgument in propertyAttribute.ConstructorArguments.Where(arg =>
                                 arg.Value is INamedTypeSymbol))
                    {
                        namespaces.UseNamespace(usedTypes, typeSymbol, (ITypeSymbol)attrConstructorArgument.Value!);
                    }
                }

                propertySentences.Add(Spacing(1) + Regex.Replace(
                    property.DeclaringSyntaxReferences[0].GetSyntax().ToString(), @"{[\s\S]+}",
                    $@"
    {{
        get => {settingName}.{property.Name};
        set => SetProperty({settingName}.{property.Name}, value, {settingName}, (setting, value) => setting.{property.Name} = value);
    }}"));
            }


            var namespaceNames = namespaces.Skip(1).Aggregate("", (current, ns) => current + $"using {ns};\n");
            var allPropertySentences = propertySentences.Aggregate("\n", (current, ps) => current + $"{ps}\n\n");
            allPropertySentences = allPropertySentences.Substring(0, allPropertySentences.Length - 1);
            var compilationUnit = nullable + namespaceNames + classBegin + allPropertySentences + classEnd;
            return compilationUnit;
        }
    }
}
