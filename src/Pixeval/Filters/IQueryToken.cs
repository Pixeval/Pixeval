using System.Diagnostics;

namespace Pixeval.Filters;

/// <summary>
/// Fragments of data that a query string carries, could be a data or numeric value, or some labels like #, @, : or standalone characters like a or c.
/// </summary>
public interface IQueryToken
{
    public interface INullableToken : IQueryToken
    {
        public bool IsNotEmpty();
    }

    [DebuggerDisplay("l")]
    public readonly record struct Like : IQueryToken;

    [DebuggerDisplay("i")]
    public readonly record struct Index : IQueryToken;

    [DebuggerDisplay("s")]
    public readonly record struct StartDate : IQueryToken;

    [DebuggerDisplay("e")]
    public readonly record struct EndDate : IQueryToken;

    [DebuggerDisplay("{Value} ({IsPrecise})")]
    public readonly record struct Data(string Value) : INullableToken
    {
        public string Value { get; } = Value.EndsWith('$') ? Value[..^1] : Value;

        public bool IsPrecise { get; } = Value.EndsWith('$');

        public bool IsNotEmpty() => Value.Length > 0;
    }

    [DebuggerDisplay("Value")]
    public readonly record struct Numeric(long Value) : INullableToken
    {
        public bool IsNotEmpty() => Value >= 0;
    }

    [DebuggerDisplay("+")]
    public readonly record struct Plus : IQueryToken;

    [DebuggerDisplay("-")]
    public readonly record struct Dash : IQueryToken;

    [DebuggerDisplay("#")]
    public readonly record struct Hashtag : IQueryToken;

    [DebuggerDisplay("@")]
    public readonly record struct At : IQueryToken;

    [DebuggerDisplay("!")]
    public readonly record struct Not : IQueryToken;

    [DebuggerDisplay("or")]
    public readonly record struct Or : IQueryToken;

    [DebuggerDisplay("and")]
    public readonly record struct And : IQueryToken;

    [DebuggerDisplay(".")]
    public readonly record struct Dot : IQueryToken;

    [DebuggerDisplay(":")]
    public readonly record struct Colon : IQueryToken;

    [DebuggerDisplay(",")]
    public readonly record struct Comma : IQueryToken;

    [DebuggerDisplay("(")]
    public readonly record struct LeftParen : IQueryToken;

    [DebuggerDisplay(")")]
    public readonly record struct RightParen : IQueryToken;

    [DebuggerDisplay("[")]
    public readonly record struct LeftBracket : IQueryToken;

    [DebuggerDisplay("]")]
    public readonly record struct RightBracket : IQueryToken;
}
