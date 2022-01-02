#region Copyright (c) Pixeval/Pixeval.SourceGen

// GPL v3 License
// 
// Pixeval/Pixeval.SourceGen
// Copyright (c) 2021 Pixeval.SourceGen/DependencyPropertyGenerator.cs
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pixeval.SourceGen.Utilities;

namespace Pixeval.SourceGen;

[Generator]
internal class LoadSaveConfigurationGenerator : GetAttributeGenerator
{
    protected override string AttributePathGetter() => "Pixeval.Attributes.LoadSaveConfigurationAttribute";

    protected override void ExecuteForEach(GeneratorExecutionContext context, INamedTypeSymbol attributeType,
        TypeDeclarationSyntax typeDeclaration, INamedTypeSymbol specificType)
    {
        foreach (var attribute in specificType.GetAttributes().Where(attribute =>
                     SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, attributeType)))
        {
            if (attribute.ConstructorArguments[0].Value is not INamedTypeSymbol type)
                continue;
            if (attribute.ConstructorArguments[1].Value is not string containerName)
                continue;

            string? staticClassName = null;
            string? methodName = null;

            foreach (var namedArgument in attribute.NamedArguments)
            {
                if (namedArgument.Value.Value is { } value)
                {
                    switch (namedArgument.Key)
                    {
                        case "CastMethod":
                            var temp = (string)value;
                            var tempIndex = temp.LastIndexOf('.');
                            if (tempIndex is -1)
                                throw new InvalidDataException("CastMethod must contain the full name.");
                            staticClassName = "static " + temp.Substring(0, tempIndex);
                            methodName = temp.Substring(tempIndex + 1);
                            break;
                    }
                }
            }

            var name = specificType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            var namespaces = new HashSet<string>();
            if (staticClassName is not null)
                namespaces.Add(staticClassName);//methodName方法所用namespace
            var usedTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            const string nullable = "#nullable enable\n";
            var classBegin = @$"namespace {specificType.ContainingNamespace.ToDisplayString()};

partial class {name}
{{";
            var loadConfigurationBegin = $@"
    public static {type.Name}? LoadConfiguration()
    {{
        try
        {{
            return new {type.Name}(";
            var loadConfigurationContent = "\n";
            const string loadConfigurationEnd = $@"           );
        }}
        catch
        {{
            return null;
        }}
    }}";
            var saveConfigurationBegin = $@"
    public static void SaveConfiguration({type.Name}? configuration)
    {{
        if (configuration is {{ }} appConfiguration)
        {{";
            var saveConfigurationContent = "\n";
            const string saveConfigurationEnd = $@"      }}
    }}";
            const string classEnd = $@"
}}";
            foreach (var member in type.GetMembers().Where(member =>
                             member is { Kind: SymbolKind.Property } and not { Name: "EqualityContract" })
                         .Cast<IPropertySymbol>())
            {
                loadConfigurationContent += LoadRecord(member.Name, member.Type.Name, type.Name, containerName, methodName);
                saveConfigurationContent += SaveRecord(member.Name, member.Type, type.Name, containerName, methodName);
                namespaces.UseNamespace(usedTypes, specificType, member.Type);
            }

            loadConfigurationContent = loadConfigurationContent.Substring(0, loadConfigurationContent.Length - 2) + "\n";

            var namespaceNames = namespaces.Aggregate("", (current, ns) => current + $"using {ns};\n");
            var fileName = specificType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat
                .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)) + ".g.cs";
            var loadConfiguration =
                nullable + namespaceNames
                         + classBegin
                         + loadConfigurationBegin + loadConfigurationContent + loadConfigurationEnd
                         + saveConfigurationBegin + saveConfigurationContent + saveConfigurationEnd
                         + classEnd;
            context.AddSource(fileName, loadConfiguration);
            break;
        }
    }

    private static string Spacing(int n)
    {
        var temp = "";
        for (var i = 0; i < n; i++)
            temp += "    ";
        return temp;
    }

    private static string LoadRecord(string name, string type, string typeName, string containerName, string? methodName)
    {
        return methodName is null
            ? $"{Spacing(4)}({type}){containerName}.Values[nameof({typeName}.{name})],\n"
            : $"{Spacing(4)}{containerName}.Values[nameof({typeName}.{name})].{methodName}<{type}>(),\n";
    }

    private static readonly HashSet<string> PrimitiveTypes = new()
    {
        nameof(SByte),
        nameof(Byte),
        nameof(Int16),
        nameof(UInt16),
        nameof(Int32),
        nameof(UInt32),
        nameof(Int64),
        nameof(UInt64),
        nameof(Single),
        nameof(Double),
        nameof(Boolean),
        nameof(Char),
        nameof(DateTime),
        nameof(TimeSpan),
        nameof(Guid),
        nameof(DateTimeOffset)
    };

    private static string SaveRecord(string name, ITypeSymbol type, string typeName, string containerName, string? methodName)
    {
        var body = $"{containerName}.Values[nameof({typeName}.{name})] = appConfiguration.{name}";
        if (!PrimitiveTypes.Contains(type.Name))
        {
            return type switch
            {
                { Name: nameof(String) } => $"{Spacing(3)}{body} ?? string.Empty;\n",
                { TypeKind: TypeKind.Enum } => methodName is null
                    ? $"{Spacing(3)}(int)({body});\n"
                    : $"{Spacing(3)}{body}.{methodName}<int>();\n",
                _ => throw new InvalidCastException("Only primitive and Enum types are supported.")
            };
        }
        return $"{Spacing(3)}{body};\n";
    }
}