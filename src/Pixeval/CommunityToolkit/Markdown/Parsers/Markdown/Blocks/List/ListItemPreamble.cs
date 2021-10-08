using Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Enums;

namespace Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Blocks.List
{
    internal class ListItemPreamble
    {
        public ListStyle Style { get; set; }

        public int ContentStartPos { get; set; }
    }
}