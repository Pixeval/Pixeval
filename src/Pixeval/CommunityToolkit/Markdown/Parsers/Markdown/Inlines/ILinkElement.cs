namespace Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Inlines
{
    /// <summary>
    /// Implemented by all inline link elements.
    /// </summary>
    internal interface ILinkElement
    {
        /// <summary>
        /// Gets the link URL.  This can be a relative URL, but note that subreddit links will always
        /// have the leading slash (i.e. the Url will be "/r/baconit" even if the text is
        /// "r/baconit").
        /// </summary>
        string? Url { get; }

        /// <summary>
        /// Gets a tooltip to display on hover.  Can be <c>null</c>.
        /// </summary>
        string? Tooltip { get; }
    }
}