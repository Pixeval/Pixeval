// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia.Controls;
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
        info.RegisterTop(new PixevalHtmlBlockParser(CreateReplaceManager(info)));
        info.Register(new PixevalHtmlInlineParser(CreateReplaceManager(info)));
    }

    private ReplaceManager CreateReplaceManager(SetupInfo info)
    {
        var manager = new ReplaceManager(_syntaxHighlight!, info);
        manager.Register(new AnchorDivParser());
        manager.Register(new ColorTagParser());
        manager.Register(new RubyTagParser());
        return manager;
    }

    private sealed partial class PixevalHtmlBlockParser(ReplaceManager replaceManager)
        : BlockParser(HeadTagPattern, nameof(PixevalHtmlBlockParser))
    {
        [GeneratedRegex(@"^<[\t ]*(?'tagname'[a-z][a-z0-9]*)(?'attributes'[ \t][^>]*|/)?>", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace, "zh-CN")]
        private static partial Regex HeadTagPattern { get; }

        public override IEnumerable<Control> Convert(
            string text,
            Match firstMatch,
            ParseStatus status,
            IMarkdownEngine engine,
            out int parseTextBegin,
            out int parseTextEnd)
        {
            parseTextBegin = firstMatch.Index;
            parseTextEnd = PixevalHtmlUtils.SearchTagRangeContinuous(text, firstMatch);
            replaceManager.Engine = engine;

            return replaceManager.Parse(text[parseTextBegin..parseTextEnd]);
        }
    }

    private sealed class PixevalHtmlInlineParser(ReplaceManager replaceManager) : InlineParser(
        PixevalHtmlUtils.CreateTagstartPattern(replaceManager.InlineTags), nameof(PixevalHtmlInlineParser))
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
            replaceManager.Engine = engine;

            return replaceManager.ParseInline(text[parseTextBegin..parseTextEnd]);
        }
    }
}
