// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using Misaki;
using Pixeval.Download.MacroParser;
using Pixeval.Download.Macros;

namespace Pixeval.Download;

public class ArtworkMetaPathParser : IMetaPathParser<IArtworkInfo>
{
    private ArtworkMetaPathParser()
    {
    }

    public static IMetaPathParser<IArtworkInfo> Instance { get; } = new ArtworkMetaPathParser();

    public IReadOnlyList<IMacro> MacroProvider { get; } = MetaPathMacroAttributeHelper.GetIArtworkInfoInstances();

    public string Reduce(string raw, IArtworkInfo context)
    {
        var parseResult = new MacroSyntaxParser(raw).Parse();
        if (parseResult.Diagnostics is [{ } diagnostic, ..])
            throw MacroParseException.FromDiagnostic(diagnostic);

        var bindingResult = new MacroBinder<IArtworkInfo>(MacroProvider).Bind(parseResult.Root);
        if (bindingResult.Diagnostics is [{ } bindingDiagnostic, ..])
            throw MacroParseException.FromDiagnostic(bindingDiagnostic);

        if (bindingResult.Root is { } root)
        {
            var reduced = root.Evaluate(context);
            if (!string.IsNullOrWhiteSpace(reduced))
                return reduced;
        }

        throw MacroParseException.ResultIsEmpty();
    }
}
