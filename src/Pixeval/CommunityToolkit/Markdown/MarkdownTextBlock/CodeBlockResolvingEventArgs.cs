#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/CodeBlockResolvingEventArgs.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using Microsoft.UI.Xaml.Documents;

namespace Pixeval.CommunityToolkit.Markdown.MarkdownTextBlock
{
    /// <summary>
    ///     Arguments for the <see cref="MarkdownTextBlock.CodeBlockResolving" /> event when a Code Block is being rendered.
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
        ///     Gets the language of the Code Block, as specified by ```{Language} on the first line of the block,
        ///     e.g.
        ///     <para />
        ///     ```C#
        ///     <para />
        ///     public void Method();
        ///     <para />
        ///     ```
        ///     <para />
        /// </summary>
        public string CodeLanguage { get; }

        /// <summary>
        ///     Gets the raw code block text
        /// </summary>
        public string Text { get; }

        /// <summary>
        ///     Gets Collection to add formatted Text to.
        /// </summary>
        public InlineCollection InlineCollection { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether this event was handled successfully.
        /// </summary>
        public bool Handled { get; set; } = false;
    }
}