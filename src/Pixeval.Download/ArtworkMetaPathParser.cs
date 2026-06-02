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
        var result = new MacroParser<IArtworkInfo>(raw).Parse();
        if (result.Diagnostics is [{ } diagnostic, ..])
            throw CreateException(diagnostic);

        if (result.Root is { } root)
        {
            var reduced = root.Evaluate(MacroProvider, context);
            if (!string.IsNullOrWhiteSpace(reduced))
                return reduced;
        }

        throw new MacroParseException(MacroParseException.ErrorType.ResultIsEmpty);
    }

    private static MacroParseException CreateException(MacroDiagnostic diagnostic)
    {
        var type = diagnostic.Kind switch
        {
            MacroDiagnosticKind.UnknownMacroName => MacroParseException.ErrorType.UnknownMacroName,
            MacroDiagnosticKind.NonParameterizedMacroBearingParameter => MacroParseException.ErrorType.NonParameterizedMacroBearingParameter,
            MacroDiagnosticKind.ConditionalBranchesMissing => MacroParseException.ErrorType.ParameterizedMacroMissingParameter,
            _ => MacroParseException.ErrorType.UnexpectedToken
        };
        return new(type, diagnostic.PrimaryParameter, diagnostic.Span);
    }
}
