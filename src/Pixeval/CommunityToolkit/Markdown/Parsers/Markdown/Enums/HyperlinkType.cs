// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo

namespace Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Enums
{
    /// <summary>
    /// Specifies the type of Hyperlink that is used.
    /// </summary>
    public enum HyperlinkType
    {
        /// <summary>
        /// A hyperlink surrounded by angle brackets (e.g. "http://www.reddit.com").
        /// </summary>
        BracketedUrl,

        /// <summary>
        /// A fully qualified hyperlink (e.g. "http://www.reddit.com").
        /// </summary>
        FullUrl,

        /// <summary>
        /// A URL without a scheme (e.g. "www.reddit.com").
        /// </summary>
        PartialUrl,

        /// <summary>
        /// An email address (e.g. "test@reddit.com").
        /// </summary>
        Email,

        /// <summary>
        /// A subreddit link (e.g. "/r/news").
        /// </summary>
        Subreddit,

        /// <summary>
        /// A user link (e.g. "/u/quinbd").
        /// </summary>
        User,
    }
}