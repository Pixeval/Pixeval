// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;
using System.Text.RegularExpressions;
using ColorTextBlock.Avalonia;
using Markdown.Avalonia;
using Markdown.Avalonia.Parsers;

namespace Pixeval.Controls;

internal sealed partial class ExtraEscapedCharacterInlineParser()
    : InlineParser(EscapePattern, nameof(ExtraEscapedCharacterInlineParser))
{
    [GeneratedRegex(@"\\([\\`*{}\[\]()#+\-.!_>~:%<|])", RegexOptions.Compiled)]
    private static partial Regex EscapePattern { get; }

    public override IEnumerable<CInline> Convert(
        string text,
        Match firstMatch,
        IMarkdownEngine engine,
        out int parseTextBegin,
        out int parseTextEnd)
    {
        parseTextBegin = firstMatch.Index;
        parseTextEnd = parseTextBegin + firstMatch.Length;

        return [new CRun { Text = firstMatch.Groups[1].Value }];
    }
}
