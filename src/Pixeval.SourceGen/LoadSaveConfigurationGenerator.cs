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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Pixeval.SourceGen;

[Generator]
internal class LoadSaveConfigurationGenerator : GetAttributeGenerator
{
    protected override string AttributePathGetter() => "Pixeval.Misc.LoadSaveConfigurationAttribute";

    protected override void ExecuteForEach(GeneratorExecutionContext context, INamedTypeSymbol attributeType,
        TypeDeclarationSyntax typeDeclaration, INamedTypeSymbol specificType)
    {
        foreach (var attribute in specificType.GetAttributes().Where(attribute =>
                     SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, attributeType)))
        {
            if (attribute.ConstructorArguments[0].Value is not INamedTypeSymbol type)
                continue;

            var name = specificType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            var namespaces = new HashSet<string> { "Pixeval.Utilities" };
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
    public static void SaveConfiguration({type.Name}? setting)
    {{
        if (setting is {{ }} appSetting)
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
                loadConfigurationContent += LoadRecord(member.Name, member.Type.Name, type.Name);
                saveConfigurationContent += SaveRecord(member.Name, member.Type, type.Name);
                namespaces.UseNamespace(usedTypes, specificType, member.Type);
            }

            loadConfigurationContent =
                loadConfigurationContent.Substring(0, loadConfigurationContent.Length - 2) + "\n";

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

    private static string LoadRecord(string name, string type, string typeName)
    {
        return $"                ConfigurationContainer.Values[nameof({typeName}.{name})].CastOrThrow<{type}>(),\n";
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
        nameof(Guid)
    };

    private static string SaveRecord(string name, ITypeSymbol type, string typeName)
    {
        var record = $"            ConfigurationContainer.Values[nameof({typeName}.{name})] = appSetting.{name}";
        if (!PrimitiveTypes.Contains(type.Name))
            if (type.Name is "String")
                record += " ?? string.Empty";
            else if (type.TypeKind is TypeKind.Enum)
                record += ".CastOrThrow<int>()";
            else throw new InvalidCastException("Only primitive and Enum types are supported.");
        return record + ";\n";
    }
}