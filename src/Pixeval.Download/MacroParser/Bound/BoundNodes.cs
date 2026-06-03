// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Text;

namespace Pixeval.Download.MacroParser.Bound;

public interface IBoundMetaPathNode<in TContext>
{
    string Evaluate(TContext context);
}

public abstract record BoundSingleNode<TContext> : IBoundMetaPathNode<TContext>
{
    public abstract string Evaluate(TContext context);
}

public record BoundSequence<TContext>(BoundSingleNode<TContext> First, BoundSequence<TContext>? Remains)
    : IBoundMetaPathNode<TContext>
{
    public string Evaluate(TContext context)
    {
        var builder = new StringBuilder();
        for (var current = this; current is not null; current = current.Remains)
            _ = builder.Append(current.First.Evaluate(context));

        return builder.ToString();
    }
}

public record BoundPlainText<TContext>(string Text) : BoundSingleNode<TContext>
{
    public override string Evaluate(TContext context) => Text;
}

public record BoundTransducer<TContext>(ITransducer<TContext> Transducer) : BoundSingleNode<TContext>
{
    public override string Evaluate(TContext context) => Transducer.Substitute(context);
}

public record BoundPredicate<TContext>(
    IPredicate<TContext> Predicate,
    BoundSequence<TContext>? WhenTrue,
    BoundSequence<TContext>? WhenFalse) : BoundSingleNode<TContext>
{
    public override string Evaluate(TContext context) =>
        Predicate.Match(context)
            ? WhenTrue?.Evaluate(context) ?? ""
            : WhenFalse?.Evaluate(context) ?? "";
}
