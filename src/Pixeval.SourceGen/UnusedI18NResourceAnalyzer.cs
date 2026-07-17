// Copyright (c) Pixeval.SourceGen.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Pixeval.SourceGen;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UnusedI18NResourceAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "PE1002";

    private const string Category = "Localization";
    private const string PixevalNamespaceName = nameof(Pixeval);
    private const string ResourcesSuffix = "Resources";

    private static readonly DiagnosticDescriptor _Rule = new(
        DiagnosticId,
        "Unused i18n resource",
        "The i18n resource '{0}' is not used",
        Category,
        DiagnosticSeverity.Warning,
        true,
        customTags: [WellKnownDiagnosticTags.CompilationEnd]);

    private static readonly Regex _XamlStaticResourceRegex = new(
        @"\{x:Static\s+(?:(?:[\w.]+):)?(?<type>[A-Za-z_][A-Za-z0-9_]*Resources)\.(?<member>[A-Za-z_][A-Za-z0-9_]*)",
        RegexOptions.Compiled);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [_Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(static context =>
        {
            var resourceInfo = ResourceInfo.FromCompilation(context.Compilation);
            if (resourceInfo.Keys.Count is 0)
                return;

            var usedResourceKeys = new ConcurrentDictionary<string, byte>(StringComparer.Ordinal);

            context.RegisterSyntaxNodeAction(context =>
            {
                var symbol = context.SemanticModel.GetSymbolInfo(context.Node, context.CancellationToken).Symbol;
                if (symbol is IFieldSymbol fieldSymbol && resourceInfo.TryGetResourceKey(fieldSymbol, out var resourceKey))
                {
                    usedResourceKeys.TryAdd(resourceKey, 0);
                }
            }, SyntaxKind.SimpleMemberAccessExpression, SyntaxKind.IdentifierName);

            foreach (var xamlFile in context.Options.AdditionalFiles.Where(static file => IsXamlFile(file.Path)))
            {
                var sourceText = xamlFile.GetText(context.CancellationToken);
                if (sourceText is null)
                    continue;

                AddXamlUsedResourceKeys(sourceText.ToString(), resourceInfo, usedResourceKeys);
            }

            context.RegisterCompilationEndAction(context =>
            {
                foreach (var resourceFile in context.Options.AdditionalFiles.Where(static file => IsI18NJsonFile(file.Path)))
                {
                    var sourceText = resourceFile.GetText(context.CancellationToken);
                    if (sourceText is null)
                        continue;

                    foreach (var entry in JsonResourceScanner.Scan(resourceFile.Path, sourceText))
                    {
                        if (!resourceInfo.Keys.Contains(entry.ResourceKey) || usedResourceKeys.ContainsKey(entry.ResourceKey))
                            continue;

                        context.ReportDiagnostic(Diagnostic.Create(
                            _Rule,
                            Location.Create(resourceFile.Path, entry.KeyNameSpan, sourceText.Lines.GetLinePositionSpan(entry.KeyNameSpan)),
                            entry.ResourceKey));
                    }
                }
            });
        });
    }

    private static void AddXamlUsedResourceKeys(string text, ResourceInfo resourceInfo, ConcurrentDictionary<string, byte> usedResourceKeys)
    {
        foreach (Match match in _XamlStaticResourceRegex.Matches(text))
        {
            if (IsInXmlComment(text, match.Index))
                continue;

            var memberKey = match.Groups["type"].Value + "." + match.Groups["member"].Value;
            if (resourceInfo.KeysByMember.TryGetValue(memberKey, out var resourceKey))
            {
                usedResourceKeys.TryAdd(resourceKey, 0);
            }
        }
    }

    private static bool IsInXmlComment(string text, int index)
    {
        var commentStart = text.LastIndexOf("<!--", index, StringComparison.Ordinal);
        if (commentStart < 0)
            return false;

        var commentEnd = text.LastIndexOf("-->", index, StringComparison.Ordinal);
        return commentEnd < commentStart;
    }

    private static bool IsXamlFile(string path) => string.Equals(Path.GetExtension(path), ".axaml", StringComparison.OrdinalIgnoreCase);

    private static bool IsI18NJsonFile(string path)
        => string.Equals(Path.GetExtension(path), ".json", StringComparison.OrdinalIgnoreCase)
           && path.Split('/', '\\').Any(static part => string.Equals(part, "i18n", StringComparison.OrdinalIgnoreCase));

    private sealed class ResourceInfo
    {
        private ResourceInfo(ImmutableHashSet<string> keys, ImmutableDictionary<string, string> keysByMember)
        {
            Keys = keys;
            KeysByMember = keysByMember;
        }

        public ImmutableHashSet<string> Keys { get; }

        public ImmutableDictionary<string, string> KeysByMember { get; }

        public static ResourceInfo FromCompilation(Compilation compilation)
        {
            var keys = ImmutableHashSet.CreateBuilder<string>(StringComparer.Ordinal);
            var keysByMember = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.Ordinal);

            foreach (var typeSymbol in GetPixevalTypes(compilation.GlobalNamespace))
            {
                if (!typeSymbol.IsStatic || !typeSymbol.Name.EndsWith(ResourcesSuffix, StringComparison.Ordinal))
                    continue;

                foreach (var fieldSymbol in typeSymbol.GetMembers().OfType<IFieldSymbol>())
                {
                    if (!TryGetConstResourceKey(fieldSymbol, out var resourceKey))
                        continue;

                    keys.Add(resourceKey);
                    keysByMember[typeSymbol.Name + "." + fieldSymbol.Name] = resourceKey;
                }
            }

            return new ResourceInfo(keys.ToImmutable(), keysByMember.ToImmutable());
        }

        public bool TryGetResourceKey(IFieldSymbol fieldSymbol, out string resourceKey)
        {
            resourceKey = "";
            if (!TryGetConstResourceKey(fieldSymbol, out var key) || !Keys.Contains(key))
                return false;

            resourceKey = key;
            return true;
        }

        private static bool TryGetConstResourceKey(IFieldSymbol fieldSymbol, out string resourceKey)
        {
            resourceKey = "";
            if (!fieldSymbol.IsConst
                || fieldSymbol.ConstantValue is not string key
                || fieldSymbol.ContainingType is not { IsStatic: true } typeSymbol
                || !typeSymbol.Name.EndsWith(ResourcesSuffix, StringComparison.Ordinal)
                || !IsPixevalNamespace(typeSymbol.ContainingNamespace))
            {
                return false;
            }

            resourceKey = key;
            return true;
        }

        private static IEnumerable<INamedTypeSymbol> GetPixevalTypes(INamespaceSymbol globalNamespace)
        {
            foreach (var namespaceSymbol in globalNamespace.GetNamespaceMembers())
            {
                if (namespaceSymbol.Name is PixevalNamespaceName)
                {
                    foreach (var typeSymbol in GetTypes(namespaceSymbol))
                    {
                        yield return typeSymbol;
                    }
                }
            }
        }

        private static IEnumerable<INamedTypeSymbol> GetTypes(INamespaceOrTypeSymbol symbol)
        {
            foreach (var member in symbol.GetMembers())
            {
                switch (member)
                {
                    case INamespaceSymbol namespaceSymbol:
                        foreach (var typeSymbol in GetTypes(namespaceSymbol))
                        {
                            yield return typeSymbol;
                        }

                        break;
                    case INamedTypeSymbol typeSymbol:
                        yield return typeSymbol;
                        foreach (var nestedTypeSymbol in GetTypes(typeSymbol))
                        {
                            yield return nestedTypeSymbol;
                        }

                        break;
                }
            }
        }

        private static bool IsPixevalNamespace(INamespaceSymbol? namespaceSymbol)
        {
            for (var current = namespaceSymbol; current is not null && !current.IsGlobalNamespace; current = current.ContainingNamespace)
            {
                if (current.Name is PixevalNamespaceName && current.ContainingNamespace.IsGlobalNamespace)
                    return true;
            }

            return false;
        }
    }

    private readonly record struct JsonResourceEntry(string ResourceKey, TextSpan KeyNameSpan);

    private sealed class JsonResourceScanner
    {
        private readonly string _fileRoot;
        private readonly string _text;
        private readonly List<JsonResourceEntry> _entries = [];
        private int _position;

        private JsonResourceScanner(string path, SourceText sourceText)
        {
            // AdditionalText paths may use separators from a different operating system.
            var fileNameStart = Math.Max(path.LastIndexOf('/'), path.LastIndexOf('\\')) + 1;
            _fileRoot = Path.GetFileNameWithoutExtension(path.Substring(fileNameStart));
            _text = sourceText.ToString();
        }

        public static IReadOnlyList<JsonResourceEntry> Scan(string path, SourceText sourceText)
        {
            var scanner = new JsonResourceScanner(path, sourceText);
            return scanner.TryParseValue(scanner._fileRoot, default) ? scanner._entries : [];
        }

        private bool TryParseValue(string path, TextSpan keyNameSpan)
        {
            SkipWhitespace();
            return Current switch
            {
                '{' => TryParseObject(path),
                '[' => TryParseArray(path, keyNameSpan),
                '"' => TryParseString(out _, out _) && AddEntry(path, keyNameSpan),
                '-' or >= '0' and <= '9' => TryParseNumber() && AddEntry(path, keyNameSpan),
                't' => TryParseLiteral("true") && AddEntry(path, keyNameSpan),
                'f' => TryParseLiteral("false") && AddEntry(path, keyNameSpan),
                'n' => TryParseLiteral("null") && AddEntry(path, keyNameSpan),
                _ => false
            };
        }

        private bool TryParseObject(string path)
        {
            if (!TryConsume('{'))
                return false;

            SkipWhitespace();
            if (TryConsume('}'))
                return true;

            while (true)
            {
                SkipWhitespace();
                if (!TryParseString(out var propertyName, out var propertyNameSpan))
                    return false;

                SkipWhitespace();
                if (!TryConsume(':'))
                    return false;

                var childPath = path + "." + propertyName;
                if (!TryParseValue(childPath, propertyNameSpan))
                    return false;

                SkipWhitespace();
                if (TryConsume('}'))
                    return true;

                if (!TryConsume(','))
                    return false;
            }
        }

        private bool TryParseArray(string path, TextSpan keyNameSpan)
        {
            if (!TryConsume('['))
                return false;

            SkipWhitespace();
            if (TryConsume(']'))
                return true;

            var index = 0;
            while (true)
            {
                if (!TryParseValue($"{path}[{index}]", keyNameSpan))
                    return false;

                index++;
                SkipWhitespace();
                if (TryConsume(']'))
                    return true;

                if (!TryConsume(','))
                    return false;
            }
        }

        private bool TryParseString(out string value, out TextSpan valueSpan)
        {
            value = "";
            valueSpan = default;

            if (!TryConsume('"'))
                return false;

            var valueStart = _position;
            StringBuilder? builder = null;
            while (_position < _text.Length)
            {
                var ch = _text[_position++];
                switch (ch)
                {
                    case '"':
                        valueSpan = TextSpan.FromBounds(valueStart, _position - 1);
                        value = builder?.ToString() ?? _text.Substring(valueStart, _position - valueStart - 1);
                        return true;
                    case '\\':
                        builder ??= new StringBuilder(_text.Substring(valueStart, _position - valueStart - 1));
                        if (!TryAppendEscapedCharacter(builder))
                            return false;

                        break;
                    default:
                        builder?.Append(ch);
                        break;
                }
            }

            return false;
        }

        private bool TryAppendEscapedCharacter(StringBuilder builder)
        {
            if (_position >= _text.Length)
                return false;

            var ch = _text[_position++];
            switch (ch)
            {
                case '"':
                case '\\':
                case '/':
                    builder.Append(ch);
                    return true;
                case 'b':
                    builder.Append('\b');
                    return true;
                case 'f':
                    builder.Append('\f');
                    return true;
                case 'n':
                    builder.Append('\n');
                    return true;
                case 'r':
                    builder.Append('\r');
                    return true;
                case 't':
                    builder.Append('\t');
                    return true;
                case 'u':
                    return TryAppendUnicodeEscape(builder);
                default:
                    return false;
            }
        }

        private bool TryAppendUnicodeEscape(StringBuilder builder)
        {
            if (_position + 4 > _text.Length)
                return false;

            var value = 0;
            for (var i = 0; i < 4; i++)
            {
                var digit = HexValue(_text[_position + i]);
                if (digit < 0)
                    return false;

                value = (value << 4) + digit;
            }

            _position += 4;
            builder.Append((char) value);
            return true;
        }

        private bool TryParseNumber()
        {
            if (Current is '-')
                _position++;

            if (!TryParseDigits())
                return false;

            if (Current is '.')
            {
                _position++;
                if (!TryParseDigits())
                    return false;
            }

            if (Current is 'e' or 'E')
            {
                _position++;
                if (Current is '+' or '-')
                    _position++;

                if (!TryParseDigits())
                    return false;
            }

            return true;
        }

        private bool TryParseDigits()
        {
            var start = _position;
            while (Current is >= '0' and <= '9')
            {
                _position++;
            }

            return _position > start;
        }

        private bool TryParseLiteral(string literal)
        {
            if (_position + literal.Length > _text.Length)
                return false;

            for (var i = 0; i < literal.Length; i++)
            {
                if (_text[_position + i] != literal[i])
                    return false;
            }

            _position += literal.Length;
            return true;
        }

        private bool AddEntry(string path, TextSpan keyNameSpan)
        {
            if (!keyNameSpan.IsEmpty)
                _entries.Add(new JsonResourceEntry(path, keyNameSpan));

            return true;
        }

        private bool TryConsume(char ch)
        {
            if (Current != ch)
                return false;

            _position++;
            return true;
        }

        private void SkipWhitespace()
        {
            while (Current is ' ' or '\t' or '\r' or '\n')
            {
                _position++;
            }
        }

        private char Current => _position < _text.Length ? _text[_position] : '\0';

        private static int HexValue(char ch) => ch switch
        {
            >= '0' and <= '9' => ch - '0',
            >= 'a' and <= 'f' => ch - 'a' + 10,
            >= 'A' and <= 'F' => ch - 'A' + 10,
            _ => -1
        };
    }
}
