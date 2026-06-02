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
    public static MacroParseResult<string> Analyze(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return MacroParseResult<string>.Empty;

        var result = new MacroParser<string>(text).Parse();
        if (result.Diagnostics.Count > 0 || result.Root is not { } root)
            return result;

        var diagnostic = Validate(root, ArtworkMetaPathParser.Instance);
        return diagnostic is null
            ? result
            : result with { Diagnostics = [diagnostic] };
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

}
