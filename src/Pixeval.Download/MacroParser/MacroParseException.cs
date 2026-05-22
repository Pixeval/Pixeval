// Copyright (c) Pixeval.Utilities.
// Licensed under the GPL v3 License.

using System;

namespace Pixeval.Download.MacroParser;

public class MacroParseException(MacroParseException.ErrorType type, string? parameter = null, MacroTextSpan? span = null)
    : Exception(type.ToString())
{
    public ErrorType Type { get; } = type;

    public string? Parameter { get; } = parameter;

    public MacroTextSpan? Span { get; } = span;

    public enum ErrorType
    {
        UnexpectedToken,
        UnknownMacroName,
        NonParameterizedMacroBearingParameter,
        ParameterizedMacroMissingParameter,
        ResultIsEmpty
    }
}
