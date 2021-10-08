using Microsoft.UI.Xaml.Documents;
using Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Render;

namespace Pixeval.CommunityToolkit.Markdown.Render
{
    /// <summary>
    /// The Context of the Current Document Rendering.
    /// </summary>
    public class InlineRenderContext : RenderContext
    {
        internal InlineRenderContext(InlineCollection inlineCollection, IRenderContext context)
        {
            InlineCollection = inlineCollection;
            TrimLeadingWhitespace = context.TrimLeadingWhitespace;
            Parent = context.Parent;

            if (context is RenderContext localContext)
            {
                Foreground = localContext.Foreground;
                OverrideForeground = localContext.OverrideForeground;
            }

            if (context is InlineRenderContext inlineContext)
            {
                WithinBold = inlineContext.WithinBold;
                WithinItalics = inlineContext.WithinItalics;
                WithinHyperlink = inlineContext.WithinHyperlink;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Current Element is being rendered inside an Italics Run.
        /// </summary>
        public bool WithinItalics { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Current Element is being rendered inside a Bold Run.
        /// </summary>
        public bool WithinBold { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Current Element is being rendered inside a Link.
        /// </summary>
        public bool WithinHyperlink { get; set; }

        /// <summary>
        /// Gets or sets the list to add to.
        /// </summary>
        public InlineCollection InlineCollection { get; set; }
    }
}