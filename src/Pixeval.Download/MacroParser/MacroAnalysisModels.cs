// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Pixeval.Download.MacroParser.Ast;
using Pixeval.Download.MacroParser.Bound;

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
    Formatter,
    Separator
}

public sealed record MacroHighlightSpan(MacroTextSpan Span, MacroHighlightKind Kind, int NestingDepth);

public enum MacroDiagnosticKind
{
    UnexpectedToken,
    ExpectedLeftBraceAfterAt,
    ExpectedMacroName,
    MissingRightBrace,
    MissingConditionalSeparator,
    UnknownMacroName,
    NonParameterizedMacroBearingParameter,
    ConditionalBranchesMissing,
    InvalidFormatter,
    MacroShouldBeContained,
    MacroShouldBeInLastSegment
}

public sealed record MacroDiagnostic(
    MacroDiagnosticKind Kind,
    MacroTextSpan Span,
    params IReadOnlyList<object?> Arguments);

public sealed record MacroParseResult(
    Sequence? Root,
    IReadOnlyList<MacroHighlightSpan> Highlights,
    IReadOnlyList<MacroDiagnostic> Diagnostics)
{
    public bool IsSuccess => Diagnostics.Count is 0;

    public static MacroParseResult Empty { get; } = new(null, [], []);
}

public sealed record MacroBindingResult<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TContext>(
    BoundSequence<TContext>? Root,
    IReadOnlyList<MacroDiagnostic> Diagnostics)
{
    public bool IsSuccess => Diagnostics.Count is 0;
}
