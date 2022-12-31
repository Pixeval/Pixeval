using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Pixeval.SourceGen;
using static Pixeval.SourceGen.Utils;

namespace Pixeval.SourceGen;

internal static partial class TypeWithAttributeDelegates
{
    public static string? LoadSaveConfiguration(INamedTypeSymbol typeSymbol, ImmutableArray<AttributeData> attributeList)
    {
        var attribute = attributeList[0];

        if (attribute.AttributeClass is not ({ IsGenericType: true } and { TypeArguments.IsDefaultOrEmpty: false }))
            return null;
        var type = attribute.AttributeClass.TypeArguments[0];
        if (attribute.ConstructorArguments[0].Value is not string containerName)
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
            _ = namespaces.Add(staticClassName); //methodName方法所用namespace
        var usedTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        /*-----Body Begin-----*/
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
            _ = loadConfigurationContent.AppendLine(LoadRecord(member.Name, member.Type.Name, type.Name, containerName, methodName));
            _ = saveConfigurationContent.AppendLine(SaveRecord(member.Name, member.Type, type.Name, containerName, methodName));
            namespaces.UseNamespace(usedTypes, typeSymbol, member.Type);
        }

        // 去除" \r\n"
        _ = loadConfigurationContent.Remove(loadConfigurationContent.Length - 3, 3);

        return namespaces.GenerateFileHeader()
            .AppendLine(classBegin)
            .AppendLine(loadConfigurationBegin)
            .AppendLine(loadConfigurationContent.ToString())
            .AppendLine(loadConfigurationEndAndSaveConfigurationBegin)
            // saveConfigurationContent 后已有空行
            .Append(saveConfigurationContent)
            .AppendLine(saveConfigurationEndAndClassEnd)
            .ToString();
    }

    private static string LoadRecord(string name, string type, string typeName, string containerName, string? methodName) => methodName is null
            ? $"{Spacing(4)}({type}){containerName}.Values[nameof({typeName}.{name})],"
            : $"{Spacing(4)}{containerName}.Values[nameof({typeName}.{name})].{methodName}<{type}>(),";

    private static readonly HashSet<string> _primitiveTypes = new()
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
        return !_primitiveTypes.Contains(type.Name)
            ? type switch
            {
                { Name: nameof(String) } => $"{Spacing(3)}{body} ?? string.Empty;",
                { TypeKind: TypeKind.Enum } => methodName is null
                    ? $"{Spacing(3)}(int)({body});"
                    : $"{Spacing(3)}{body}.{methodName}<int>();",
                _ => throw new InvalidCastException("Only primitive and Enum types are supported.")
            }
            : $"{Spacing(3)}{body};";
    }
}
