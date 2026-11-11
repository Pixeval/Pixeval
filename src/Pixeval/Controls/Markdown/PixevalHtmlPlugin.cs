// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ColorDocument.Avalonia;
using ColorTextBlock.Avalonia;
using Markdown.Avalonia;
using Markdown.Avalonia.Html.Core;
using Markdown.Avalonia.Parsers;
using Markdown.Avalonia.Plugins;
using Markdown.Avalonia.SyntaxHigh;

namespace Pixeval.Controls;

internal sealed partial class PixevalHtmlPlugin : IMdAvPluginRequestAnother
{
    private SyntaxHighlight? _syntaxHighlight;

    public IEnumerable<Type> DependsOn => [typeof(SyntaxHighlight)];

    public void Inject(IEnumerable<IMdAvPlugin> plugin)
    {
        _syntaxHighlight = plugin.OfType<SyntaxHighlight>().FirstOrDefault();
    }

    public void Setup(SetupInfo info)
    {
        if (_syntaxHighlight is null)
            throw new InvalidOperationException("SyntaxHighlight required.");

        info.EnableNoteBlock = false;
        info.RegisterTop(new PixevalHtmlBlockParser(CreateSyntax(info, false)));
        info.Register(new PixevalHtmlInlineParser(CreateSyntax(info, true)));
    }

    private ReplaceManagerSyntax CreateSyntax(SetupInfo info, bool inlineMode)
    {
        var syntax = new ReplaceManagerSyntax(_syntaxHighlight!, info, inlineMode);
        syntax.Register(new AnchorDivParser());
        syntax.Register(new ColorTagParser());
        syntax.Register(new RubyTagParser());
        return syntax;
    }

    private sealed partial class PixevalHtmlBlockParser(ReplaceManagerSyntax syntax)
        : BlockParser2(HeadTagPattern, nameof(PixevalHtmlBlockParser))
    {
        [GeneratedRegex(@"^<[\t ]*(?'tagname'[a-z][a-z0-9]*)(?'attributes'[ \t][^>]*|/)?>", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace, "zh-CN")]
        private static partial Regex HeadTagPattern { get; }

        public override IEnumerable<DocumentElement> Convert2(
            string text,
            Match firstMatch,
            ParseStatus status,
            IMarkdownEngine2 engine,
            out int parseTextBegin,
            out int parseTextEnd)
        {
            parseTextBegin = firstMatch.Index;
            parseTextEnd = PixevalHtmlUtils.SearchTagRangeContinuous(text, firstMatch);
            return syntax.Create(engine).Parse(text[parseTextBegin..parseTextEnd]);
        }
    }

    private sealed class PixevalHtmlInlineParser(ReplaceManagerSyntax syntax) : InlineParser(
        PixevalHtmlUtils.CreateTagstartPattern(syntax.InlineTags), nameof(PixevalHtmlInlineParser))
    {
        public override IEnumerable<CInline> Convert(
            string text,
            Match firstMatch,
            IMarkdownEngine engine,
            out int parseTextBegin,
            out int parseTextEnd)
        {
            parseTextBegin = firstMatch.Index;
            parseTextEnd = PixevalHtmlUtils.SearchTagRange(text, firstMatch);
            return syntax.Create(engine.Upgrade()).ParseInline(text[parseTextBegin..parseTextEnd]);
        }
    }
}
