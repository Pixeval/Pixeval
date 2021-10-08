using Microsoft.UI.Xaml.Documents;

namespace Pixeval.CommunityToolkit.Markdown.Render
{
    /// <summary>
    /// A Parser to parse code strings into Syntax Highlighted text.
    /// </summary>
    public interface ICodeBlockResolver
    {
        /// <summary>
        /// Parses Code Block text into Rich text.
        /// </summary>
        /// <param name="inlineCollection">Block to add formatted Text to.</param>
        /// <param name="text">The raw code block text</param>
        /// <param name="codeLanguage">The language of the Code Block, as specified by ```{Language} on the first line of the block,
        /// e.g. <para/>
        /// ```C# <para/>
        /// public void Method();<para/>
        /// ```<para/>
        /// </param>
        /// <returns>Parsing was handled Successfully</returns>
        bool ParseSyntax(InlineCollection inlineCollection, string text, string codeLanguage);
    }
}