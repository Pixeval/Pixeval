// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Pixeval.Download.MacroParser.Ast;
using Pixeval.Download.MacroParser.Bound;

namespace Pixeval.Download.MacroParser;

public sealed class MacroBinder<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TContext>(IReadOnlyList<IMacro> macroProvider)
{
    public MacroBindingResult<TContext> Bind(Sequence? root)
    {
        var diagnostics = new List<MacroDiagnostic>();
        var lastSegmentContexts = new List<LastSegmentContext>();
        var boundRoot = Bind(root, [], lastSegmentContexts, diagnostics);
        return new(boundRoot, diagnostics);
    }

    private BoundSequence<TContext>? Bind(
        Sequence? sequence,
        ImmutableDictionary<string, bool> context,
        List<LastSegmentContext> lastSegmentContexts,
        List<MacroDiagnostic> diagnostics)
    {
        if (sequence is null)
            return null;

        var nodes = new List<BoundSingleNode<TContext>>();
        for (var current = sequence; current is not null; current = current.Remains)
        {
            if (Bind(current.First, context, lastSegmentContexts, diagnostics) is { } bound)
                nodes.Add(bound);
        }

        return CreateSequence(nodes);
    }

    private BoundSingleNode<TContext>? Bind(
        SingleNode node,
        ImmutableDictionary<string, bool> context,
        List<LastSegmentContext> lastSegmentContexts,
        List<MacroDiagnostic> diagnostics)
    {
        return node switch
        {
            PlainText(var text, _) => BindPlainText(text, context, lastSegmentContexts, diagnostics),
            Macro({ Text: var name, Position: var position }, var formatter, var branches) =>
                BindMacro(name, position, formatter, branches, context, lastSegmentContexts, diagnostics),
            _ => throw new ArgumentOutOfRangeException(nameof(node))
        };
    }

    private static BoundSingleNode<TContext> BindPlainText(
        string text,
        ImmutableDictionary<string, bool> context,
        List<LastSegmentContext> lastSegmentContexts,
        List<MacroDiagnostic> diagnostics)
    {
        if (text.Contains('\\'))
            ValidateLastSegmentContext(lastSegmentContexts, context, diagnostics);

        return new BoundPlainText<TContext>(text);
    }

    private BoundSingleNode<TContext>? BindMacro(
        string name,
        Range position,
        PlainText? formatter,
        ConditionalMacroBranches? branches,
        ImmutableDictionary<string, bool> context,
        List<LastSegmentContext> lastSegmentContexts,
        List<MacroDiagnostic> diagnostics)
    {
        var span = position.ToTextSpan();
        var formatterText = formatter?.Text;
        var formatterSpan = formatter?.Position.ToTextSpan() ?? span;
        var macro = macroProvider.TryResolve(name);
        return macro switch
        {
            Unknown => AddDiagnostic(diagnostics, MacroDiagnosticKind.UnknownMacroName, span, name),
            ITransducer transducer when !transducer.IsFormatterValid(formatterText)
                => AddDiagnostic(diagnostics, MacroDiagnosticKind.InvalidFormatter, formatterSpan, name, formatterText),
            ITransducer when branches is not null => AddDiagnostic(diagnostics, MacroDiagnosticKind.NonParameterizedMacroBearingParameter, span, name),
            IContextRestrictedMacro restricted when !restricted.ContextPredicate(context)
                => AddDiagnostic(diagnostics, MacroDiagnosticKind.MacroContextRestrictionNotSatisfied, span, restricted.Name),
            ITransducer transducer => BindTransducer(transducer, formatterText, context, span, lastSegmentContexts),
            IPredicate when formatter is not null => AddDiagnostic(diagnostics, MacroDiagnosticKind.InvalidFormatter, formatterSpan, name, formatterText),
            IPredicate when branches is null => AddDiagnostic(diagnostics, MacroDiagnosticKind.ConditionalBranchesMissing, span, name),
            IPredicate predicate when branches is not null => BindPredicate(predicate, branches, context, lastSegmentContexts, diagnostics),
            _ => AddDiagnostic(diagnostics, MacroDiagnosticKind.UnknownMacroName, span, name)
        };
    }

    private static BoundSingleNode<TContext> BindTransducer(
        ITransducer transducer,
        string? formatter,
        ImmutableDictionary<string, bool> context,
        MacroTextSpan span,
        List<LastSegmentContext> lastSegmentContexts)
    {
        if (transducer is ILastSegment lastSegment)
            lastSegmentContexts.Add(new(lastSegment.Name, span, context));

        return new BoundTransducer<TContext>(transducer, formatter);
    }

    private BoundSingleNode<TContext> BindPredicate(
        IPredicate predicate,
        ConditionalMacroBranches branches,
        ImmutableDictionary<string, bool> context,
        List<LastSegmentContext> lastSegmentContexts,
        List<MacroDiagnostic> diagnostics)
    {
        var whenTrue = Bind(branches.WhenTrue, context.SetItem(predicate.Name, true), lastSegmentContexts, diagnostics);
        var whenFalse = Bind(branches.WhenFalse, context.SetItem(predicate.Name, false), lastSegmentContexts, diagnostics);
        return new BoundPredicate<TContext>(predicate, whenTrue, whenFalse);
    }

    private static BoundSingleNode<TContext>? AddDiagnostic(
        List<MacroDiagnostic> diagnostics,
        MacroDiagnosticKind kind,
        MacroTextSpan span,
        params IReadOnlyList<object?> arguments)
    {
        if (diagnostics.Count is 0)
            diagnostics.Add(new(kind, span, arguments));

        return null;
    }

    private static BoundSequence<TContext>? CreateSequence(IReadOnlyList<BoundSingleNode<TContext>> nodes)
    {
        BoundSequence<TContext>? sequence = null;
        for (var i = nodes.Count - 1; i >= 0; --i)
            sequence = new(nodes[i], sequence);

        return sequence;
    }

    private static void ValidateLastSegmentContext(
        List<LastSegmentContext> lastSegmentContexts,
        ImmutableDictionary<string, bool> context,
        List<MacroDiagnostic> diagnostics)
    {
        foreach (var (name, span, expectedContext) in lastSegmentContexts)
        {
            if (!IsContextCompatible(expectedContext, context))
                continue;

            if (diagnostics.Count is 0)
                diagnostics.Add(new(MacroDiagnosticKind.MacroShouldBeInLastSegment, span, name));
            return;
        }
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

    private sealed record LastSegmentContext(
        string Name,
        MacroTextSpan Span,
        ImmutableDictionary<string, bool> Context);
}
