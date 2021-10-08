using System.Linq;

namespace Pixeval.CommunityToolkit.Markdown.Parsers.Core
{
    /// <summary>
    /// This class offers helpers for Parsing.
    /// </summary>
    public static class ParseHelpers
    {
        /// <summary>
        /// Determines if a Markdown string is blank or comprised entirely of whitespace characters.
        /// </summary>
        /// <returns>true if blank or white space</returns>
        public static bool IsMarkdownBlankOrWhiteSpace(string str)
        {
            return str.All(IsMarkdownWhiteSpace);
        }

        /// <summary>
        /// Determines if a character is a Markdown whitespace character.
        /// </summary>
        /// <returns>true if is white space</returns>
        public static bool IsMarkdownWhiteSpace(char c)
        {
            return c is ' ' or '\t' or '\r' or '\n';
        }
    }
}