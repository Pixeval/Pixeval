using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Pixeval.Filters.TagParser;

public interface ITokenTree
{
    public ITokenTreeNode? Parent { get; set; }
}

public interface ITokenTreeNode : ITokenTree
{
    public void Insert(ITokenTree subElem);
}

public interface IQueryToken : ITokenTree;

[DebuggerDisplay("(and ...)")]
public record TokenAndNode(IList<ITokenTree> Children) : ITokenTreeNode, IEnumerable<ITokenTree>
{
    public ITokenTreeNode? Parent { get; set; } = null;

    public virtual bool Equals(TokenAndNode? other)
    {
        return other?.Children.SequenceEqual(Children) ?? false;
    }

    public void Insert(ITokenTree subElem)
    {
        Children.Add(subElem);
        subElem.Parent = this;
    }

    IEnumerator<ITokenTree> IEnumerable<ITokenTree>.GetEnumerator() => Children.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Children).GetEnumerator();
}

[DebuggerDisplay("(or ...)")]
public record TokenOrNode(IList<ITokenTree> Children) : ITokenTreeNode, IEnumerable<ITokenTree>
{
    public ITokenTreeNode? Parent { get; set; } = null;

    public virtual bool Equals(TokenOrNode? other)
    {
        return other?.Children.SequenceEqual(Children) ?? false;
    }

    public void Insert(ITokenTree subElem)
    {
        Children.Add(subElem);
        subElem.Parent = this;
    }

    IEnumerator<ITokenTree> IEnumerable<ITokenTree>.GetEnumerator() => Children.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Children).GetEnumerator();
}

[DebuggerDisplay("#{Content}")]
public record TagToken(string Content) : IQueryToken
{
    public ITokenTreeNode? Parent { get; set; } = null;

    public virtual bool Equals(TagToken? other)
    {
        return other?.Content.Equals(Content) ?? false;
    }
}

public enum RangeType
{
    Collection, Sequences
}

public enum RangeEdge
{
    Starting, Ending
}

[DebuggerDisplay("{Start}-{End} ({Type})")]
public record NumericRangeToken(RangeType Type, long? Start, long? End) : IQueryToken
{
    public ITokenTreeNode? Parent { get; set; } = null;

    public virtual bool Equals(NumericRangeToken? other)
    {
        return other?.Type == Type && other?.Start == Start && other?.End == End;
    }
}

[DebuggerDisplay("{Date} ({Edge})")]
public record DateToken(RangeEdge Edge, DateTimeOffset? Date) : IQueryToken
{
    public ITokenTreeNode? Parent { get; set; } = null;

    public virtual bool Equals(DateToken? other)
    {
        return other?.Date == Date && other?.Edge == Edge;
    }
}
