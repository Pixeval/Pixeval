// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ColorDocument.Avalonia;
using Markdown.Avalonia;
using Markdown.Avalonia.Parsers;
using Markdown.Avalonia.Plugins;

namespace Pixeval.Controls;

public sealed class NovelMarkdownBox : MarkdownBox
{
    public NovelMarkdownBox()
    {
        var plugin = Plugins.Plugins.OfType<PixevalMarkdownPlugin>().Single();
        Plugins.Plugins[Plugins.Plugins.IndexOf(plugin)] = new NovelMarkdownPlugin();
    }

    private sealed class NovelMarkdownPlugin : IMdAvPlugin
    {
        public void Setup(SetupInfo info)
        {
            // 同名解析器采用首个覆盖项，先禁用小说缩进代码块，再注册通用解析规则。
            info.Register(new DisabledIndentedCodeBlockOverride());
            new PixevalMarkdownPlugin().Setup(info);
        }
    }

    private sealed class DisabledIndentedCodeBlockOverride() : BlockOverride2("CodeBlocksWithoutLangEvaluator")
    {
        public override IEnumerable<DocumentElement>? Convert2(
            string text,
            Match firstMatch,
            ParseStatus status,
            IMarkdownEngine2 engine,
            out int parseTextBegin,
            out int parseTextEnd)
        {
            parseTextBegin = parseTextEnd = -1;
            return null;
        }
    }
}
