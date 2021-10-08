using Microsoft.UI.Xaml.Media;
using Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Render;

namespace Pixeval.CommunityToolkit.Markdown.Render
{
    /// <summary>
    /// The Context of the Current Position
    /// </summary>
    public abstract class RenderContext : IRenderContext
    {
        /// <summary>
        /// Gets or sets the Foreground of the Current Context.
        /// </summary>
        public Brush? Foreground { get; set; }

        /// <inheritdoc/>
        public bool TrimLeadingWhitespace { get; set; }

        /// <inheritdoc/>
        public object? Parent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to override the Foreground Property.
        /// </summary>
        public bool OverrideForeground { get; set; }

        /// <inheritdoc/>
        public IRenderContext Clone()
        {
            return (IRenderContext) MemberwiseClone();
        }
    }
}