namespace Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Render
{
    /// <summary>
    /// Helper for holding persistent state of Renderer.
    /// </summary>
    public interface IRenderContext
    {
        /// <summary>
        /// Gets or sets a value indicating whether to trim whitespace.
        /// </summary>
        bool TrimLeadingWhitespace { get; set; }

        /// <summary>
        /// Gets or sets the parent Element for this Context.
        /// </summary>
        object? Parent { get; set; }

        /// <summary>
        /// Clones the Context.
        /// </summary>
        /// <returns>Clone</returns>
        IRenderContext Clone();
    }
}