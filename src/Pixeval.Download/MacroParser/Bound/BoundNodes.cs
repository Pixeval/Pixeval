// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Pixeval.Download.MacroParser.Bound;

public sealed record MacroEvaluationResult(string Text, IReadOnlyList<string> UnevaluatedMacroNames)
{
    public bool IsCompleted => UnevaluatedMacroNames.Count is 0;
}

public interface IBoundMetaPathNode<in TContext>
{
    string Evaluate(TContext context);
}

public abstract record BoundSingleNode<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TContext> : IBoundMetaPathNode<TContext>
{
    public string Evaluate(TContext context)
    {
        var builder = new StringBuilder();
        var unevaluatedMacroNames = new List<string>();
        Evaluate(context, builder, unevaluatedMacroNames);
        return builder.ToString();
    }

    internal abstract void Evaluate(TContext context, StringBuilder builder, ICollection<string> unevaluatedMacroNames);
}

public record BoundSequence<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TContext>(BoundSingleNode<TContext> First, BoundSequence<TContext>? Remains)
    : IBoundMetaPathNode<TContext>
{
    public string Evaluate(TContext context)
    {
        return EvaluateDetailed(context).Text;
    }

    public MacroEvaluationResult EvaluateDetailed(TContext context)
    {
        var builder = new StringBuilder();
        var unevaluatedMacroNames = new List<string>();
        for (var current = this; current is not null; current = current.Remains)
            current.First.Evaluate(context, builder, unevaluatedMacroNames);

        return new(builder.ToString(), [.. unevaluatedMacroNames.Distinct(StringComparer.Ordinal)]);
    }
}

public record BoundPlainText<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TContext>(string Text) : BoundSingleNode<TContext>
{
    internal override void Evaluate(TContext context, StringBuilder builder, ICollection<string> unevaluatedMacroNames)
    {
        _ = builder.Append(Text);
    }
}

public record BoundTransducer<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TContext>(
    ITransducer Transducer,
    string? Formatter) : BoundSingleNode<TContext>
{
    internal override void Evaluate(TContext context, StringBuilder builder, ICollection<string> unevaluatedMacroNames)
    {
        if (MacroContextResolver<TContext>.TryResolve(context, Transducer.ContextType, out var macroContext))
        {
            _ = builder.Append(MetaPathParserHelper.NormalizePathSegmentInMacro(Transducer.Substitute(macroContext, Formatter, out var includeToken), includeToken));
            return;
        }

        unevaluatedMacroNames.Add(Transducer.Name);
    }
}

public record BoundPredicate<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TContext>(
    IPredicate Predicate,
    BoundSequence<TContext>? WhenTrue,
    BoundSequence<TContext>? WhenFalse) : BoundSingleNode<TContext>
{
    internal override void Evaluate(TContext context, StringBuilder builder, ICollection<string> unevaluatedMacroNames)
    {
        if (!MacroContextResolver<TContext>.TryResolve(context, Predicate.ContextType, out var macroContext))
        {
            unevaluatedMacroNames.Add(Predicate.Name);
            return;
        }

        var branch = Predicate.Match(macroContext) ? WhenTrue : WhenFalse;
        for (var current = branch; current is not null; current = current.Remains)
            current.First.Evaluate(context, builder, unevaluatedMacroNames);
    }
}

internal static class MacroContextResolver<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TContext>
{
    private static readonly Lazy<IReadOnlyList<PropertyInfo>> _ContextProperties = new(static () =>
    [
        .. typeof(TContext)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(static property => property.GetMethod is not null && property.GetIndexParameters().Length is 0)
    ]);

    public static bool TryResolve(TContext context, Type contextType, out object? value)
    {
        foreach (var property in _ContextProperties.Value)
        {
            if (!contextType.IsAssignableFrom(property.PropertyType))
                continue;

            value = property.GetValue(context);
            return true;
        }

        value = null;
        return false;
    }
}
