// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Pixeval.Filters;

/// <summary>
/// Fragments of data that a query string carries, could be a data or numeric value, or some labels like #, @, : or standalone characters like a or c.
/// </summary>
[SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
public interface IQueryToken
{
    static virtual string Name => "<Unknown>";

    interface INullableToken : IQueryToken
    {
        bool IsNotEmpty();
    }

    readonly record struct Like : IQueryToken
    {
        public override string ToString() => Name;

        public static string Name => "l";
    }

    readonly record struct Index : IQueryToken
    {
        public override string ToString() => Name;

        public static string Name => "i";
    }

    readonly record struct Ratio : IQueryToken
    {
        public override string ToString() => Name;

        public static string Name => "r";
    }

    readonly record struct StartDate : IQueryToken
    {
        public override string ToString() => Name;

        public static string Name => "s";
    }

    readonly record struct EndDate : IQueryToken
    {
        public override string ToString() => Name;

        public static string Name => "e";
    }

    [DebuggerDisplay("{Value} ({IsPrecise})")]
    readonly record struct Data(string Value) : INullableToken
    {
        public string Value { get; } = Value.EndsWith('$') ? Value[..^1] : Value;

        public bool IsPrecise { get; } = Value.EndsWith('$');

        public bool IsNotEmpty() => Value.Length > 0;

        public override string ToString() => Value;

        public static string Name => "<String>";
    }

    [DebuggerDisplay("{Value}")]
    readonly record struct Numeric(long Value) : INullableToken
    {
        public bool IsNotEmpty() => Value >= 0;

        public override string ToString() => Value.ToString();

        public static string Name => "<Number>";
    }

    readonly record struct Plus : IQueryToken
    {
        public override string ToString() => Name;

        public static string Name => "+";
    }

    readonly record struct Dash : IQueryToken
    {
        public override string ToString() => Name;

        public static string Name => "-";
    }

    readonly record struct Hashtag : IQueryToken
    {
        public override string ToString() => Name;

        public static string Name => "#";
    }

    readonly record struct At : IQueryToken
    {
        public override string ToString() => Name;

        public static string Name => "@";
    }

    readonly record struct Slash : IQueryToken
    {
        public override string ToString() => Name;

        public static string Name => "/";
    }

    readonly record struct Not : IQueryToken
    {
        public override string ToString() => Name;

        public static string Name => "!";
    }

    readonly record struct Or : IQueryToken
    {
        public override string ToString() => Name;

        public static string Name => "or";
    }

    readonly record struct And : IQueryToken
    {
        public override string ToString() => Name;

        public static string Name => "and";
    }

    readonly record struct Dot : IQueryToken
    {
        public override string ToString() => Name;

        public static string Name => ".";
    }

    readonly record struct Colon : IQueryToken
    {
        public override string ToString() => Name;

        public static string Name => ":";
    }

    readonly record struct Comma : IQueryToken
    {
        public override string ToString() => Name;

        public static string Name => ",";
    }

    readonly record struct LeftParen : IQueryToken
    {
        public override string ToString() => Name;

        public static string Name => "(";
    }

    readonly record struct RightParen : IQueryToken
    {
        public override string ToString() => Name;

        public static string Name => ")";
    }

    readonly record struct LeftBracket : IQueryToken
    {
        public override string ToString() => Name;

        public static string Name => "[";
    }

    readonly record struct RightBracket : IQueryToken
    {
        public override string ToString() => Name;

        public static string Name => "]";
    }
}
