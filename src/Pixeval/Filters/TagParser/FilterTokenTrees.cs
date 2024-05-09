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

public record TokenAndNode(IEnumerable<ITokenTreeNode> Children) : ITokenTreeNode
{
    public ITokenTreeNode? Parent { get; set; } = null;
    
    public virtual bool Equals(TokenAndNode? other)
    {
        return other?.Children.SequenceEqual(this.Children) ?? false;
    }

    public void Insert(ITokenTree subElem)
    {
        this.Children.Append(subElem);
        subElem.Parent = this;
    }
}

public record TokenOrNode(IEnumerable<ITokenTreeNode> Children) : ITokenTreeNode
{
    public ITokenTreeNode? Parent { get; set; } = null;
    
    public virtual bool Equals(TokenOrNode? other)
    {
        return other?.Children.SequenceEqual(this.Children) ?? false;
    }
    
    public void Insert(ITokenTree subElem)
    {
        this.Children.Append(subElem);
        subElem.Parent = this;
    }
}

public record TagToken(string Content) : IQueryToken
{
    public ITokenTreeNode? Parent { get; set; } = null;
    
    public virtual bool Equals(TagToken? other)
    {
        return other?.Content.Equals(this.Content) ?? false;
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

public record NumericRangeToken(RangeType Type, long? Start, long? End) : IQueryToken
{
    public ITokenTreeNode? Parent { get; set; } = null;
    
    public virtual bool Equals(NumericRangeToken? other)
    {
        return other?.Type == this.Type && other?.Start == this.Start && other?.End == this.End;
    }
}

public record DateToken(RangeEdge Edge, DateTimeOffset? Date) : IQueryToken
{
    public ITokenTreeNode? Parent { get; set; } = null;
    
    public virtual bool Equals(DateToken? other)
    {
        return other?.Date == this.Date && other?.Edge == this.Edge;
    }
}

public record FilterSetting(
    ITokenTreeNode TagTree
);
