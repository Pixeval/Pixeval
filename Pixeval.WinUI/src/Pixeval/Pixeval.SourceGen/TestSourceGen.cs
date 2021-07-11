using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Pixeval.SourceGen
{
    [Generator]
    public class TestSourceGen : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // TODO source not generated in reference project
            context.AddSource("TestGen", @"
                namespace Pixeval.Test {
                    public static class HelloWorld {
                        public static string Key = ""Hello, World"";
                    }
                }
            ");
        }
    }
}