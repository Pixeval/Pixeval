// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Pixeval.Download.MacroParser;

namespace Pixeval.Download;

public static class MetaPathParserHelper
{
    public static string InvalidNameCharsInMacro => field ??= "<:>" + InvalidNameCharsInMacroWithToken;

    public static string InvalidNameCharsInMacroWithToken { get; } = @"\/*?""|" + new string(Path.GetInvalidPathChars());

    public static string NormalizePathSegmentInMacro(string path, bool includeToken)
    {
        var invalidChars = includeToken ? InvalidNameCharsInMacroWithToken : InvalidNameCharsInMacro;

        return invalidChars.Aggregate(path, (s, c) => s.Replace(c.ToString(), "")).TrimEnd('.');
    }
}

public class MetaPathParser<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TContext>(IReadOnlyList<IMacro> macroProvider) : IMetaPathParser<TContext>
{
    public IReadOnlyList<IMacro> MacroProvider { get; } = macroProvider;

    public string Reduce(string raw, TContext context)
    {
        var parseResult = new MacroSyntaxParser(raw).Parse();
        if (parseResult.Diagnostics is [{ } diagnostic, ..])
            throw MacroParseException.FromDiagnostic(diagnostic);

        var bindingResult = new MacroBinder<TContext>(MacroProvider).Bind(parseResult.Root);
        if (bindingResult.Diagnostics is [{ } bindingDiagnostic, ..])
            throw MacroParseException.FromDiagnostic(bindingDiagnostic);

        if (bindingResult.Root is { } root)
        {
            var evaluationResult = root.EvaluateDetailed(context);
            if (!evaluationResult.IsCompleted)
                throw MacroParseException.ReductionNotCompleted(evaluationResult.UnevaluatedMacroNames);

            var reduced = evaluationResult.Text;
            if (!string.IsNullOrWhiteSpace(reduced))
                return reduced;
        }

        throw MacroParseException.ResultIsEmpty();
    }
}
