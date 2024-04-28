namespace Pixeval.Filters.TagParser;

/// <summary>
/// Fragments of data that a query string carries, could be a data or numeric value, or some labels like #, @, : or standalone characters like a or c.
/// </summary>
internal interface IQueryFragmentNode
{
    internal record A : IQueryFragmentNode;

    internal record C : IQueryFragmentNode;

    internal record Data(string Value) : IQueryFragmentNode;

    internal record Numeric(long Value) : IQueryFragmentNode;

    internal record Dash : IQueryFragmentNode;

    internal record Hashtag : IQueryFragmentNode;

    internal record Arobase : IQueryFragmentNode;

    internal record Colon : IQueryFragmentNode;

    internal record Comma : IQueryFragmentNode;

    internal record LeftParen : IQueryFragmentNode;

    internal record RightParen : IQueryFragmentNode;

    internal record LeftBracket : IQueryFragmentNode;

    internal record RightBracket : IQueryFragmentNode;
    
}
