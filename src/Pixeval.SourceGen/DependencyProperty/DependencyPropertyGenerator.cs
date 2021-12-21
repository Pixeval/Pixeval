using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Pixeval.SourceGen.DependencyProperty;

[Generator]
internal class DependencyPropertyGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new AttributeReceiver(AttributePath));
    }

    private const string AttributePath = "Pixeval.Misc.DependencyPropertyAttribute";

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.Compilation.GetTypeByMetadataName(AttributePath) is not { } attributeType)
            return;

        foreach (var classDeclaration in ((AttributeReceiver)context.SyntaxContextReceiver!).CandidateClasses)
        {
            var semanticModel = context.Compilation.GetSemanticModel(classDeclaration.SyntaxTree);

            if (semanticModel.GetDeclaredSymbol(classDeclaration) is not { } specificClass)
                continue;

            var members = new List<MemberDeclarationSyntax>();
            var namespaces = new HashSet<string> { "Microsoft.UI.Xaml" };
            var usedTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

            foreach (var attribute in specificClass.GetAttributes().Where(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, attributeType)))
            {
                if (attribute.ConstructorArguments[0].Value is not string propertyName)
                    continue;

                if (attribute.ConstructorArguments[1].Value is not INamedTypeSymbol type)
                    continue;

                var isSetterPublic = true;
                string? defaultValue = null;
                var isNullable = false;
                var instanceChangedCallback = false;

                foreach (var namedArgument in attribute.NamedArguments)
                    if (namedArgument.Value.Value is { } value)
                        switch (namedArgument.Key)
                        {
                            case "IsSetterPublic": isSetterPublic = (bool)value; break;
                            case "DefaultValue": defaultValue = (string)value; break;
                            case "IsNullable": isNullable = (bool)value; break;
                            case "InstanceChangedCallback": instanceChangedCallback = (bool)value; break;
                        }

                var fieldName = propertyName + "Property";

                namespaces.UseNamespace(usedTypes, specificClass, type);
                var defaultValueExpression = defaultValue is null ? LiteralExpression(SyntaxKind.NullLiteralExpression) : ParseExpression(defaultValue);
                var metadataCreation = GetObjectCreationExpression(defaultValueExpression);
                if (instanceChangedCallback)
                {
                    var partialMethodName = $"On{propertyName}Changed";
                    var oldValueExpression = GetCastExpression(isNullable, type, "OldValue");
                    var newValueExpression = GetCastExpression(isNullable, type, "NewValue");
                    var lambdaBody = GetInvocationExpression(partialMethodName, specificClass, oldValueExpression, newValueExpression);
                    metadataCreation = GetMetadataCreation(metadataCreation, lambdaBody);
                    var partialMethod = GetMethodDeclaration(partialMethodName, isNullable, type);
                    members.Add(partialMethod);
                }
                var registration = GetRegistration(propertyName, type, specificClass, metadataCreation);
                var staticFieldDeclaration = GetStaticFieldDeclaration(fieldName, registration);
                var getter = GetGetter(fieldName, isNullable, type ,context);
                var setter = GetSetter(fieldName, isSetterPublic);
                var propertyDeclaration = GetPropertyDeclaration(propertyName, isNullable, type, getter, setter);

                members.Add(staticFieldDeclaration);
                members.Add(propertyDeclaration);
            }

            if (members.Count > 0)
            {
                var generatedClass = GetClassDeclaration(specificClass, members);
                var generatedNamespace = GetNamespaceDeclaration(specificClass, generatedClass);
                var compilationUnit = GetCompilationUnit(generatedNamespace, namespaces);
                var fileName = specificClass.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat
                    .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)) + ".g.cs";
                context.AddSource(fileName, SyntaxTree(compilationUnit, encoding: Encoding.UTF8).GetText());
            }
        }
    }

    /// <summary>
    /// 生成如下代码
    /// <code>
    /// new PropertyMetadata(<paramref name="defaultValueExpression"/>);
    /// </code>
    /// </summary>
    /// <returns>ObjectCreationExpression</returns>
    private static ObjectCreationExpressionSyntax GetObjectCreationExpression(ExpressionSyntax defaultValueExpression)
    {
        return ObjectCreationExpression(IdentifierName("PropertyMetadata"))
            .AddArgumentListArguments(Argument(defaultValueExpression));
    }
    
    /// <summary>
    /// 生成如下代码
    /// <code>
    /// <paramref name="type"/>&lt;<paramref name="isNullable"/>&gt; <paramref name="identifierName"/>;
    /// </code>
    /// </summary>
    /// <returns>CastExpression</returns>
    private static CastExpressionSyntax GetCastExpression(bool isNullable, INamedTypeSymbol type, string identifierName)
    {
        return CastExpression(type.GetTypeSyntax(isNullable),
            MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName("e"), IdentifierName(identifierName)));
    }

    /// <summary>
    /// 生成如下代码
    /// <code>
    /// (d, e) => ((<paramref name="specificClass"/>)d).<paramref name="partialMethodName"/>((<paramref name="oldValueExpression"/>)e.OldValue, (<paramref name="newValueExpression"/>)e.NewValue)
    /// </code>
    /// </summary>
    /// <returns>InvocationExpression</returns>
    private static InvocationExpressionSyntax GetInvocationExpression(string partialMethodName,  INamedTypeSymbol specificClass, CastExpressionSyntax oldValueExpression, CastExpressionSyntax newValueExpression)
    {
        return InvocationExpression(MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                ParenthesizedExpression(CastExpression(specificClass.GetTypeSyntax(false), IdentifierName("d"))),
                IdentifierName(partialMethodName)))
            .AddArgumentListArguments(Argument(oldValueExpression), Argument(newValueExpression));
    }

    /// <summary>
    /// 生成如下代码
    /// <code>
    /// new PropertyMetadata(<paramref name="metadataCreation"/>, <paramref name="lambdaBody"/>)
    /// </code>
    /// </summary>
    /// <returns>MetadataCreation</returns>
    private static ObjectCreationExpressionSyntax GetMetadataCreation(ObjectCreationExpressionSyntax metadataCreation, InvocationExpressionSyntax lambdaBody)
    {
        return metadataCreation.AddArgumentListArguments(Argument(ParenthesizedLambdaExpression()
            .AddParameterListParameters(Parameter(Identifier("d")), Parameter(Identifier("e")))
            .WithExpressionBody(lambdaBody)));
    }

    /// <summary>
    /// 生成如下代码
    /// <code>
    /// partial void <paramref name="partialMethodName"/>(<paramref name="type"/>&lt;<paramref name="isNullable"/>&gt; oldValue, <paramref name="type"/>&lt;<paramref name="isNullable"/>&gt; newValue);
    /// </code>
    /// </summary>
    /// <returns>MethodDeclaration</returns>
    private static MethodDeclarationSyntax GetMethodDeclaration(string partialMethodName, bool isNullable, INamedTypeSymbol type)
    {
        return MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), partialMethodName)
            .AddParameterListParameters(
                Parameter(Identifier("oldValue")).WithType(type.GetTypeSyntax(isNullable)),
                Parameter(Identifier("newValue")).WithType(type.GetTypeSyntax(isNullable)))
            .AddModifiers(Token(SyntaxKind.PartialKeyword))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
    }

    /// <summary>
    /// 生成如下代码
    /// <code>
    /// DependencyProperty.Register("<paramref name="propertyName"/>", typeof(<paramref name="type"/>), typeof(<paramref name="specificClass"/>), <paramref name="metadataCreation"/>);
    /// </code>
    /// </summary>
    /// <returns>Registration</returns>
    private static InvocationExpressionSyntax GetRegistration(string propertyName, INamedTypeSymbol type, INamedTypeSymbol specificClass, ObjectCreationExpressionSyntax metadataCreation)
    {
        return InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("DependencyProperty"),
                    IdentifierName("Register")))
            .AddArgumentListArguments(
                Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(propertyName))),
                Argument(TypeOfExpression(type.GetTypeSyntax(false))),
                Argument(TypeOfExpression(specificClass.GetTypeSyntax(false))),
                Argument(metadataCreation));
    }

    /// <summary>
    /// 生成如下代码
    /// <code>
    /// public static readonly DependencyProperty <paramref name="fieldName"/> = <paramref name="registration"/>;
    /// </code>
    /// </summary>
    /// <returns>StaticFieldDeclaration</returns>
    private static FieldDeclarationSyntax GetStaticFieldDeclaration(string fieldName, InvocationExpressionSyntax registration)
    {
        return FieldDeclaration(VariableDeclaration(IdentifierName("DependencyProperty")))
            .AddModifiers(
                Token(SyntaxKind.PublicKeyword),
                Token(SyntaxKind.StaticKeyword),
                Token(SyntaxKind.ReadOnlyKeyword))
            .AddDeclarationVariables(VariableDeclarator(fieldName).WithInitializer(EqualsValueClause(registration)));
    }

    /// <summary>
    /// 生成如下代码
    /// <code>
    /// get => (<paramref name="type"/>&lt;<paramref name="isNullable"/>&gt;)GetValue(<paramref name="fieldName"/>);
    /// </code>
    /// </summary>
    /// <returns>Getter</returns>
    private static AccessorDeclarationSyntax GetGetter(string fieldName, bool isNullable, INamedTypeSymbol type, GeneratorExecutionContext context)
    {
        ExpressionSyntax getProperty = InvocationExpression(IdentifierName("GetValue"))
            .AddArgumentListArguments(Argument(IdentifierName(fieldName)));
        if (!SymbolEqualityComparer.Default.Equals(type, context.Compilation.GetSpecialType(SpecialType.System_Object)))
            getProperty = CastExpression(type.GetTypeSyntax(isNullable), getProperty);

        return AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
            .WithExpressionBody(ArrowExpressionClause(getProperty))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
    }

    /// <summary>
    /// 生成如下代码
    /// <code>
    /// &lt;<paramref name="isSetterPublic"/>&gt; set => SetValue(<paramref name="fieldName"/>, value);
    /// </code>
    /// </summary>
    /// <returns>Setter</returns>
    private static AccessorDeclarationSyntax GetSetter(string fieldName, bool isSetterPublic)
    {
        ExpressionSyntax setProperty = InvocationExpression(IdentifierName("SetValue"))
            .AddArgumentListArguments(
                Argument(IdentifierName(fieldName)),
                Argument(IdentifierName("value")));
        var setter = AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
            .WithExpressionBody(ArrowExpressionClause(setProperty))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        return !isSetterPublic ? setter.AddModifiers(Token(SyntaxKind.PrivateKeyword)) : setter;
    }

    /// <summary>
    /// 生成如下代码
    /// <code>
    /// public <paramref name="type"/>&lt;<paramref name="isNullable"/>&gt; <paramref name="propertyName"/> { <paramref name="getter"/>; <paramref name="setter"/>; }
    /// </code>
    /// </summary>
    /// <returns>PropertyDeclaration</returns>
    private static PropertyDeclarationSyntax GetPropertyDeclaration(string propertyName, bool isNullable, INamedTypeSymbol type, AccessorDeclarationSyntax getter, AccessorDeclarationSyntax setter)
    {
        return PropertyDeclaration(type.GetTypeSyntax(isNullable), propertyName)
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddAccessorListAccessors(getter, setter);
    }

    /// <summary>
    /// 生成如下代码
    /// <code>
    /// partial class <paramref name="specificClass"/><br/>
    /// {<br/> <paramref name="members"/><br/>}
    /// </code>
    /// </summary>
    /// <returns>ClassDeclaration</returns>
    private static ClassDeclarationSyntax GetClassDeclaration(INamedTypeSymbol specificClass, List<MemberDeclarationSyntax> members)
    {
        return ClassDeclaration(specificClass.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat))
            .AddModifiers(Token(SyntaxKind.PartialKeyword))
            .AddMembers(members.ToArray());
    }

    /// <summary>
    /// 生成如下代码
    /// <code>
    /// namespace <paramref name="specificClass"/>.ContainingNamespace<br/>
    /// {<br/> <paramref name="generatedClass"/><br/>}
    /// </code>
    /// </summary>
    /// <returns>NamespaceDeclaration</returns>
    private static NamespaceDeclarationSyntax GetNamespaceDeclaration(INamedTypeSymbol specificClass, ClassDeclarationSyntax generatedClass)
    {
        return NamespaceDeclaration(ParseName(specificClass.ContainingNamespace.ToDisplayString()))
            .AddMembers(generatedClass)
            .WithNamespaceKeyword(Token(SyntaxKind.NamespaceKeyword)
                .WithLeadingTrivia(Trivia(NullableDirectiveTrivia(Token(SyntaxKind.EnableKeyword), true))));
    }

    /// <summary>
    /// 生成如下代码
    /// <code>
    /// using Microsoft.UI.Xaml;<br/>
    /// ...<br/>
    /// <paramref name="generatedNamespace"/>
    /// </code>
    /// </summary>
    /// <returns>CompilationUnit</returns>
    private static CompilationUnitSyntax GetCompilationUnit(NamespaceDeclarationSyntax generatedNamespace, HashSet<string> namespaces)
    {
        return CompilationUnit()
            .AddMembers(generatedNamespace)
            .AddUsings(namespaces.Select(ns => UsingDirective(ParseName(ns))).ToArray())
            .NormalizeWhitespace();
    }
}