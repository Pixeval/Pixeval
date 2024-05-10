using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Pixeval.Filters;

public abstract record TreeNodeBase(bool IsNot)
{
    public LeafSequence? Parent { get; set; }
}

public enum SequenceType
{
    And, Or
}

[DebuggerDisplay("({Type} ...)")]
public record LeafSequence(SequenceType Type, IList<TreeNodeBase> Children, bool IsNot) : TreeNodeBase(IsNot), IEnumerable<TreeNodeBase>
{
    public void Insert(TreeNodeBase subElem)
    {
        Children.Add(subElem);
        subElem.Parent = this;
    }

    IEnumerator<TreeNodeBase> IEnumerable<TreeNodeBase>.GetEnumerator() => Children.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Children).GetEnumerator();
}

public abstract record QueryLeaf(bool IsNot) : TreeNodeBase(IsNot);

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

[DebuggerDisplay("{Type} ({IsInclude})")]
public record BoolLeaf(bool IsInclude, BoolType Type, bool IsNot) : QueryLeaf(IsNot);

[DebuggerDisplay("{Content} ({Type})")]
public record StringLeaf(StringType Type, IQueryToken.Data Content, bool IsNot) : QueryLeaf(IsNot);

[DebuggerDisplay("{Start}-{End} ({Type})")]
public record NumericRangeLeaf(RangeType Type, long? Start, long? End, bool IsNot) : QueryLeaf(IsNot);

[DebuggerDisplay("{Date} ({Edge})")]
public record DateLeaf(RangeEdge Edge, DateTimeOffset Date, bool IsNot) : QueryLeaf(IsNot);
