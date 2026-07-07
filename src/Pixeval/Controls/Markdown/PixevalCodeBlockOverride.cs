// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using ColorDocument.Avalonia;
using ColorTextBlock.Avalonia;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using Markdown.Avalonia;
using Markdown.Avalonia.Parsers;
using Markdown.Avalonia.Plugins;

namespace Pixeval.Controls;

internal sealed class PixevalFencedCodeBlockOverride() : BlockOverride2("CodeBlocksWithLangEvaluator")
{
    public override IEnumerable<DocumentElement>? Convert2(
        string text,
        Match firstMatch,
        ParseStatus status,
        IMarkdownEngine2 engine,
        out int parseTextBegin,
        out int parseTextEnd)
    {
        var closeTagPattern = new Regex($"\n[ ]*{firstMatch.Groups[1].Value}[ ]*\n");
        var closeTagMatch = closeTagPattern.Match(text, firstMatch.Index + firstMatch.Length);

        int codeEndIndex;
        if (closeTagMatch.Success)
        {
            codeEndIndex = closeTagMatch.Index;
            parseTextEnd = closeTagMatch.Index + closeTagMatch.Length;
        }
        else
        {
            parseTextBegin = parseTextEnd = -1;
            return null;
        }

        parseTextBegin = firstMatch.Index;

        var code = text[(firstMatch.Index + firstMatch.Length)..codeEndIndex];
        var lang = firstMatch.Groups[2].Value.Trim();
        return [new PixevalCodeBlockElement(lang, code)];
    }
}

internal sealed partial class PixevalIndentedCodeBlockOverride() : BlockOverride2("CodeBlocksWithoutLangEvaluator")
{
    private static readonly Regex NewlinesLeadingTrailing = new(@"^\n+|\n+\z", RegexOptions.Compiled);

    public override IEnumerable<DocumentElement>? Convert2(
        string text,
        Match firstMatch,
        ParseStatus status,
        IMarkdownEngine2 engine,
        out int parseTextBegin,
        out int parseTextEnd)
    {
        parseTextBegin = firstMatch.Index;
        parseTextEnd = firstMatch.Index + firstMatch.Length;

        var code = string.Join(
            "\n",
            firstMatch.Groups[1].Value.Split('\n').Select(static line => DetentLineBestEffort(line, 4)));

        return [new PixevalCodeBlockElement(null, NewlinesLeadingTrailing.Replace(code, ""))];
    }

    private static string DetentLineBestEffort(string line, int indentCount)
    {
        var realIndex = 0;
        var viewIndex = 0;

        while (viewIndex < indentCount && realIndex < line.Length)
        {
            var c = line[realIndex];
            if (c == ' ')
            {
                realIndex += 1;
                viewIndex += 1;
            }
            else if (c == '\t')
            {
                realIndex += 1;
                viewIndex = ((viewIndex >> 2) + 1) << 2;
            }
            else
            {
                break;
            }
        }

        return line[realIndex..];
    }
}

internal sealed class PixevalCodeBlockElement : DocumentElement
{
    internal const string RootClass = "PixevalCodeBlockRoot";
    internal const string ToolbarClass = "PixevalCodeBlockToolbar";
    private const string CodeBlockClass = "CodeBlock";

    private readonly string? _lang;
    private readonly string _code;
    private readonly Lazy<Border> _control;
    private CTextBlock? _textBlock;

    public PixevalCodeBlockElement(string? lang, string code)
    {
        _lang = string.IsNullOrWhiteSpace(lang) ? null : lang;
        _code = code;
        _control = new Lazy<Border>(CreateControl);
    }

    public override Control Control => _control.Value;

    public override IEnumerable<DocumentElement> Children => [];

    public override void Select(Point from, Point to)
    {
        if (_textBlock is not { } textBlock
            || textBlock.TranslatePoint(default, Control) is not { } textOffset)
        {
            return;
        }

        var fromPoint = textBlock.CalcuatePointerFrom(from.X - textOffset.X, from.Y - textOffset.Y);
        var toPoint = textBlock.CalcuatePointerFrom(to.X - textOffset.X, to.Y - textOffset.Y);
        textBlock.Select(fromPoint, toPoint);
    }

    public override void UnSelect()
    {
        _textBlock?.ClearSelection();
    }

    public override void ConstructSelectedText(StringBuilder stringBuilder)
    {
        if (_textBlock?.GetSelectedText() is { Length: > 0 } text)
            stringBuilder.Append(text);
    }

    private Border CreateControl()
    {
        _textBlock = new CTextBlock
        {
            TextWrapping = TextWrapping.NoWrap,
            Cursor = new Cursor(StandardCursorType.Ibeam)
        };
        _textBlock.Classes.Add(CodeBlockClass);
        _textBlock.Content.Add(new CRun { Text = _code });

        var scrollViewer = new ScrollViewer
        {
            Content = _textBlock,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled
        };
        scrollViewer.Classes.Add(CodeBlockClass);

        var root = new Grid();
        root.Classes.Add(RootClass);
        root.Children.Add(scrollViewer);
        root.Children.Add(CreateToolbar());

        var border = new Border
        {
            Child = root
        };
        border.Classes.Add(CodeBlockClass);

        return border;
    }

    private StackPanel CreateToolbar()
    {
        var toolbar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top
        };
        toolbar.SetValue(Panel.ZIndexProperty, 1);
        toolbar.Classes.Add(ToolbarClass);

        if (_lang is { } lang)
        {
            var langLabel = new Label
            {
                Content = lang,
                VerticalAlignment = VerticalAlignment.Center
            };
            langLabel.Classes.Add("LangInfo");
            toolbar.Children.Add(langLabel);
        }

        var copyButton = new Button
        {
            Content = new SymbolIcon
            {
                Symbol = Symbol.Clipboard,
                FontSize = 14
            },
            VerticalAlignment = VerticalAlignment.Center
        };
        copyButton.Classes.Add("CopyButton");
        ToolTip.SetTip(copyButton, "复制代码");
        copyButton.Click += async (_, _) =>
        {
            if (TopLevel.GetTopLevel(copyButton)?.Clipboard is { } clipboard)
            {
                var item = new DataTransferItem();
                item.Set(DataFormat.Text, _code);
                var data = new DataTransfer();
                data.Add(item);

                await clipboard.SetDataAsync(data);
            }
        };
        toolbar.Children.Add(copyButton);

        return toolbar;
    }
}
