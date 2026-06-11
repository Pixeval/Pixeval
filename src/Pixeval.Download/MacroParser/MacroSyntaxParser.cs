// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.IO;
using Pixeval.Download.MacroParser.Ast;

namespace Pixeval.Download.MacroParser;

public sealed class MacroSyntaxParser(string text)
{
    private static readonly char[] _InvalidPathChars = Path.GetInvalidPathChars();

    private readonly List<MacroDiagnostic> _diagnostics = [];
    private readonly List<MacroHighlightSpan> _highlights = [];
    private int _position;

    public MacroParseResult Parse()
    {
        var root = ParseSequence(stopAtColon: false, stopAtRightBrace: false, nestingDepth: 0);
        if (_diagnostics.Count is 0 && !IsAtEnd)
            AddDiagnostic(MacroDiagnosticKind.UnexpectedToken, CurrentSpan());

        return new(root, _highlights, _diagnostics);
    }

    private Sequence? ParseSequence(bool stopAtColon, bool stopAtRightBrace, int nestingDepth)
    {
        var nodes = new List<SingleNode>();
        var plainTextStart = _position;
        while (!IsAtEnd)
        {
            if (stopAtRightBrace && Current is '}')
                break;

            if (stopAtColon && Current is ':')
                break;

            if (Current is '}')
            {
                AddPlainText(nodes, plainTextStart, _position);
                AddDiagnostic(MacroDiagnosticKind.UnexpectedToken, CurrentSpan());
                break;
            }

            if (Current is '@')
            {
                AddPlainText(nodes, plainTextStart, _position);
                var macro = ParseMacro(nestingDepth);
                if (macro is not null)
                    nodes.Add(macro);

                plainTextStart = _position;
                continue;
            }

            if (IsInvalidPathChar(Current))
            {
                AddPlainText(nodes, plainTextStart, _position);
                AddDiagnostic(MacroDiagnosticKind.UnexpectedToken, CurrentSpan());
                ++_position;
                plainTextStart = _position;
                continue;
            }

            ++_position;
        }

        AddPlainText(nodes, plainTextStart, _position);
        return CreateSequence(nodes);
    }

    private Macro? ParseMacro(int nestingDepth)
    {
        var macroStart = _position;
        AddHighlight(macroStart, 1, MacroHighlightKind.Delimiter, nestingDepth);
        ++_position;

        if (IsAtEnd || Current is not '{')
        {
            AddDiagnostic(MacroDiagnosticKind.ExpectedLeftBraceAfterAt, MacroTextSpan.FromBounds(macroStart, Math.Min(macroStart + 1, text.Length)));
            RecoverAfterBrokenMacro(stopAtColon: false, stopAtRightBrace: false);
            return null;
        }

        AddHighlight(_position, 1, MacroHighlightKind.Delimiter, nestingDepth);
        ++_position;

        var nameStart = _position;
        while (!IsAtEnd && IsMacroNameCharacter(Current))
            ++_position;

        if (_position == nameStart)
        {
            AddDiagnostic(MacroDiagnosticKind.ExpectedMacroName, MacroTextSpan.FromBounds(nameStart, Math.Min(nameStart + 1, text.Length)));
            RecoverAfterBrokenMacro(stopAtColon: false, stopAtRightBrace: true);
            return null;
        }

        AddHighlight(nameStart, _position - nameStart, MacroHighlightKind.Name, nestingDepth);
        var macroName = new PlainText(text[nameStart.._position], nameStart.._position);
        var formatter = ParseFormatter(nestingDepth);
        var diagnosticStart = _diagnostics.Count;
        var branches = ParseConditionalBranches(nestingDepth);
        if (_diagnostics.Count > diagnosticStart)
        {
            RecoverAfterBrokenMacro(stopAtColon: false, stopAtRightBrace: true);
            return null;
        }

        if (IsAtEnd || Current is not '}')
        {
            AddDiagnostic(MacroDiagnosticKind.MissingRightBrace, MacroTextSpan.FromBounds(macroStart, Math.Min(macroStart + 2, text.Length)));
            RecoverAfterBrokenMacro(stopAtColon: false, stopAtRightBrace: true);
            return null;
        }

        AddHighlight(_position, 1, MacroHighlightKind.Delimiter, nestingDepth);
        ++_position;
        return new(macroName, formatter, branches);
    }

    private PlainText? ParseFormatter(int nestingDepth)
    {
        if (IsAtEnd || Current is not ':')
            return null;

        AddHighlight(_position, 1, MacroHighlightKind.Separator, nestingDepth);
        ++_position;

        var formatterStart = _position;
        while (!IsAtEnd && Current is not '?' and not '}')
            ++_position;

        if (_position > formatterStart)
            AddHighlight(formatterStart, _position - formatterStart, MacroHighlightKind.Formatter, nestingDepth);

        return new(text[formatterStart.._position], formatterStart.._position);
    }

    private ConditionalMacroBranches? ParseConditionalBranches(int nestingDepth)
    {
        if (IsAtEnd || Current is not '?')
            return null;

        AddHighlight(_position, 1, MacroHighlightKind.Separator, nestingDepth);
        ++_position;
        var diagnosticStart = _diagnostics.Count;
        var whenTrue = ParseSequence(stopAtColon: true, stopAtRightBrace: true, nestingDepth + 1);
        if (_diagnostics.Count > diagnosticStart)
            return null;

        if (IsAtEnd || Current is not ':')
        {
            AddDiagnostic(MacroDiagnosticKind.MissingConditionalSeparator, CurrentSpan());
            return null;
        }

        AddHighlight(_position, 1, MacroHighlightKind.Separator, nestingDepth);
        ++_position;
        var whenFalse = ParseSequence(stopAtColon: false, stopAtRightBrace: true, nestingDepth + 1);
        return new(whenTrue, whenFalse);
    }

    private void RecoverAfterBrokenMacro(bool stopAtColon, bool stopAtRightBrace)
    {
        while (!IsAtEnd)
        {
            if (stopAtRightBrace && Current is '}')
            {
                AddHighlight(_position, 1, MacroHighlightKind.Delimiter, 0);
                ++_position;
                return;
            }

            if (stopAtColon && Current is ':')
                return;

            if (Current is '@' or '}')
                return;

            ++_position;
        }
    }

    private void AddPlainText(ICollection<SingleNode> nodes, int start, int end)
    {
        if (end <= start)
            return;

        nodes.Add(new PlainText(text[start..end], start..end));
    }

    private static Sequence? CreateSequence(IReadOnlyList<SingleNode> nodes)
    {
        Sequence? sequence = null;
        for (var i = nodes.Count - 1; i >= 0; --i)
            sequence = new(nodes[i], sequence);

        return sequence;
    }

    private void AddHighlight(int start, int length, MacroHighlightKind kind, int nestingDepth)
        => _highlights.Add(new(new(start, length), kind, nestingDepth));

    private void AddDiagnostic(MacroDiagnosticKind kind, MacroTextSpan span)
        => _diagnostics.Add(new(kind, span));

    private MacroTextSpan CurrentSpan()
        => IsAtEnd
            ? MacroTextSpan.FromBounds(Math.Max(text.Length - 1, 0), text.Length)
            : new(_position, 1);

    private static bool IsMacroNameCharacter(char character)
        => char.IsAsciiLetterOrDigit(character) || character is '_';

    private static bool IsInvalidPathChar(char character)
        => _InvalidPathChars.AsSpan().IndexOf(character) >= 0;

    private bool IsAtEnd => _position >= text.Length;

    private char Current => text[_position];
}
