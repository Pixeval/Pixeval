// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Misaki;
using Pixeval.Download.MacroParser;
using Pixeval.Download.MacroParser.Ast;
using Pixeval.Download.Macros;

namespace Pixeval.Download;

public static class ArtworkMetaPathAnalyzer
{
    public static MacroAnalysisResult Analyze(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return MacroAnalysisResult.Empty;

        var highlights = CollectHighlights(text);

        try
        {
            var parser = new MacroParser<string>();
            parser.SetupParsingEnvironment(new Lexer(text));
            if (parser.Parse() is not { } root)
                return new MacroAnalysisResult(highlights, []);

            var diagnostic = Validate(root, ArtworkMetaPathParser.Instance);
            return diagnostic is null
                ? new MacroAnalysisResult(highlights, [])
                : new MacroAnalysisResult(highlights, [diagnostic]);
        }
        catch (MacroParseException exception)
        {
            return new MacroAnalysisResult(
                highlights,
                [new MacroDiagnostic(
                    MacroDiagnosticKind.UnexpectedToken,
                    exception.Span ?? new MacroTextSpan(0, 1),
                    exception.Parameter)]);
        }
    }

    private static MacroDiagnostic? Validate(
        IMetaPathNode<string> tree,
        IMetaPathParser<IArtworkInfo> macroProvider)
    {
        return Validate(tree, ImmutableDictionary<string, bool>.Empty, [], macroProvider);
    }

    private static MacroDiagnostic? Validate(
        IMetaPathNode<string>? tree,
        ImmutableDictionary<string, bool> context,
        List<(string Name, MacroTextSpan Span, ImmutableDictionary<string, bool> Context)> lastSegmentContexts,
        IMetaPathParser<IArtworkInfo> macroProvider)
    {
        return tree switch
        {
            null => null,
            PlainText<string>(var text, _) when text.Contains('\\') => ValidateLastSegmentContext(lastSegmentContexts, context),
            PlainText<string> => null,
            ConditionalMacroBranches<string>(var whenTrue, var whenFalse) =>
                Validate(whenTrue, context, lastSegmentContexts, macroProvider)
                ?? Validate(whenFalse, context, lastSegmentContexts, macroProvider),
            Macro<string>({ Text: var name, Position: var position }, var branches) =>
                macroProvider.MacroProvider.TryResolve(name) switch
                {
                    Unknown => new MacroDiagnostic(MacroDiagnosticKind.UnknownMacroName, position.ToTextSpan(), name),
                    ITransducer when branches is not null => new MacroDiagnostic(MacroDiagnosticKind.NonParameterizedMacroBearingParameter, position.ToTextSpan(), name),
                    PicSetIndexMacro macro when !(context.TryGetValue(IsPicSetMacro.NameConst, out var value) && value)
                        => new MacroDiagnostic(MacroDiagnosticKind.MacroShouldBeContained, position.ToTextSpan(), macro.Name, IsPicSetMacro.NameConst),
                    ILastSegment lastSegment => AddLastSegmentContext(lastSegmentContexts, context, lastSegment, position.ToTextSpan()),
                    ITransducer => null,
                    IPredicate when branches is null => new MacroDiagnostic(MacroDiagnosticKind.ConditionalBranchesMissing, position.ToTextSpan(), name),
                    IPredicate predicate => Validate(predicate, branches, context, lastSegmentContexts, macroProvider),
                    _ => new MacroDiagnostic(MacroDiagnosticKind.UnknownMacroName, position.ToTextSpan(), name)
                },
            Sequence<string>(var first, var remains) =>
                Validate(first, context, lastSegmentContexts, macroProvider)
                ?? Validate(remains, context, lastSegmentContexts, macroProvider),
            _ => throw new ArgumentOutOfRangeException(nameof(tree))
        };
    }

    private static MacroDiagnostic? Validate(
        IPredicate predicate,
        ConditionalMacroBranches<string> branches,
        ImmutableDictionary<string, bool> context,
        List<(string Name, MacroTextSpan Span, ImmutableDictionary<string, bool> Context)> lastSegmentContexts,
        IMetaPathParser<IArtworkInfo> macroProvider)
    {
        return Validate(branches.WhenTrue, context.SetItem(predicate.Name, true), lastSegmentContexts, macroProvider)
            ?? Validate(branches.WhenFalse, context.SetItem(predicate.Name, false), lastSegmentContexts, macroProvider);
    }

