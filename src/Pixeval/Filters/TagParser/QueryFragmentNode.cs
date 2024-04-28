using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixeval.Filters.TagParser;
/**
 * Fragments of data that a query string carries, could be a data or numeric value, or some labels like #, @, : or standalone characters like a or c.
 */
internal interface QueryFragmentNode
{
    internal record A() : QueryFragmentNode;

    internal record C() : QueryFragmentNode;

    internal record Data(String data) : QueryFragmentNode;

    internal record Numeric(long value) : QueryFragmentNode;

    internal record Dash() : QueryFragmentNode;

    internal record Hashtag() : QueryFragmentNode;

    internal record Arobase() : QueryFragmentNode;

    internal record Colon() : QueryFragmentNode;

    internal record Comma() : QueryFragmentNode;

    internal record LeftParen() : QueryFragmentNode;

    internal record RightParen() : QueryFragmentNode;

    internal record LeftBracket() : QueryFragmentNode;

    internal record RightBracket() : QueryFragmentNode;
    
}
