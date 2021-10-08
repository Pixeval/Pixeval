namespace Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Blocks.List
{
    internal class NestedListInfo
    {
        public ListBlock? List { get; set; }

        // The number of spaces at the start of the line the list first appeared.
        public int SpaceCount { get; set; }
    }
}