namespace Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Helpers
{
    /// <summary>
    /// Represents the result of parsing an inline element.
    /// </summary>
    internal class InlineParseResult
    {
        public InlineParseResult(MarkdownInline parsedElement, int start, int end)
        {
            ParsedElement = parsedElement;
            Start = start;
            End = end;
        }

        /// <summary>
        /// Gets the element that was parsed (can be <c>null</c>).
        /// </summary>
        public MarkdownInline ParsedElement { get; }

        /// <summary>
        /// Gets the position of the first character in the parsed element.
        /// </summary>
        public int Start { get; }

        /// <summary>
        /// Gets the position of the character after the last character in the parsed element.
        /// </summary>
        public int End { get; }
    }
}