using System.Collections.Generic;

namespace Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Blocks.List
{
    /// <summary>
    /// This specifies the Content of the List element.
    /// </summary>
    public class ListItemBlock
    {
        /// <summary>
        /// Gets or sets the contents of the list item.
        /// </summary>
        public IList<MarkdownBlock?>? Blocks { get; set; }

        internal ListItemBlock()
        {
        }
    }
}