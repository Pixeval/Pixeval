using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using static Pixeval.SourceGen.SyntaxHelper;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Pixeval.SourceGen;

[Generator]
public class FactoryGenerator : IIncrementalGenerator
{
    private const string AttributeName = "FactoryAttribute";

    private const string AttributeNamespace = nameof(Pixeval) + ".CoreApi";

    private const string AttributeFullName = AttributeNamespace + "." + AttributeName;

    internal string TypeWithAttribute(INamedTypeSymbol typeSymbol, ImmutableArray<AttributeData> attributeList)
    {
        var name = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        var typeSyntax = typeSymbol.GetTypeSyntax(false);
        const string createDefault = "CreateDefault";
        var list = typeSymbol.GetProperties(attributeList[0].AttributeClass!)
            .Where(symbol => !symbol.IsReadOnly && !symbol.IsStatic)
            .Select(symbol =>
            {
                var syntax = (PropertyDeclarationSyntax)symbol.DeclaringSyntaxReferences[0].GetSyntax();
                return (ExpressionSyntax)AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName(symbol.Name),
                    syntax.Initializer is { } init
                        ? init.Value
                        : symbol.Type.NullableAnnotation is NullableAnnotation.NotAnnotated && symbol.Type.GetAttributes().Any(i => i.AttributeClass?.MetadataName == AttributeName)
                            ? InvocationExpression(symbol.Type.GetStaticMemberAccessExpression(createDefault))
                            : DefaultExpression(symbol.Type.GetTypeSyntax(false)));
            });

        var method = MethodDeclaration(typeSyntax, createDefault)
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
            .WithExpressionBody(ArrowExpressionClause(
                ObjectCreationExpression(typeSyntax, null, InitializerExpression(SyntaxKind.ObjectInitializerExpression, SeparatedList(list)))))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

        // 如果typeSymbol的任意一个基类有CreateDefault方法，则加上new关键字
        var baseType = typeSymbol.BaseType;
        while (baseType is not null)
        {
            if (baseType.HasAttribute(AttributeFullName))
            {
                method = method.AddModifiers(Token(SyntaxKind.NewKeyword));
                break;
            }
            baseType = baseType.BaseType;
        }

        var generatedType = GetDeclaration(name, typeSymbol, method)
            .WithBaseList(BaseList(SeparatedList([(BaseTypeSyntax)SimpleBaseType(ParseTypeName($"{AttributeNamespace}.IFactory<{name}>"))])));
        var generatedNamespace = GetFileScopedNamespaceDeclaration(typeSymbol, generatedType, true);
        var compilationUnit = CompilationUnit()
            .AddMembers(generatedNamespace)
            .NormalizeWhitespace();
        return SyntaxTree(compilationUnit, encoding: Encoding.UTF8).GetText().ToString();
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var generatorAttributes = context.SyntaxProvider.ForAttributeWithMetadataName(
            AttributeFullName,
            (_, _) => true,
            (syntaxContext, _) => syntaxContext
        ).Combine(context.CompilationProvider);

        context.RegisterSourceOutput(generatorAttributes, (spc, tuple) =>
        {
            var (ga, compilation) = tuple;

            if (ga.TargetSymbol is not INamedTypeSymbol symbol)
                return;

            if (TypeWithAttribute(symbol, ga.Attributes) is { } source)
                spc.AddSource(
                    // 不能重名
                    $"{symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted))}_{AttributeFullName}.g.cs",
                    source);
        });
    }
}
