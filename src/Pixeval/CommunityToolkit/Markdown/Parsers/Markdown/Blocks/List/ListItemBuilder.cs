using System.Text;
using Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Enums;

namespace Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Blocks.List
{
    internal class ListItemBuilder : MarkdownBlock
    {
        public StringBuilder Builder { get; } = new();

        public ListItemBuilder()
            : base(MarkdownBlockType.ListItemBuilder)
        {
        }
    }
}