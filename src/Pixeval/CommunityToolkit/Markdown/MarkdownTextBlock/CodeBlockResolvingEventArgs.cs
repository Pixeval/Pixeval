using System;
using Microsoft.UI.Xaml.Documents;

namespace Pixeval.CommunityToolkit.Markdown.MarkdownTextBlock
{
    /// <summary>
    /// Arguments for the <see cref="MarkdownTextBlock.CodeBlockResolving"/> event when a Code Block is being rendered.
    /// </summary>
    public class CodeBlockResolvingEventArgs : EventArgs
    {
        internal CodeBlockResolvingEventArgs(InlineCollection inlineCollection, string text, string codeLanguage)
        {
            InlineCollection = inlineCollection;
            Text = text;
            CodeLanguage = codeLanguage;
        }

        /// <summary>
        /// Gets the language of the Code Block, as specified by ```{Language} on the first line of the block,
        /// e.g. <para/>
        /// ```C# <para/>
        /// public void Method();<para/>
        /// ```<para/>
        /// </summary>
        public string CodeLanguage { get; }

        /// <summary>
        /// Gets the raw code block text
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Gets Collection to add formatted Text to.
        /// </summary>
        public InlineCollection InlineCollection { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this event was handled successfully.
        /// </summary>
        public bool Handled { get; set; } = false;
    }
}