using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;

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
    private static readonly Dictionary<string, TypeWithAttribute> _attributes = new()
    {
        { "Pixeval.Attributes.GenerateConstructorAttribute", TypeWithAttributeDelegates.GenerateConstructor },
        { "Pixeval.Attributes.LoadSaveConfigurationAttribute`1", TypeWithAttributeDelegates.LoadSaveConfiguration },
        { "Pixeval.Attributes.DependencyPropertyAttribute`1", TypeWithAttributeDelegates.DependencyProperty },
        { "Pixeval.Attributes.SettingsViewModelAttribute`1", TypeWithAttributeDelegates.SettingsViewModel }
    };

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        foreach (var attribute in _attributes)
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
