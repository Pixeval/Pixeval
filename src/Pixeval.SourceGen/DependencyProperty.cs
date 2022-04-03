using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Pixeval.SourceGen.Utils;

namespace Pixeval.SourceGen;

internal static partial class TypeWithAttributeDelegates
{
    public static string? DependencyProperty(TypeDeclarationSyntax typeDeclaration, INamedTypeSymbol typeSymbol, Func<AttributeData, bool> attributeEqualityComparer)
    {
        var members = new List<MemberDeclarationSyntax>();
        var namespaces = new HashSet<string> { "Microsoft.UI.Xaml" };
        var usedTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

        foreach (var attribute in typeSymbol.GetAttributes().Where(attributeEqualityComparer))
        {
            if (attribute.ConstructorArguments[0].Value is not string propertyName || attribute.ConstructorArguments[1].Value is not INamedTypeSymbol type)
                continue;

            if (attribute.ConstructorArguments.Length < 3 || attribute.ConstructorArguments[2].Value is not string propertyChanged)
                continue;

            var isSetterPublic = true;
            var defaultValue = "DependencyProperty.UnsetValue";
            var isNullable = false;

            foreach (var namedArgument in attribute.NamedArguments)
                if (namedArgument.Value.Value is { } value)
                    switch (namedArgument.Key)
                    {
                        case "IsSetterPublic":
                            isSetterPublic = (bool)value;
                            break;
                        case "DefaultValue":
                            defaultValue = (string)value;
                            break;
                        case "IsNullable":
                            isNullable = (bool)value;
                            break;
                    }

            var fieldName = propertyName + "Property";

            namespaces.UseNamespace(usedTypes, typeSymbol, type);
            var defaultValueExpression = ParseExpression(defaultValue);
            var metadataCreation = GetObjectCreationExpression(defaultValueExpression);
            if (propertyChanged is not "")
                metadataCreation = GetMetadataCreation(metadataCreation, propertyChanged);

            var registration = GetRegistration(propertyName, type, typeSymbol, metadataCreation);
            var staticFieldDeclaration = GetStaticFieldDeclaration(fieldName, registration);
            var getter = GetGetter(fieldName, isNullable, type);
            var setter = GetSetter(fieldName, isSetterPublic);
            var propertyDeclaration = GetPropertyDeclaration(propertyName, isNullable, type, getter, setter);

            members.Add(staticFieldDeclaration);
            members.Add(propertyDeclaration);
        }

        if (members.Count > 0)
        {
            var generatedClass = GetClassDeclaration(typeSymbol, members);
            var generatedNamespace = GetNamespaceDeclaration(typeSymbol, generatedClass);
            var compilationUnit = GetCompilationUnit(generatedNamespace, namespaces);
            return SyntaxTree(compilationUnit, encoding: Encoding.UTF8).GetText().ToString();
        }

        return null;
    }

}