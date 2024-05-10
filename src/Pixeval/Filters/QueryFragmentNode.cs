using System.Diagnostics;

namespace Pixeval.Filters;

/// <summary>
/// Fragments of data that a query string carries, could be a data or numeric value, or some labels like #, @, : or standalone characters like a or c.
/// </summary>
public interface IQueryNode
{
    public interface INullableNode : IQueryNode
    {
        public bool IsNotEmpty();
    }

    [DebuggerDisplay("a")]
    public record A : IQueryNode;

    [DebuggerDisplay("c")]
    public record C : IQueryNode;

    [DebuggerDisplay("e")]
    public record E : IQueryNode;

    [DebuggerDisplay("l")]
    public record L : IQueryNode;

    [DebuggerDisplay("n")]
    public record N : IQueryNode;

    [DebuggerDisplay("s")]
    public record S : IQueryNode;

    [DebuggerDisplay("{Value} ({IsPrecise})")]
    public record Data(string Value, bool IsPrecise) : INullableNode
    {
        public Data(string Value) : this(Value.EndsWith('$') ? Value[..^1] : Value, Value.EndsWith('$'))
        {

        }

        public bool IsNotEmpty()
        {
            return Value.Length > 0;
        }
    }

    [DebuggerDisplay("Value")]
    public record Numeric(long Value) : INullableNode
    {
        public bool IsNotEmpty()
        {
            return Value >= 0;
        }
    }

    [DebuggerDisplay("-")]
    public record Dash : IQueryNode;

    [DebuggerDisplay("#")]
    public record Hashtag : IQueryNode;

    [DebuggerDisplay("@")]
    public record Arobase : IQueryNode;

    [DebuggerDisplay("!")]
    public record Not : IQueryNode;

    [DebuggerDisplay("or")]
    public record Or : IQueryNode;

    [DebuggerDisplay("and")]
    public record And : IQueryNode;

    [DebuggerDisplay(".")]
    public record Dot : IQueryNode;

    [DebuggerDisplay(":")]
    public record Colon : IQueryNode;

    [DebuggerDisplay(",")]
    public record Comma : IQueryNode;

    [DebuggerDisplay("(")]
    public record LeftParen : IQueryNode;

    [DebuggerDisplay(")")]
    public record RightParen : IQueryNode;

    [DebuggerDisplay("[")]
    public record LeftBracket : IQueryNode;

    [DebuggerDisplay("]")]
    public record RightBracket : IQueryNode;
}
