// Copyright (c) Pixeval.Utilities.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;

namespace Pixeval.Download.MacroParser;

public class MacroParseException(MacroParseException.ErrorType type, MacroTextSpan? span = null, params IReadOnlyList<object?> parameter)
    : Exception(CreateMessage(type, parameter))
{
    public ErrorType Type { get; } = type;

    public IReadOnlyList<object?> Parameter { get; } = parameter;

    public MacroTextSpan? Span { get; } = span;

    public enum ErrorType
    {
        UnexpectedToken,
        UnknownMacroName,
        NonParameterizedMacroBearingParameter,
        ConditionalBranchesMissing,
        InvalidFormatter,
        MacroShouldBeContained,
        MacroShouldBeInLastSegment,
        ResultIsEmpty,
        ReductionNotCompleted
    }

    public static MacroParseException FromDiagnostic(MacroDiagnostic diagnostic)
    {
        var type = diagnostic.Kind switch
        {
            MacroDiagnosticKind.UnknownMacroName => ErrorType.UnknownMacroName,
            MacroDiagnosticKind.NonParameterizedMacroBearingParameter => ErrorType.NonParameterizedMacroBearingParameter,
            MacroDiagnosticKind.ConditionalBranchesMissing => ErrorType.ConditionalBranchesMissing,
            MacroDiagnosticKind.InvalidFormatter => ErrorType.InvalidFormatter,
            MacroDiagnosticKind.MacroShouldBeContained => ErrorType.MacroShouldBeContained,
            MacroDiagnosticKind.MacroShouldBeInLastSegment => ErrorType.MacroShouldBeInLastSegment,
            _ => ErrorType.UnexpectedToken
        };
        return new(type, diagnostic.Span, diagnostic.Arguments);
    }

    public static MacroParseException ResultIsEmpty() => new(ErrorType.ResultIsEmpty);

    public static MacroParseException ReductionNotCompleted(IReadOnlyList<string> macroNames) =>
        new(ErrorType.ReductionNotCompleted, null, [.. macroNames]);

    private static string CreateMessage(ErrorType type, IReadOnlyList<object?> parameter) =>
        type is ErrorType.ReductionNotCompleted && parameter.Count > 0
            ? $"Macro reduction is not completed. Unevaluated macros: {string.Join(", ", parameter)}"
            : type.ToString();
}
