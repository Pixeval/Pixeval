// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

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
public partial record LeafSequence(SequenceType Type, IList<TreeNodeBase> Children, bool IsNot) : TreeNodeBase(IsNot), IEnumerable<TreeNodeBase>
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
    R18, R18G, Ai, Gif
}

public enum StringType
{
    Title, Author, Tag
}

public enum NumericRangeType
{
    Bookmark, Index
}

public enum FloatRangeType
{
    Ratio
}

public enum DateRangeEdge
{
    Starting, Ending
}

[DebuggerDisplay("{Type} ({IsExclude})")]
public record BoolLeaf(bool IsExclude, BoolType Type, bool IsNot) : QueryLeaf(IsNot);

[DebuggerDisplay("@{Value}")]
public record NumericLeaf(long Value, bool IsNot) : QueryLeaf(IsNot);

[DebuggerDisplay("{Content} ({Type})")]
public record StringLeaf(StringType Type, IQueryToken.Data Content, bool IsNot) : QueryLeaf(IsNot);

[DebuggerDisplay("{Range}")]
public abstract record AbstractRangeLeaf<T>(Range<T> Range, bool IsNot) : QueryLeaf(IsNot) where T : INumber<T>
{
    public bool IsInRange(T value)
    {
        return value >= Range.Start && (value < Range.End || Range.IsFromEnd);
    }
}

[DebuggerDisplay("{Range} ({Type})")]
public record NumericRangeLeaf(NumericRangeType Type, Range<long> Range, bool IsNot) : AbstractRangeLeaf<long>(Range, IsNot)
{
    public Range NarrowRange => new Range(new Index(Parser.TryNarrow(Range.Start)), new Index(Parser.TryNarrow(Range.End), Range.IsFromEnd));
}

[DebuggerDisplay("{Range} ({Type})")]
public record FloatRangeLeaf(FloatRangeType Type, Range<double> Range, bool IsNot) : AbstractRangeLeaf<double>(Range, IsNot);

[DebuggerDisplay("{Date} ({Edge})")]
public record DateLeaf(DateRangeEdge Edge, DateTimeOffset Date, bool IsNot) : QueryLeaf(IsNot);

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="Start"></param>
/// <param name="End"></param>
/// <param name="IsFromEnd">为<see langword="true"/>时，<see cref="End"/>就是最后一位(^1)</param>
public readonly record struct Range<T>(T Start, T End, bool IsFromEnd) where T : INumber<T>;
