#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/MarkdownRendererBase.Blocks.cs
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

using Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Blocks;

namespace Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Render
{
    /// <summary>
    ///     Block Rendering Methods.
    /// </summary>
    public partial class MarkdownRendererBase
    {
        /// <summary>
        ///     Renders a paragraph element.
        /// </summary>
        protected abstract void RenderParagraph(ParagraphBlock element, IRenderContext context);

        /// <summary>
        ///     Renders a yaml header element.
        /// </summary>
        protected abstract void RenderYamlHeader(YamlHeaderBlock element, IRenderContext context);

        /// <summary>
        ///     Renders a header element.
        /// </summary>
        protected abstract void RenderHeader(HeaderBlock element, IRenderContext context);

        /// <summary>
        ///     Renders a list element.
        /// </summary>
        protected abstract void RenderListElement(ListBlock element, IRenderContext context);

        /// <summary>
        ///     Renders a horizontal rule element.
        /// </summary>
        protected abstract void RenderHorizontalRule(IRenderContext context);

        /// <summary>
        ///     Renders a quote element.
        /// </summary>
        protected abstract void RenderQuote(QuoteBlock element, IRenderContext context);

        /// <summary>
        ///     Renders a code element.
        /// </summary>
        protected abstract void RenderCode(CodeBlock element, IRenderContext context);

        /// <summary>
        ///     Renders a table element.
        /// </summary>
        protected abstract void RenderTable(TableBlock element, IRenderContext context);
    }
}