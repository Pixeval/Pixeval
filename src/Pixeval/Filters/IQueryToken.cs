using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Pixeval.Filters;

/// <summary>
/// Fragments of data that a query string carries, could be a data or numeric value, or some labels like #, @, : or standalone characters like a or c.
/// </summary>
[SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
public interface IQueryToken
{
    public static virtual string Name => "<Unknown>";

    public interface INullableToken : IQueryToken
    {
        public bool IsNotEmpty();
    }

    public readonly record struct Like : IQueryToken
    {
        public override string ToString() => Name;

        public static string Name => "l";
    }

    public readonly record struct Index : IQueryToken
    {
        public override string ToString() => Name;

        public static string Name => "i";
    }

    public readonly record struct Ratio : IQueryToken
    {
        public override string ToString() => Name;

        public static string Name => "r";
    }

    public readonly record struct StartDate : IQueryToken
    {
        public override string ToString() => Name;

        public static string Name => "s";
    }

    public readonly record struct EndDate : IQueryToken
    {
        public override string ToString() => Name;

        public static string Name => "e";
    }

    [DebuggerDisplay("{Value} ({IsPrecise})")]
    public readonly record struct Data(string Value) : INullableToken
    {
        public string Value { get; } = Value.EndsWith('$') ? Value[..^1] : Value;

        public bool IsPrecise { get; } = Value.EndsWith('$');

        public bool IsNotEmpty() => Value.Length > 0;

        public override string ToString() => Value;

        public static string Name => "<String>";
    }

    [DebuggerDisplay("{Value}")]
    public readonly record struct Numeric(double Value) : INullableToken
    {
        public bool IsNotEmpty() => Value >= 0;

        public override string ToString() => Value.ToString();

        public static string Name => "<Number>";
    }

    public readonly record struct Plus : IQueryToken
    {
        public override string ToString() => Name;

        public static string Name => "+";
    }

    public readonly record struct Dash : IQueryToken
    {
        public override string ToString() => Name;

        public static string Name => "-";
    }

    public readonly record struct Hashtag : IQueryToken
    {
        public override string ToString() => Name;

        public static string Name => "#";
    }

    public readonly record struct At : IQueryToken
    {
        public override string ToString() => Name;

        public static string Name => "@";
    }
    
    public readonly record struct Slash : IQueryToken
    {
        public override string ToString() => Name;

        public static string Name => "/";
    }

    public readonly record struct Not : IQueryToken
    {
        public override string ToString() => Name;

        public static string Name => "!";
    }

    public readonly record struct Or : IQueryToken
    {
        public override string ToString() => Name;

        public static string Name => "or";
    }

    public readonly record struct And : IQueryToken
    {
        public override string ToString() => Name;

        public static string Name => "and";
    }

    public readonly record struct Dot : IQueryToken
    {
        public override string ToString() => Name;

        public static string Name => ".";
    }

    public readonly record struct Colon : IQueryToken
    {
        public override string ToString() => Name;

        public static string Name => ":";
    }

    public readonly record struct Comma : IQueryToken
    {
        public override string ToString() => Name;

        public static string Name => ",";
    }

    public readonly record struct LeftParen : IQueryToken
    {
        public override string ToString() => Name;

        public static string Name => "(";
    }

    public readonly record struct RightParen : IQueryToken
    {
        public override string ToString() => Name;

        public static string Name => ")";
    }

    public readonly record struct LeftBracket : IQueryToken
    {
        public override string ToString() => Name;

        public static string Name => "[";
    }

    public readonly record struct RightBracket : IQueryToken
    {
        public override string ToString() => Name;

        public static string Name => "]";
    }
}
