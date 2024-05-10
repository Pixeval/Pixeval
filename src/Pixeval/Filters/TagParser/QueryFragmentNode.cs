using System.Diagnostics;

namespace Pixeval.Filters.TagParser;

/// <summary>
/// Fragments of data that a query string carries, could be a data or numeric value, or some labels like #, @, : or standalone characters like a or c.
/// </summary>
public interface IQueryFragmentNode
{
    bool IsNotEmpty()
    {
        return true;
    }

    [DebuggerDisplay("a")]
    public record A : IQueryFragmentNode;

    [DebuggerDisplay("c")]
    public record C : IQueryFragmentNode;

    [DebuggerDisplay("e")]
    public record E : IQueryFragmentNode;

    [DebuggerDisplay("l")]
    public record L : IQueryFragmentNode;

    [DebuggerDisplay("n")]
    public record N : IQueryFragmentNode;

    [DebuggerDisplay("s")]
    public record S : IQueryFragmentNode;

    [DebuggerDisplay("{Value}")]
    public record Data(string Value) : IQueryFragmentNode
    {
        public bool IsNotEmpty()
        {
            return Value.Length > 0;
        }
    }

    [DebuggerDisplay("Value")]
    public record Numeric(long Value) : IQueryFragmentNode
    {
        public bool IsNotEmpty()
        {
            return Value >= 0;
        }
    }

    [DebuggerDisplay("-")]
    public record Dash : IQueryFragmentNode;

    [DebuggerDisplay("#")]
    public record Hashtag : IQueryFragmentNode;

    [DebuggerDisplay("@")]
    public record Arobase : IQueryFragmentNode;

    public record Not : IQueryFragmentNode;

    [DebuggerDisplay("or")]
    public record Or : IQueryFragmentNode;

    [DebuggerDisplay("and")]
    public record And : IQueryFragmentNode;

    [DebuggerDisplay(".")]
    public record Dot : IQueryFragmentNode;

    [DebuggerDisplay(":")]
    public record Colon : IQueryFragmentNode;

    [DebuggerDisplay(",")]
    public record Comma : IQueryFragmentNode;

    [DebuggerDisplay("(")]
    public record LeftParen : IQueryFragmentNode;

    [DebuggerDisplay(")")]
    public record RightParen : IQueryFragmentNode;

    [DebuggerDisplay("[")]
    public record LeftBracket : IQueryFragmentNode;

    [DebuggerDisplay("]")]
    public record RightBracket : IQueryFragmentNode;
}
