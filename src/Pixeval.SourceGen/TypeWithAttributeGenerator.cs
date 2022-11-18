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

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Pixeval.SourceGen;

[Generator]
public class TypeWithAttributeGenerator : IIncrementalGenerator
{
    /// <summary>
    /// 对拥有某attribute的type生成代码
    /// </summary>
    /// <param name="typeSymbol"></param>
    /// <param name="attributeList">该类的某种Attribute</param>
    /// <returns>生成的代码</returns>
    private delegate string? TypeWithAttribute(INamedTypeSymbol typeSymbol, ImmutableArray<AttributeData> attributeList);

    /// <summary>
    /// 需要生成的Attribute
    /// </summary>
    private static readonly Dictionary<string, TypeWithAttribute> Attributes = new()
    {
        { "Pixeval.Attributes.GenerateConstructorAttribute", TypeWithAttributeDelegates.GenerateConstructor },
        // Generic Attribute code
        // { "Pixeval.Attributes.LoadSaveConfigurationAttribute`1", TypeWithAttributeDelegates.LoadSaveConfiguration },
        // { "Pixeval.Attributes.DependencyPropertyAttribute`1", TypeWithAttributeDelegates.DependencyProperty },
        // { "Pixeval.Attributes.SettingsViewModelAttribute`1", TypeWithAttributeDelegates.SettingsViewModel },
        { "Pixeval.Attributes.LoadSaveConfigurationAttribute", TypeWithAttributeDelegates.LoadSaveConfiguration },
        { "Pixeval.Attributes.DependencyPropertyAttribute", TypeWithAttributeDelegates.DependencyProperty },
        { "Pixeval.Attributes.SettingsViewModelAttribute", TypeWithAttributeDelegates.SettingsViewModel }
    };

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        foreach (var attribute in Attributes)
        {
            var generatorAttributes = context.SyntaxProvider.ForAttributeWithMetadataName(
                attribute.Key,
                (_, _) => true,
                (syntaxContext, _) => syntaxContext
            );
            context.RegisterSourceOutput(generatorAttributes, (spc, ga) =>
            {
                if (ga.TargetSymbol is not INamedTypeSymbol symbol)
                    return;
                if (attribute.Value(symbol, ga.Attributes) is { } source)
                    spc.AddSource(
                        // 不能重名
                        $"{symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted))}_{attribute.Key}.g.cs",
                        source);
            });
        }
    }
}
