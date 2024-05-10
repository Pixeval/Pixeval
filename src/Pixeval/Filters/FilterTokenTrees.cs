using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;

namespace Pixeval.Filters;

public abstract record TokenTree(bool IsNot)
{
    public TokenSequence? Parent { get; set; }
}

public enum SequenceType
{
    And, Or
}

[DebuggerDisplay("... ({Type})")]
public record TokenSequence(SequenceType Type, IList<TokenTree> Children, bool IsNot) : TokenTree(IsNot), IEnumerable<TokenTree>
{
    public virtual bool Equals(TokenSequence? other)
    {
        return other?.Children.SequenceEqual(Children) ?? false;
    }

    public void Insert(TokenTree subElem)
    {
        Children.Add(subElem);
        subElem.Parent = this;
    }

    IEnumerator<TokenTree> IEnumerable<TokenTree>.GetEnumerator() => Children.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Children).GetEnumerator();
}

public abstract record QueryToken(bool IsNot) : TokenTree(IsNot);

public enum BoolType
{
    R18, R18G, Gif
}

public enum StringType
{
    Title, Author, Tag
}

public enum RangeType
{
    Bookmark, Index
}

public enum RangeEdge
{
    Starting, Ending
}

[DebuggerDisplay("{Content} ({Type})")]
public record BoolToken(bool IsInclude, BoolType Type, bool IsNot) : QueryToken(IsNot)
{
    public virtual bool Equals(BoolToken? other)
    {
        return other?.IsInclude == IsInclude && other?.Type == Type && other?.IsNot == IsNot;
    }
}

[DebuggerDisplay("{Content} ({Type})")]
public record StringToken(StringType Type, IQueryNode.Data Content, bool IsNot) : QueryToken(IsNot)
{
    public virtual bool Equals(StringToken? other)
    {
        return other?.Type == Type && other?.Content == Content && other?.IsNot == IsNot;
    }
}

[DebuggerDisplay("{Start}-{End} ({Type})")]
public record NumericRangeToken(RangeType Type, long? Start, long? End, bool IsNot) : QueryToken(IsNot)
{
    public virtual bool Equals(NumericRangeToken? other)
    {
        return other?.Type == Type && other?.Start == Start && other?.End == End && other?.IsNot == IsNot;
    }
}

[DebuggerDisplay("{Date} ({Edge})")]
public record DateToken(RangeEdge Edge, DateTimeOffset Date, bool IsNot) : QueryToken(IsNot)
{
    public virtual bool Equals(DateToken? other)
    {
        return other?.Date == Date && other?.Edge == Edge && other?.IsNot == IsNot;
    }
}
