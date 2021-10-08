namespace Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Helpers
{
    internal class LineInfo
    {
        public int StartOfLine { get; set; }

        public int FirstNonWhitespaceChar { get; set; }

        public int EndOfLine { get; set; }

        public bool IsLineBlank => FirstNonWhitespaceChar == EndOfLine;

        public int StartOfNextLine { get; set; }
    }
}