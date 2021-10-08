namespace Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Enums
{
    /// <summary>
    /// Determines the type of Block the Block element is.
    /// </summary>
    public enum MarkdownBlockType
    {
        /// <summary>
        /// The root element
        /// </summary>
        Root,

        /// <summary>
        /// A paragraph element.
        /// </summary>
        Paragraph,

        /// <summary>
        /// A quote block
        /// </summary>
        Quote,

        /// <summary>
        /// A code block
        /// </summary>
        Code,

        /// <summary>
        /// A header block
        /// </summary>
        Header,

        /// <summary>
        /// A list block
        /// </summary>
        List,

        /// <summary>
        /// A list item block
        /// </summary>
        ListItemBuilder,

        /// <summary>
        /// a horizontal rule block
        /// </summary>
        HorizontalRule,

        /// <summary>
        /// A table block
        /// </summary>
        Table,

        /// <summary>
        /// A link block
        /// </summary>
        LinkReference,

        /// <summary>
        /// A Yaml header block
        /// </summary>
        YamlHeader
    }
}