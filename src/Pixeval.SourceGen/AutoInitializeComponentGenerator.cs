using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Pixeval.SourceGen;

internal class AutoInitializeComponentGenerator:IIncrementalGenerator
{
    private const string RootNamespace = "Pixeval";
    private const string AutoInitializeComponentAttributeName = "AutoInitializeComponentAttribute";
    private const string AutoInitializeComponentAttributeText = $$"""
        using System;

        #nullable enable

        namespace {{RootNamespace}};

        [AttributeUsage(AttributeTargets.Class)]
        public sealed class {{AutoInitializeComponentAttributeName}} : global::System.Attribute
        { 
            public {{AutoInitializeComponentAttributeName}}()
            {
            }
        }
        """;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(output =>
        {
            output.AddSource($"{AutoInitializeComponentAttributeName}.g.cs", AutoInitializeComponentAttributeText);
        });

        var attributes = context.SyntaxProvider.ForAttributeWithMetadataName(
            $"{RootNamespace}.{AutoInitializeComponentAttributeName}",
            (_, _) => true,
            (syntaxContext, _) => syntaxContext);


        context.RegisterSourceOutput(attributes, Execute);
    }

    public void Execute(SourceProductionContext spc, GeneratorAttributeSyntaxContext asc)
    {
        
    }
}