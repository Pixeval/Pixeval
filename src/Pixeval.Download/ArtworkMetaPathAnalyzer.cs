// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Misaki;
using Pixeval.Download.MacroParser;

namespace Pixeval.Download;

public static class ArtworkMetaPathAnalyzer
{
    public static MacroParseResult Analyze(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return MacroParseResult.Empty;

        var result = new MacroSyntaxParser(text).Parse();
        if (result.Diagnostics.Count > 0 || result.Root is not { } root)
            return result;

        var bindingResult = new MacroBinder<IArtworkInfo>(ArtworkMetaPathParser.Instance.MacroProvider).Bind(root);
        return bindingResult.Diagnostics.Count is 0
            ? result
            : result with { Diagnostics = bindingResult.Diagnostics };
    }
}
