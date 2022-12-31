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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Pixeval.SourceGen.Utils;

namespace Pixeval.SourceGen;

internal static partial class TypeWithAttributeDelegates
{
    public static string? DependencyProperty(INamedTypeSymbol typeSymbol, ImmutableArray<AttributeData> attributeList)
    {
        var members = new List<MemberDeclarationSyntax>();
        var namespaces = new HashSet<string> { "Microsoft.UI.Xaml" };
        var usedTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

        foreach (var attribute in attributeList)
        {
            if (attribute.AttributeClass is not ({ IsGenericType: true } and { TypeArguments.IsDefaultOrEmpty: false }))
                return null;
            var type = attribute.AttributeClass.TypeArguments[0];
            if (attribute.ConstructorArguments[0].Value is not string propertyName)
                continue;

            if (attribute.ConstructorArguments.Length < 2 || attribute.ConstructorArguments[1].Value is not string propertyChanged)
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
            var generatedNamespace = GetFileScopedNamespaceDeclaration(typeSymbol, generatedClass);
            var compilationUnit = GetCompilationUnit(generatedNamespace, namespaces);
            return SyntaxTree(compilationUnit, encoding: Encoding.UTF8).GetText().ToString();
        }

        return null;
    }
}