    private static MacroDiagnostic? ValidateLastSegmentContext(
        List<(string Name, MacroTextSpan Span, ImmutableDictionary<string, bool> Context)> lastSegmentContexts,
        ImmutableDictionary<string, bool> context)
    {
        foreach (var (name, span, expectedContext) in lastSegmentContexts)
        {
            if (!IsContextCompatible(expectedContext, context))
                continue;

            return new MacroDiagnostic(MacroDiagnosticKind.MacroShouldBeInLastSegment, span, name);
        }

        return null;
    }

    private static MacroDiagnostic? AddLastSegmentContext(
        List<(string Name, MacroTextSpan Span, ImmutableDictionary<string, bool> Context)> lastSegmentContexts,
        ImmutableDictionary<string, bool> context,
        ILastSegment lastSegment,
        MacroTextSpan span)
    {
        lastSegmentContexts.Add((lastSegment.Name, span, context));
        return null;
    }

    private static bool IsContextCompatible(
        ImmutableDictionary<string, bool> expectedContext,
        ImmutableDictionary<string, bool> actualContext)
    {
        foreach (var pair in expectedContext)
        {
            if (actualContext.TryGetValue(pair.Key, out var actual) && actual != pair.Value)
                return false;
        }

        return true;
    }

    private static IReadOnlyList<MacroHighlightSpan> CollectHighlights(string text)
    {
        var highlights = new List<MacroHighlightSpan>();
        ScanSequence(text, 0, 0, false, false, highlights, out _);
        return highlights;
    }

    private static void ScanSequence(
        string text,
        int start,
        int nestingDepth,
        bool stopAtRightBrace,
        bool stopAtColon,
        ICollection<MacroHighlightSpan> highlights,
        out int nextIndex)
    {
        var index = start;
        while (index < text.Length)
        {
            if (stopAtRightBrace && text[index] == '}')
                break;

            if (stopAtColon && text[index] == ':')
                break;

            if (index + 1 < text.Length && text[index] == '@' && text[index + 1] == '{')
            {
                var scannedIndex = ScanMacro(text, index, nestingDepth, highlights);
                index = scannedIndex > index ? scannedIndex : index + 1;
                continue;
            }

            index++;
        }

        nextIndex = index;
    }

    private static int ScanMacro(
        string text,
        int start,
        int nestingDepth,
        ICollection<MacroHighlightSpan> highlights)
    {
        var index = start;
        highlights.Add(new MacroHighlightSpan(new MacroTextSpan(index, 2), MacroHighlightKind.Delimiter, nestingDepth));
        index += 2;

        var nameStart = index;
        while (index < text.Length && IsMacroNameCharacter(text[index]))
            index++;

        if (index > nameStart)
            highlights.Add(new MacroHighlightSpan(new MacroTextSpan(nameStart, index - nameStart), MacroHighlightKind.Name, nestingDepth));

        if (index >= text.Length)
            return index;

        if (text[index] == '?')
        {
            highlights.Add(new MacroHighlightSpan(new MacroTextSpan(index, 1), MacroHighlightKind.Separator, nestingDepth));
            index++;
            ScanSequence(text, index, nestingDepth + 1, true, true, highlights, out index);
            if (index < text.Length && text[index] == ':')
            {
                highlights.Add(new MacroHighlightSpan(new MacroTextSpan(index, 1), MacroHighlightKind.Separator, nestingDepth));
                index++;
                ScanSequence(text, index, nestingDepth + 1, true, false, highlights, out index);
            }
        }

        if (index < text.Length && text[index] == '}')
        {
            highlights.Add(new MacroHighlightSpan(new MacroTextSpan(index, 1), MacroHighlightKind.Delimiter, nestingDepth));
            return index + 1;
        }

        return index;
    }

    private static bool IsMacroNameCharacter(char character)
    {
        return char.IsAsciiLetterOrDigit(character) || character == '_';
    }
}
