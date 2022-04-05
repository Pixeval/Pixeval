using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using static Pixeval.SourceGen.Utils;

namespace Pixeval.SourceGen;

/// <summary>
/// References:
/// <br/> <a href="https://andrewlock.net/series/creating-a-source-generator/"/>
/// <br/> <a href="https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md/"/>
/// </summary>
[Generator]
public class TypeWithAttributeGenerator : IIncrementalGenerator
{
    /// <summary>
    /// 对拥有某attribute的type生成代码
    /// </summary>
    /// <param name="typeDeclarationSyntax"></param>
    /// <param name="typeSymbol"></param>
    /// <param name="attributeEqualityComparer">判断是否是指定的attribute的比较器</param>
    /// <returns>生成的代码</returns>
    private delegate string? TypeWithAttribute(TypeDeclarationSyntax typeDeclarationSyntax, INamedTypeSymbol typeSymbol, Func<AttributeData, bool> attributeEqualityComparer);

    /// <summary>
    /// 需要生成的Attribute
    /// </summary>
    private static readonly Dictionary<string, TypeWithAttribute> Attributes = new()
    {
        { "Pixeval.Attributes.GenerateConstructorAttribute", TypeWithAttributeDelegates.GenerateConstructor },
        { "Pixeval.Attributes.LoadSaveConfigurationAttribute", TypeWithAttributeDelegates.LoadSaveConfiguration },
        { "Pixeval.Attributes.DependencyPropertyAttribute", TypeWithAttributeDelegates.DependencyProperty },
        { "Pixeval.Attributes.SettingsViewModelAttribute", TypeWithAttributeDelegates.SettingsViewModel }
    };

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<TypeDeclarationSyntax> typeDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (s, _) => IsSyntaxTargetForGeneration(s),
                static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null)!;

        IncrementalValueProvider<(Compilation Compilation, ImmutableArray<TypeDeclarationSyntax> TypeDeclarationSyntaxes)> compilationAndTypes =
            context.CompilationProvider.Combine(typeDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndTypes, static (spc, source) =>
            Execute(source.Compilation, source.TypeDeclarationSyntaxes, spc));
    }

    /// <summary>
    /// 初次快速筛选（对拥有Attribute的class和record）
    /// </summary>
    private static bool IsSyntaxTargetForGeneration(SyntaxNode node) =>
        node is TypeDeclarationSyntax { AttributeLists.Count: > 0 }
            and (ClassDeclarationSyntax or RecordDeclarationSyntax);

    /// <summary>
    /// 获取TypeDeclarationSyntax
    /// </summary>
    private static TypeDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var typeDeclarationSyntax = (TypeDeclarationSyntax)context.Node;
        // 不用Linq，用foreach保证速度
        foreach (var attributeListSyntax in typeDeclarationSyntax.AttributeLists)
            foreach (var attributeSyntax in attributeListSyntax.Attributes)
            {
                if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                    continue;

                if (Attributes.ContainsKey(attributeSymbol.ContainingType.ToDisplayString()))
                    return typeDeclarationSyntax;
            }

        return null;
    }

    /// <summary>
    /// 对获取的每个type和Attribute进行生成
    /// </summary>
    private static void Execute(Compilation compilation, ImmutableArray<TypeDeclarationSyntax> types, SourceProductionContext context)
    {
        ObjectSymbol ??= compilation.GetSpecialType(SpecialType.System_Object);

        if (types.IsDefaultOrEmpty)
            return;

        // 遍历每个class
        foreach (var typeDeclarationSyntax in types)
        {
            var semanticModel = compilation.GetSemanticModel(typeDeclarationSyntax.SyntaxTree);
            if (semanticModel.GetDeclaredSymbol(typeDeclarationSyntax) is not INamedTypeSymbol typeSymbol)
                continue;

            // 同种attribute只判断一遍
            var usedAttributes = new HashSet<string>();

            // 遍历class上每个Attribute
            //[...,...]
            //[...,...]
            foreach (var attributeListSyntax in typeDeclarationSyntax.AttributeLists)
                //[...,...]
                foreach (var attributeSyntax in attributeListSyntax.Attributes)
                {
                    if (semanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeCtorSymbol)
                        continue;
                    var attributeName = attributeCtorSymbol.ContainingType.ToDisplayString();
                    if (!Attributes.ContainsKey(attributeName))
                        continue;
                    if(usedAttributes.Contains(attributeName))
                        continue;
                    usedAttributes.Add(attributeName);

                    if (Attributes[attributeName](typeDeclarationSyntax, typeSymbol, attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, attributeCtorSymbol.ContainingType)) is { } source)
                        context.AddSource(
                            // 不能重名
                            $"{typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted))}_{attributeName}.g.cs",
                            source);
                }
        }
    }
}