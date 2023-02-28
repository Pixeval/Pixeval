#region Copyright (c) Pixeval/Pixeval.SourceGen
// GPL v3 License
// 
// Pixeval/Pixeval.SourceGen
// Copyright (c) 2023 Pixeval.SourceGen/TypeWithAttributeDelegates.cs
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
using System.Text;
using System;
using System.Linq;

namespace Pixeval.SourceGen;

internal static partial class TypeWithAttributeDelegates
{
    public static string? AppContext(INamedTypeSymbol typeSymbol, ImmutableArray<AttributeData> attributeList)
    {
        var attribute = attributeList[0];

        if (attribute.AttributeClass is not ({ IsGenericType: true } and { TypeArguments.IsDefaultOrEmpty: false }))
            return null;
        var type = attribute.AttributeClass.TypeArguments[0];

        var staticClassName = "static WinUI3Utilities.Misc";
        var methodName = "ToNotNull";

        string? configKey = null;

        foreach (var namedArgument in attribute.NamedArguments)
            if (namedArgument.Value.Value is { } value)
                switch (namedArgument.Key)
                {
                    case "ConfigKey":
                        configKey = (string)value;
                        break;
                    case "CastMethod":
                        var castMethodFullName = (string)value;
                        var dotPosition = castMethodFullName.LastIndexOf('.');
                        if (dotPosition is -1)
                            throw new InvalidDataException("\"CastMethod\" must contain the full name.");
                        staticClassName = "static " + castMethodFullName[..dotPosition];
                        methodName = castMethodFullName[(dotPosition + 1)..];
                        break;
                }

        configKey ??= "Configuration";

        var name = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        var namespaces = new HashSet<string> { "Windows.Storage" };
        // methodName方法所用namespace
        _ = namespaces.Add(staticClassName);
        var usedTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        /*-----Body Begin-----*/
        var classBegin = @$"
namespace {typeSymbol.ContainingNamespace.ToDisplayString()};

partial class {name}
{{
    private static ApplicationDataContainer _configurationContainer = null!;

    private const string ConfigurationContainerKey = ""{configKey}"";

    public static void InitializeConfigurationContainer()
    {{
        if (!ApplicationData.Current.RoamingSettings.Containers.ContainsKey(ConfigurationContainerKey))
            _ = ApplicationData.Current.RoamingSettings.CreateContainer(ConfigurationContainerKey, ApplicationDataCreateDisposition.Always);

        _configurationContainer = ApplicationData.Current.RoamingSettings.Containers[ConfigurationContainerKey];
    }}

    public static {type.Name}? LoadConfiguration()
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
        const string saveConfigurationEndAndClassEnd = @"        }
    }
}";
        /*-----Body End-----*/
        foreach (var member in type.GetMembers().Where(member => member is { Kind: SymbolKind.Property } and not { Name: "EqualityContract" }).Cast<IPropertySymbol>())
        {
            _ = loadConfigurationContent.AppendLine(LoadRecord(member.Name, member.Type.Name, type.Name, methodName));
            _ = saveConfigurationContent.AppendLine(SaveRecord(member.Name, member.Type, type.Name, methodName));
            namespaces.UseNamespace(usedTypes, typeSymbol, member.Type);
        }

        // 去除" \r\n"
        _ = loadConfigurationContent.Remove(loadConfigurationContent.Length - 3, 3);

        return namespaces.GenerateFileHeader()
            .AppendLine(classBegin)
            .AppendLine(loadConfigurationContent.ToString())
            .AppendLine(loadConfigurationEndAndSaveConfigurationBegin)
            // saveConfigurationContent 后已有空行
            .Append(saveConfigurationContent)
            .AppendLine(saveConfigurationEndAndClassEnd)
            .ToString();
    }

    private static string LoadRecord(string name, string type, string typeName, string? methodName) => methodName is null
            ? $"{SyntaxHelper.Spacing(4)}({type})_configurationContainer.Values[nameof({typeName}.{name})],"
            : $"{SyntaxHelper.Spacing(4)}_configurationContainer.Values[nameof({typeName}.{name})].{methodName}<{type}>(),";

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

    private static string SaveRecord(string name, ITypeSymbol type, string typeName, string? methodName)
    {
        var body = $"_configurationContainer.Values[nameof({typeName}.{name})] = appConfiguration.{name}";
        return !PrimitiveTypes.Contains(type.Name)
            ? type switch
            {
                { Name: nameof(String) } => $"{SyntaxHelper.Spacing(3)}{body} ?? string.Empty;",
                { TypeKind: TypeKind.Enum } => methodName is null
                    ? $"{SyntaxHelper.Spacing(3)}(int)({body});"
                    : $"{SyntaxHelper.Spacing(3)}{body}.{methodName}<int>();",
                _ => throw new InvalidCastException("Only primitive and Enum types are supported.")
            }
            : $"{SyntaxHelper.Spacing(3)}{body};";
    }
}