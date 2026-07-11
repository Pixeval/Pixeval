// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentIcons.Common;
using Pixeval.I18N;
using Pixeval.Utilities;
using SharpYaml;
using SharpYaml.Model;

namespace Pixeval.Models.Navigation;

public static class NavigationYamlParser
{
    public const int MaxDepth = 3;

    private const string SettingsPageKey = "Settings";

    private static readonly IReadOnlySet<string> _DocumentKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "newTab",
        "header",
        "footer"
    };

    private static readonly IReadOnlySet<string> _PageKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "page",
        "title",
        "icon"
    };

    private static readonly IReadOnlySet<string> _FolderKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "folder",
        "icon",
        "children"
    };

    private static readonly IReadOnlySet<string> _ItemKeys = CreateItemKeys();

    public static NavigationParseResult Parse(string? yaml)
    {
        var text = string.IsNullOrWhiteSpace(yaml) ? NavigationMenuYaml.DefaultYaml : yaml;
        var diagnostics = new List<NavigationDiagnostic>();

        NavigationYamlSourceMap sourceMap;
        try
        {
            sourceMap = NavigationYamlSourceMap.Parse(text);
        }
        catch (YamlException exception)
        {
            diagnostics.Add(CreateYamlExceptionDiagnostic(exception, text));
            return new(null, null, diagnostics);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new(
                exception.Message,
                0,
                int.Min(text.Length, 1),
                1,
                1));
            return new(null, null, diagnostics);
        }

        NavigationYamlSettings? document;
        try
        {
            document = YamlSerializer.Deserialize(text, NavigationYamlSerializerContext.Default.NavigationYamlSettings);
        }
        catch (YamlException exception)
        {
            diagnostics.Add(CreateYamlExceptionDiagnostic(exception, text));
            return new(null, null, diagnostics);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new(
                exception.Message,
                0,
                int.Min(text.Length, 1),
                1,
                1));
            return new(null, null, diagnostics);
        }

        document ??= new();
        diagnostics.AddRange(sourceMap.CreateUnknownKeyDiagnostics());

        var newTab = string.IsNullOrWhiteSpace(document.NewTab) ? null : document.NewTab.Trim();
        NavigationPageDefinition? newTabDefinition = null;
        if (newTab is not null && !NavigationPageRegistry.TryGetPage(newTab, out newTabDefinition))
        {
            diagnostics.Add(CreateValueDiagnostic(
                sourceMap,
                "newTab",
                newTab,
                Diagnostic(NavigationYamlParserResources.DiagnosticsUnknownPageFormatted, newTab)));
        }

        var header = document.Header.BuildItems("header", 1, diagnostics, sourceMap);
        var footer = document.Footer.BuildItems("footer", 1, diagnostics, sourceMap);

        if (header.Count is 0 && footer.Count is 0)
        {
            diagnostics.Add(new(
                Diagnostic(NavigationYamlParserResources.DiagnosticsBothHeaderAndFooterEmpty),
                0,
                int.Min(text.Length, 1),
                1,
                1));
        }

        if (!header.ContainsPage(SettingsPageKey) && !footer.ContainsPage(SettingsPageKey))
        {
            diagnostics.Add(new(
                Diagnostic(NavigationYamlParserResources.DiagnosticsMenuMustContainSettingsPage),
                0,
                int.Min(text.Length, 1),
                1,
                1));
        }

        var configuration = diagnostics.Count is 0
            ? new NavigationConfiguration(newTab, newTabDefinition, header, footer)
            : null;
        return new(document, configuration, diagnostics);
    }

    public static NavigationConfiguration ParseOrDefault(string? yaml)
    {
        var result = Parse(yaml);
        if (result.Configuration is { } configuration)
            return configuration;

        var parseResult = Parse(NavigationMenuYaml.DefaultYaml);
        return parseResult.Configuration
               ?? throw new InvalidOperationException(Diagnostic(NavigationYamlParserResources.DiagnosticsBuiltInNavigationYamlInvalid));
    }

    extension(IReadOnlyList<NavigationMenuItem> items)
    {
        private bool ContainsPage(string key) =>
            items.Any(item => item switch
            {
                NavigationPageItem page => string.Equals(page.PageKey, key, StringComparison.OrdinalIgnoreCase),
                NavigationFolderItem folder => folder.Children.ContainsPage(key),
                _ => false
            });
    }

    extension(IReadOnlyList<NavigationYamlItem>? source)
    {
        private List<NavigationMenuItem> BuildItems(string path,
            int depth,
            List<NavigationDiagnostic> diagnostics,
            NavigationYamlSourceMap sourceMap)
        {
            if (source is null or { Count: 0 })
                return [];

            var items = new List<NavigationMenuItem>();
            for (var i = 0; i < source.Count; i++)
            {
                var item = source[i];
                if (item.BuildItem($"{path}[{i}]", depth, diagnostics, sourceMap) is { } menuItem)
                    items.Add(menuItem);
            }

            return items;
        }
    }

    extension(NavigationYamlItem source)
    {
        private NavigationMenuItem? BuildItem(string path,
            int depth,
            List<NavigationDiagnostic> diagnostics,
            NavigationYamlSourceMap sourceMap)
        {
            if (depth > MaxDepth)
            {
                diagnostics.Add(CreateItemDiagnostic(
                    sourceMap,
                    source,
                    path,
                    Diagnostic(NavigationYamlParserResources.DiagnosticsMaxDepthExceededFormatted, path, MaxDepth)));
                return null;
            }

            var page = source.Page?.Trim();
            var folder = source.Folder?.Trim();
            var hasPage = !string.IsNullOrWhiteSpace(page);
            var hasFolder = !string.IsNullOrWhiteSpace(folder);

            if (hasPage == hasFolder)
            {
                diagnostics.AddRange(sourceMap.CreateItemFieldDiagnostics(path, _ItemKeys));
                diagnostics.Add(CreateItemDiagnostic(
                    sourceMap,
                    source,
                    path,
                    Diagnostic(NavigationYamlParserResources.DiagnosticsItemMustHaveEitherPageOrFolderFormatted, path)));
                return null;
            }

            if (hasPage)
            {
                diagnostics.AddRange(sourceMap.CreateItemFieldDiagnostics(path, _PageKeys));
                return source.BuildPageItem(page!, path, diagnostics, sourceMap);
            }

            if (depth >= MaxDepth)
            {
                diagnostics.Add(CreateItemDiagnostic(
                    sourceMap,
                    source,
                    path,
                    Diagnostic(NavigationYamlParserResources.DiagnosticsMaxDepthExceededFolderFormatted, path, MaxDepth)));
                return null;
            }

            diagnostics.AddRange(sourceMap.CreateItemFieldDiagnostics(path, _FolderKeys));
            return source.BuildFolderItem(folder!, path, depth, diagnostics, sourceMap);
        }

        private NavigationPageItem? BuildPageItem(string page,
            string path,
            List<NavigationDiagnostic> diagnostics,
            NavigationYamlSourceMap sourceMap)
        {
            if (!NavigationPageRegistry.TryGetPage(page, out var definition))
            {
                diagnostics.Add(CreateValueDiagnostic(
                    sourceMap,
                    $"{path}.page",
                    page,
                    Diagnostic(NavigationYamlParserResources.DiagnosticsUnknownPageFormatted, page)));
                return null;
            }

            var icon = ResolveIcon(source.Icon, definition.Icon, $"{path}.icon", sourceMap, diagnostics);
            var (header, headerSource) = ResolveDisplayText(source.Title, definition.Header, "title", $"{path}.title", sourceMap, diagnostics);

            return new NavigationPageItem(
                definition.PageType,
                definition.Key,
                header,
                headerSource,
                icon,
                definition.NeedLogin);
        }

        private NavigationMenuItem BuildFolderItem(string folder,
            string path,
            int depth,
            List<NavigationDiagnostic> diagnostics,
            NavigationYamlSourceMap sourceMap)
        {
            var children = source.Children.BuildItems($"{path}.children", depth + 1, diagnostics, sourceMap);
            if (children.Count is 0)
            {
                diagnostics.Add(CreateValueDiagnostic(
                    sourceMap,
                    $"{path}.folder",
                    folder,
                    Diagnostic(NavigationYamlParserResources.DiagnosticsEmptyFolderFormatted, folder)));
            }

            var icon = ResolveIcon(source.Icon, Symbol.Folder, $"{path}.icon", sourceMap, diagnostics);
            var (header, headerSource) = ResolveDisplayText(folder, folder, "folder", $"{path}.folder", sourceMap, diagnostics);
            var needLogin = children.All(static child => child.NeedLogin);

            return new NavigationFolderItem(
                header,
                headerSource,
                icon,
                needLogin,
                children);
        }
    }

    private static Symbol ResolveIcon(
        string? icon,
        Symbol fallback,
        string path,
        NavigationYamlSourceMap sourceMap,
        List<NavigationDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(icon))
            return fallback;

        var trimmed = icon.Trim();
        if (Enum.TryParse<Symbol>(trimmed, true, out var symbol))
            return symbol;

        diagnostics.Add(CreateValueDiagnostic(
            sourceMap,
            path,
            trimmed,
            Diagnostic(NavigationYamlParserResources.DiagnosticsUnknownIconFormatted, trimmed)));
        return fallback;
    }

    private static (string Header, string? Source) ResolveDisplayText(
        string? text,
        string fallback,
        string fieldName,
        string path,
        NavigationYamlSourceMap sourceMap,
        List<NavigationDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(text))
            return (fallback, null);

        var trimmed = text.Trim();
        if (!trimmed.StartsWith('$'))
            return (trimmed, trimmed);

        if (trimmed.StartsWith("$$", StringComparison.Ordinal))
            return (trimmed[1..], trimmed);

        var resourceKey = trimmed[1..].Trim();
        if (resourceKey.Length is not 0 && I18NManager.TryGetResource(resourceKey, out var value))
            return (value, trimmed);

        diagnostics.Add(CreateValueDiagnostic(
            sourceMap,
            path,
            trimmed,
            resourceKey.Length is 0
                ? Diagnostic(NavigationYamlParserResources.DiagnosticsI18NResourceKeyEmptyFormatted, fieldName)
                : Diagnostic(NavigationYamlParserResources.DiagnosticsUnknownI18NResourceKeyFormatted, resourceKey)));
        return (trimmed, trimmed);
    }

    private static string Diagnostic(string key, params object?[] args) =>
        args.Length is 0 ? I18NManager.GetResource(key) : I18NManager.GetResource(key, args);

    private static NavigationDiagnostic CreateYamlExceptionDiagnostic(YamlException exception, string text)
    {
        var start = exception.Start.Index >= 0 ? int.Clamp(exception.Start.Index, 0, text.Length) : 0;
        var end = exception.End.Index > start ? int.Clamp(exception.End.Index, start, text.Length) : int.Min(start + 1, text.Length);
        return new(
            exception.Message,
            start,
            int.Max(end - start, 1),
            int.Max(exception.Start.Line + 1, 1),
            int.Max(exception.Start.Column + 1, 1));
    }

    private static NavigationDiagnostic CreateItemDiagnostic(
        NavigationYamlSourceMap sourceMap,
        NavigationYamlItem source,
        string path,
        string message)
    {
        if (!string.IsNullOrWhiteSpace(source.Page))
            return CreateValueDiagnostic(sourceMap, $"{path}.page", source.Page, message);

        if (!string.IsNullOrWhiteSpace(source.Folder))
            return CreateValueDiagnostic(sourceMap, $"{path}.folder", source.Folder, message);

        var location = sourceMap.GetNodeLocation(path) ?? new(0, 1, 1, 1);
        return new(message, location.Start, int.Max(location.Length, 1), location.Line, location.Column);
    }

    private static NavigationDiagnostic CreateValueDiagnostic(
        NavigationYamlSourceMap sourceMap,
        string path,
        string value,
        string message)
    {
        var location = sourceMap.GetValueLocation(path) ?? sourceMap.GetNodeLocation(path) ?? new(0, int.Max(value.Length, 1), 1, 1);
        return new(
            message,
            location.Start,
            int.Max(location.Length, 1),
            location.Line,
            location.Column);
    }

    private static IReadOnlySet<string> CreateItemKeys()
    {
        var keys = new HashSet<string>(_PageKeys, StringComparer.OrdinalIgnoreCase);
        keys.UnionWith(_FolderKeys);
        return keys;
    }

    private readonly record struct TextLocation(int Start, int Length, int Line, int Column);

    private sealed class NavigationYamlSourceMap(string text)
    {
        private readonly Dictionary<string, TextLocation> _keyLocations = new(StringComparer.Ordinal);
        private readonly Dictionary<string, TextLocation> _nodeLocations = new(StringComparer.Ordinal);
        private readonly Dictionary<string, TextLocation> _valueLocations = new(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _mappingKeys = new(StringComparer.Ordinal);

        public static NavigationYamlSourceMap Parse(string text)
        {
            using var reader = new StringReader(text);
            var stream = YamlStream.Load(reader, new YamlNodeTracker());
            var sourceMap = new NavigationYamlSourceMap(text);
            if (stream.Count is not 0 && stream[0].Contents is { } contents)
                sourceMap.Visit("", contents);

            return sourceMap;
        }

        public IReadOnlyList<NavigationDiagnostic> CreateUnknownKeyDiagnostics() =>
            CreateUnknownKeyDiagnostics("", _DocumentKeys);

        public IReadOnlyList<NavigationDiagnostic> CreateItemFieldDiagnostics(string path, IReadOnlySet<string> allowedKeys) =>
            CreateFieldDiagnostics(path, allowedKeys, _ItemKeys);

        public IReadOnlyList<NavigationDiagnostic> CreateUnknownKeyDiagnostics(string path, IReadOnlySet<string> knownKeys)
        {
            var diagnostics = new List<NavigationDiagnostic>();
            foreach (var (fieldPath, key) in EnumerateDirectKeys(path))
            {
                if (knownKeys.Contains(key))
                    continue;

                var location = GetKeyLocation(fieldPath) ?? new(0, int.Max(key.Length, 1), 1, 1);
                diagnostics.Add(new(
                    Diagnostic(NavigationYamlParserResources.DiagnosticsUnknownFieldFormatted, key),
                    location.Start,
                    int.Max(location.Length, 1),
                    location.Line,
                    location.Column));
            }

            return diagnostics;
        }

        private IReadOnlyList<NavigationDiagnostic> CreateFieldDiagnostics(
            string path,
            IReadOnlySet<string> allowedKeys,
            IReadOnlySet<string> schemaKeys)
        {
            var diagnostics = new List<NavigationDiagnostic>();
            foreach (var (fieldPath, key) in EnumerateDirectKeys(path))
            {
                if (allowedKeys.Contains(key))
                    continue;

                var location = GetKeyLocation(fieldPath) ?? new(0, int.Max(key.Length, 1), 1, 1);
                var message = schemaKeys.Contains(key)
                    ? Diagnostic(NavigationYamlParserResources.DiagnosticsFieldNotAllowedFormatted, path, key)
                    : Diagnostic(NavigationYamlParserResources.DiagnosticsUnknownFieldFormatted, key);
                diagnostics.Add(new(
                    message,
                    location.Start,
                    int.Max(location.Length, 1),
                    location.Line,
                    location.Column));
            }

            return diagnostics;
        }

        private IEnumerable<KeyValuePair<string, string>> EnumerateDirectKeys(string path)
        {
            var prefix = path.Length is 0 ? "" : path + ".";
            foreach (var (fieldPath, key) in _mappingKeys)
            {
                if (!fieldPath.StartsWith(prefix, StringComparison.Ordinal))
                    continue;

                var relativePath = fieldPath[prefix.Length..];
                if (relativePath.Contains('.'))
                    continue;

                yield return new(fieldPath, key);
            }
        }

        private TextLocation? GetKeyLocation(string path) =>
            _keyLocations.TryGetValue(path, out var location)
                ? location
                : null;

        public TextLocation? GetNodeLocation(string path) =>
            _nodeLocations.TryGetValue(path, out var location)
                ? location
                : null;

        public TextLocation? GetValueLocation(string path) =>
            _valueLocations.TryGetValue(path, out var location)
                ? location
                : null;

        private void Visit(string path, YamlElement? element)
        {
            if (element is null)
                return;

            RecordNodeLocation(path, element);
            switch (element)
            {
                case YamlMapping mapping:
                    VisitMapping(path, mapping);
                    break;
                case YamlSequence sequence:
                    VisitSequence(path, sequence);
                    break;
            }
        }

        private void VisitMapping(string path, YamlMapping mapping)
        {
            foreach (var pair in mapping)
            {
                if (pair.Key is not YamlValue keyNode)
                    continue;

                var key = keyNode.Value;
                var fieldPath = CombinePath(path, key);
                RecordKeyLocation(fieldPath, keyNode);
                _mappingKeys[fieldPath] = key;
                RecordValueLocation(fieldPath, pair.Value);
                Visit(fieldPath, pair.Value);
            }
        }

        private void VisitSequence(string path, YamlSequence sequence)
        {
            for (var i = 0; i < sequence.Count; i++)
                Visit($"{path}[{i}]", sequence[i]);
        }

        private void RecordNodeLocation(string path, YamlElement element)
        {
            if (TryCreateLocation(element, out var location))
                _nodeLocations[path] = location;
        }

        private void RecordKeyLocation(string path, YamlElement element)
        {
            if (TryCreateLocation(element, out var location))
                _keyLocations[path] = location;
        }

        private void RecordValueLocation(string path, YamlElement? element)
        {
            if (element is not null && TryCreateLocation(element, out var location))
                _valueLocations[path] = location;
        }

        private bool TryCreateLocation(YamlElement element, out TextLocation location) =>
            element switch
            {
                YamlValue value => TryCreateLocation(value.Scalar.Start, value.Scalar.End, out location),
                YamlMapping mapping => TryCreateLocation(mapping.MappingStart.Start, mapping.MappingStart.End, out location),
                YamlSequence sequence => TryCreateLocation(sequence.SequenceStart.Start, sequence.SequenceStart.End, out location),
                _ => CreateEmptyLocation(out location)
            };

        private bool TryCreateLocation(Mark start, Mark end, out TextLocation location)
        {
            if (start.Index < 0)
                return CreateEmptyLocation(out location);

            var startIndex = int.Clamp(start.Index, 0, text.Length);
            var endIndex = end.Index > startIndex
                ? int.Clamp(end.Index, startIndex, text.Length)
                : int.Min(startIndex + 1, text.Length);
            location = new(
                startIndex,
                endIndex - startIndex,
                int.Max(start.Line + 1, 1),
                int.Max(start.Column + 1, 1));
            return true;
        }

        private static bool CreateEmptyLocation(out TextLocation location)
        {
            location = default;
            return false;
        }

        private static string CombinePath(string path, string key) =>
            path.Length is 0 ? key : $"{path}.{key}";
    }
}
