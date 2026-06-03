// Copyright (c) Pixeval.Utilities.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;

namespace Pixeval.Download.MacroParser;

public class MacroParseException(MacroParseException.ErrorType type, MacroTextSpan? span = null, params IReadOnlyList<object?> parameter)
    : Exception(type.ToString())
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
        MacroShouldBeContained,
        MacroShouldBeInLastSegment,
        ResultIsEmpty
    }

    public static MacroParseException FromDiagnostic(MacroDiagnostic diagnostic)
    {
        var type = diagnostic.Kind switch
        {
            MacroDiagnosticKind.UnknownMacroName => ErrorType.UnknownMacroName,
            MacroDiagnosticKind.NonParameterizedMacroBearingParameter => ErrorType.NonParameterizedMacroBearingParameter,
            MacroDiagnosticKind.ConditionalBranchesMissing => ErrorType.ConditionalBranchesMissing,
            MacroDiagnosticKind.MacroShouldBeContained => ErrorType.MacroShouldBeContained,
            MacroDiagnosticKind.MacroShouldBeInLastSegment => ErrorType.MacroShouldBeInLastSegment,
            _ => ErrorType.UnexpectedToken
        };
        return new(type, diagnostic.Span, diagnostic.Arguments);
    }

    public static MacroParseException ResultIsEmpty() => new(ErrorType.ResultIsEmpty);
}
