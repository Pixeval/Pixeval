using Microsoft.UI.Xaml.Controls;
using Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Render;

namespace Pixeval.CommunityToolkit.Markdown.Render
{
    /// <summary>
    /// The Context of the Current Document Rendering.
    /// </summary>
    public class UIElementCollectionRenderContext : RenderContext
    {
        internal UIElementCollectionRenderContext(UIElementCollection? blockUIElementCollection)
        {
            BlockUIElementCollection = blockUIElementCollection;
        }

        internal UIElementCollectionRenderContext(UIElementCollection? blockUIElementCollection, IRenderContext context)
            : this(blockUIElementCollection)
        {
            TrimLeadingWhitespace = context.TrimLeadingWhitespace;
            Parent = context.Parent;

            if (context is RenderContext localContext)
            {
                Foreground = localContext.Foreground;
                OverrideForeground = localContext.OverrideForeground;
            }
        }

        /// <summary>
        /// Gets or sets the list to add to.
        /// </summary>
        public UIElementCollection? BlockUIElementCollection { get; set; }
    }
}