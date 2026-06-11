// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Pixeval.Download;
using Pixeval.Download.MacroParser;

namespace Pixeval.Models.Download;

public static partial class DownloadPathMacroParser
{
    private static MetaPathParser<ParserContext> Parser => field ??= new(MacroProvider);

    private static MetaPathAnalyzer<ParserContext> Analyzer => field ??= new(MacroProvider);

    public static MacroParseResult Analyze(string text) => Analyzer.Analyze(text);

    public static string Reduce(string raw, ParserContext context) => Parser.Reduce(raw, context);
}
