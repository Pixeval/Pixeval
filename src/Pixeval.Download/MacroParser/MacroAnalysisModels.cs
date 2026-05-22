// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;

namespace Pixeval.Download.MacroParser;

public readonly record struct MacroTextSpan(int Start, int Length)
{
    public int End => Start + Math.Max(Length, 0);

    public static MacroTextSpan FromBounds(int start, int end)
    {
        return new(start, Math.Max(0, end - start));
    }
}

public static class MacroTextSpanExtensions
{
    public static MacroTextSpan ToTextSpan(this Range range)
    {
        return MacroTextSpan.FromBounds(range.Start.Value, range.End.Value);
    }
}

public enum MacroHighlightKind
{
    Delimiter,
    Name,
    Separator
}

public sealed record MacroHighlightSpan(MacroTextSpan Span, MacroHighlightKind Kind, int NestingDepth);

public enum MacroDiagnosticKind
{
    UnexpectedToken,
    UnknownMacroName,
    NonParameterizedMacroBearingParameter,
    ConditionalBranchesMissing,
    MacroShouldBeContained,
    MacroShouldBeInLastSegment
}

public sealed record MacroDiagnostic(
    MacroDiagnosticKind Kind,
    MacroTextSpan Span,
    string? PrimaryParameter = null,
    string? SecondaryParameter = null);

public sealed record MacroAnalysisResult(
    IReadOnlyList<MacroHighlightSpan> Highlights,
    IReadOnlyList<MacroDiagnostic> Diagnostics)
{
    public static MacroAnalysisResult Empty { get; } = new([], []);
}