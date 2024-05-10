using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Pixeval.Filters;

public abstract record TokenTree(bool IsNot)
{
    public TokenTreeNode? Parent { get; set; }
}

public enum TreeType
{
    And, Or
}

[DebuggerDisplay("... ({Type})")]
public record TokenTreeNode(TreeType Type, IList<TokenTree> Children, bool IsNot) : TokenTree(IsNot), IEnumerable<TokenTree>
{
    public virtual bool Equals(TokenTreeNode? other)
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
public record StringToken(StringType Type, IQueryNode.Data Content, bool IsNot) : QueryToken(IsNot)
{
    public virtual bool Equals(StringToken? other)
    {
        return other?.Content.Equals(Content) ?? false;
    }
}

[DebuggerDisplay("{Start}-{End} ({Type})")]
public record NumericRangeToken(RangeType Type, long? Start, long? End, bool IsNot) : QueryToken(IsNot)
{
    public virtual bool Equals(NumericRangeToken? other)
    {
        return other?.Type == Type && other?.Start == Start && other?.End == End;
    }
}

[DebuggerDisplay("{Date} ({Edge})")]
public record DateToken(RangeEdge Edge, DateTimeOffset Date, bool IsNot) : QueryToken(IsNot)
{
    public virtual bool Equals(DateToken? other)
    {
        return other?.Date == Date && other?.Edge == Edge;
    }
}
