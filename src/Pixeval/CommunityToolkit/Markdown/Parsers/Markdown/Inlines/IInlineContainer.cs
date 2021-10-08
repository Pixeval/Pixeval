using System.Collections.Generic;

namespace Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Inlines
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IInlineContainer"/> class.
    /// </summary>
    public interface IInlineContainer
    {
        /// <summary>
        /// Gets or sets the contents of the inline.
        /// </summary>
        IList<MarkdownInline>? Inlines { get; set; }
    }
}