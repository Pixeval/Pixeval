// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Pixeval.Download.MacroParser;

namespace Pixeval.Download;

public class MetaPathAnalyzer<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TContext>(IReadOnlyList<IMacro> macroProvider)
{
    public IReadOnlyList<IMacro> MacroProvider { get; } = macroProvider;

    public MacroParseResult Analyze(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return MacroParseResult.Empty;

        var result = new MacroSyntaxParser(text).Parse();
        if (result.Diagnostics.Count > 0 || result.Root is not { } root)
            return result;

        var bindingResult = new MacroBinder<TContext>(MacroProvider).Bind(root);
        return bindingResult.Diagnostics.Count is 0
            ? result
            : result with { Diagnostics = bindingResult.Diagnostics };
    }
}
