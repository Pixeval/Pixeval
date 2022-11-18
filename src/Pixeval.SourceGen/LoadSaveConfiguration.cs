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
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using static Pixeval.SourceGen.Utils;

namespace Pixeval.SourceGen;
internal static partial class TypeWithAttributeDelegates
{
    public static string? LoadSaveConfiguration(INamedTypeSymbol typeSymbol, ImmutableArray<AttributeData> attributeList)
    {
        var attribute = attributeList[0];

        // Generic Attribute code
        // if (attribute.AttributeClass is not ({ IsGenericType: true } and { TypeArguments.IsDefaultOrEmpty: false }))
        //     return null;
        // var type = attribute.AttributeClass.TypeArguments[0];
        // if (attribute.ConstructorArguments[0].Value is not string containerName)
        //     return null;
        if (attribute.ConstructorArguments[0].Value is not INamedTypeSymbol type)
            return null;
        if (attribute.ConstructorArguments[1].Value is not string containerName)
            return null;

        string? staticClassName = null;
        string? methodName = null;

        if (attribute.NamedArguments[0].Key is "CastMethod" && attribute.NamedArguments[0].Value.Value is string castMethodFullName)
        {
            var dotPosition = castMethodFullName.LastIndexOf('.');
            if (dotPosition is -1)
                throw new InvalidDataException("\"CastMethod\" must contain the full name.");
            staticClassName = "static " + castMethodFullName.Substring(0, dotPosition);
            methodName = castMethodFullName.Substring(dotPosition + 1);
        }

        var name = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        var namespaces = new HashSet<string>();
        if (staticClassName is not null)
            namespaces.Add(staticClassName); //methodName方法所用namespace
        var usedTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        /*-----Body Begin-----*/
        var stringBuilder = new StringBuilder().AppendLine("#nullable enable\n");
        /*-----Splitter-----*/
        var classBegin = @$"
namespace {typeSymbol.ContainingNamespace.ToDisplayString()};

partial class {name}
{{";
        var loadConfigurationBegin = $@"    public static {type.Name}? LoadConfiguration()
    {{
        try
        {{
            return new {type.Name}(";
        /*-----Splitter-----*/
        var loadConfigurationContent = new StringBuilder();
        /*-----Splitter-----*/
        var loadConfigurationEndAndSaveConfigurationBegin = $@"           );
        }}
        catch
        {{
            return null;
        }}
    }}

    public static void SaveConfiguration({type.Name}? configuration)
    {{
        if (configuration is {{ }} appConfiguration)
        {{";
        /*-----Splitter-----*/
        var saveConfigurationContent = new StringBuilder();
        /*-----Splitter-----*/
        const string saveConfigurationEndAndClassEnd = $@"      }}
    }}
}}";
        /*-----Body End-----*/
        foreach (var member in type.GetMembers().Where(member =>
                         member is { Kind: SymbolKind.Property } and not { Name: "EqualityContract" })
                     .Cast<IPropertySymbol>())
        {
            loadConfigurationContent.AppendLine(LoadRecord(member.Name, member.Type.Name, type.Name, containerName,
                methodName));
            saveConfigurationContent.AppendLine(SaveRecord(member.Name, member.Type, type.Name, containerName,
                methodName));
            namespaces.UseNamespace(usedTypes, typeSymbol, member.Type);
        }

        // 去除" \r\n"
        loadConfigurationContent = loadConfigurationContent.Remove(loadConfigurationContent.Length - 3, 3);

        foreach (var s in namespaces)
            _ = stringBuilder.AppendLine($"using {s};");
        stringBuilder.AppendLine(classBegin)
            .AppendLine(loadConfigurationBegin)
            .AppendLine(loadConfigurationContent.ToString())
            .AppendLine(loadConfigurationEndAndSaveConfigurationBegin)
            // saveConfigurationContent 后已有空行
            .Append(saveConfigurationContent)
            .AppendLine(saveConfigurationEndAndClassEnd);
        return stringBuilder.ToString();
    }


    private static string LoadRecord(string name, string type, string typeName, string containerName, string? methodName)
    {
        return methodName is null
            ? $"{Spacing(4)}({type}){containerName}.Values[nameof({typeName}.{name})],"
            : $"{Spacing(4)}{containerName}.Values[nameof({typeName}.{name})].{methodName}<{type}>(),";
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
                { Name: nameof(String) } => $"{Spacing(3)}{body} ?? string.Empty;",
                { TypeKind: TypeKind.Enum } => methodName is null
                    ? $"{Spacing(3)}(int)({body});"
                    : $"{Spacing(3)}{body}.{methodName}<int>();",
                _ => throw new InvalidCastException("Only primitive and Enum types are supported.")
            };
        }
        return $"{Spacing(3)}{body};";
    }
}
