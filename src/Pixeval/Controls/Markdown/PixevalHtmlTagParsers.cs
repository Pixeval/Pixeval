// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using ColorTextBlock.Avalonia;
using HtmlAgilityPack;
using Markdown.Avalonia.Html.Core;
using Markdown.Avalonia.Html.Core.Parsers;

namespace Pixeval.Controls;

internal static class MarkdownAnchor
{
    public const string ClassName = "MarkdownAnchor";

    public static Border Create(string id)
    {
        var anchor = new Border
        {
            Width = 0,
            Height = 0,
            MinWidth = 0,
            MinHeight = 0,
            IsHitTestVisible = false,
            Tag = id
        };
        anchor.Classes.Add(ClassName);
        return anchor;
    }
}

internal sealed class AnchorDivParser : IBlockTagParser, IInlineTagParser, IHasPriority
{
    public int Priority => 0;

    public IEnumerable<string> SupportTag => ["div"];

    bool ITagParser.TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<StyledElement> generated)
    {
        if (TryReplace(node, manager, out IEnumerable<Control> controls))
        {
            generated = controls;
            return true;
        }

        generated = [];
        return false;
    }

    public bool TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<Control> generated)
    {
        if (GetAnchorId(node) is not { Length: > 0 } id)
        {
            generated = [];
            return false;
        }

        var anchor = MarkdownAnchor.Create(id);
        var children = manager.ParseChildrenAndGroup(node).ToArray();

        if (children.Length is 0)
        {
            generated = [anchor];
            return true;
        }

        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };
        panel.Children.Add(anchor);
        panel.Children.AddRange(children);
        generated = [panel];
        return true;
    }

    public bool TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<CInline> generated)
    {
        if (GetAnchorId(node) is { Length: > 0 } id && node.ChildNodes.Count is 0)
        {
            generated = [new CInlineUIContainer(MarkdownAnchor.Create(id))];
            return true;
        }

        generated = [];
        return false;
    }

    private static string? GetAnchorId(HtmlNode node) =>
        node.Attributes["id"]?.Value ?? node.Attributes["name"]?.Value;
}

internal sealed class ColorTagParser : IInlineTagParser, IHasPriority
{
    public int Priority => 0;

    public IEnumerable<string> SupportTag => ["color"];

    bool ITagParser.TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<StyledElement> generated)
    {
        if (TryReplace(node, manager, out var inlines))
        {
            generated = inlines;
            return true;
        }

        generated = [];
        return false;
    }

    public bool TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<CInline> generated)
    {
        var parsed = manager.ParseChildrenJagging(node).ToArray();
        if (parsed.Any(element => element is not CInline))
        {
            generated = [];
            return false;
        }

        var span = new CSpan(parsed.Cast<CInline>());

        if (TryGetBrush(GetColorValue(node, "value", "color", "foreground", "fg"), out var foreground))
            span.Foreground = foreground;

        if (TryGetBrush(GetColorValue(node, "background", "background-color", "bg"), out var background))
            span.Background = background;

        generated = [span];
        return true;
    }

    private static string? GetColorValue(HtmlNode node, params string[] names)
    {
        foreach (var name in names)
        {
            if (node.Attributes[name]?.Value is { Length: > 0 } value)
                return value;
        }

        if (node.Attributes["style"]?.Value is not { Length: > 0 } style)
            return null;

        foreach (var declaration in style.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var separator = declaration.IndexOf(':');
            if (separator <= 0)
                continue;

            var name = declaration[..separator].Trim();
            if (names.Any(candidate => candidate.Equals(name, StringComparison.OrdinalIgnoreCase)))
                return declaration[(separator + 1)..].Trim();
        }

        return null;
    }

    private static bool TryGetBrush(string? value, out IBrush? brush)
    {
        brush = null;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        try
        {
            brush = value.StartsWith('#')
                ? (IBrush?) new BrushConverter().ConvertFrom(value)
                : (IBrush?) new BrushConverter().ConvertFromString(value);
            return brush is not null;
        }
        catch
        {
            return false;
        }
    }
}

internal sealed class RubyTagParser : IInlineTagParser, IHasPriority
{
    public int Priority => 0;

    public IEnumerable<string> SupportTag => ["ruby"];

    bool ITagParser.TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<StyledElement> generated)
    {
        if (TryReplace(node, manager, out var inlines))
        {
            generated = inlines;
            return true;
        }

        generated = [];
        return false;
    }

    public bool TryReplace(HtmlNode node, ReplaceManager manager, out IEnumerable<CInline> generated)
    {
        var annotation = HtmlEntity.DeEntitize(string.Concat(
            node.Descendants()
                .Where(IsRubyTextNode)
                .Select(child => child.InnerText)));

        if (annotation.Length is 0)
        {
            generated = [];
            return false;
        }

        var body = ExtractBodyText(node);

        if (body.Length is 0)
        {
            generated = [];
            return false;
        }

        generated = [new RubyInline(body, annotation)];
        return true;
    }

    private static string ExtractBodyText(HtmlNode node)
    {
        var builder = new StringBuilder();

        foreach (var child in node.ChildNodes)
        {
            if (IsRubyTextNode(child) || IsRubyFallbackNode(child))
                continue;

            builder.Append(child.InnerText);
        }

        return HtmlEntity.DeEntitize(builder.ToString());
    }

    private static bool IsRubyTextNode(HtmlNode node) =>
        node.Name.Equals("rt", StringComparison.OrdinalIgnoreCase);

    private static bool IsRubyFallbackNode(HtmlNode node) =>
        node.Name.Equals("rp", StringComparison.OrdinalIgnoreCase);
}
