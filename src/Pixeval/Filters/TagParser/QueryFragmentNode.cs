namespace Pixeval.Filters.TagParser;

/// <summary>
/// Fragments of data that a query string carries, could be a data or numeric value, or some labels like #, @, : or standalone characters like a or c.
/// </summary>
public interface IQueryFragmentNode
{
    bool isNotEmpty()
    {
        return true;
    }
    
    public record A : IQueryFragmentNode;

    public record C : IQueryFragmentNode;

    public record E : IQueryFragmentNode;

    public record L : IQueryFragmentNode;

    public record N : IQueryFragmentNode;

    public record S : IQueryFragmentNode;

    public record Data(string Value) : IQueryFragmentNode
    {
        public bool isNotEmpty()
        {
            return Value.Length > 0;
        }
    }

    public record Numeric(long Value) : IQueryFragmentNode
    {
        public bool isNotEmpty()
        {
            return Value >= 0;
        }
    }

    public record Dash : IQueryFragmentNode;

    public record Hashtag : IQueryFragmentNode;

    public record Arobase : IQueryFragmentNode;

    public record Not : IQueryFragmentNode;

    public record Or : IQueryFragmentNode;

    public record And : IQueryFragmentNode;

    public record Dot : IQueryFragmentNode;

    public record Colon : IQueryFragmentNode;

    public record Comma : IQueryFragmentNode;

    public record LeftParen : IQueryFragmentNode;

    public record RightParen : IQueryFragmentNode;

    public record LeftBracket : IQueryFragmentNode;

    public record RightBracket : IQueryFragmentNode;
    
}
