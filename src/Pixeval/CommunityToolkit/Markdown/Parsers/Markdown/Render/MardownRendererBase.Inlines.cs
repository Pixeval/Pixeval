#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/MardownRendererBase.Inlines.cs
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

using Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Inlines;

namespace Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Render
{
    /// <summary>
    ///     Inline Rendering Methods.
    /// </summary>
    public partial class MarkdownRendererBase
    {
        /// <summary>
        ///     Renders emoji element.
        /// </summary>
        /// <param name="element"> The parsed inline element to render. </param>
        /// <param name="context"> Persistent state. </param>
        protected abstract void RenderEmoji(EmojiInline element, IRenderContext context);

        /// <summary>
        ///     Renders a text run element.
        /// </summary>
        /// <param name="element"> The parsed inline element to render. </param>
        /// <param name="context"> Persistent state. </param>
        protected abstract void RenderTextRun(TextRunInline element, IRenderContext context);

        /// <summary>
        ///     Renders a bold run element.
        /// </summary>
        /// <param name="element"> The parsed inline element to render. </param>
        /// <param name="context"> Persistent state. </param>
        protected abstract void RenderBoldRun(BoldTextInline element, IRenderContext context);

        /// <summary>
        ///     Renders a link element
        /// </summary>
        /// <param name="element"> The parsed inline element to render. </param>
        /// <param name="context"> Persistent state. </param>
        protected abstract void RenderMarkdownLink(MarkdownLinkInline element, IRenderContext context);

        /// <summary>
        ///     Renders an image element.
        /// </summary>
        /// <param name="element"> The parsed inline element to render. </param>
        /// <param name="context"> Persistent state. </param>
        protected abstract void RenderImage(ImageInline element, IRenderContext context);

        /// <summary>
        ///     Renders a raw link element.
        /// </summary>
        /// <param name="element"> The parsed inline element to render. </param>
        /// <param name="context"> Persistent state. </param>
        protected abstract void RenderHyperlink(HyperlinkInline element, IRenderContext context);

        /// <summary>
        ///     Renders a text run element.
        /// </summary>
        /// <param name="element"> The parsed inline element to render. </param>
        /// <param name="context"> Persistent state. </param>
        protected abstract void RenderItalicRun(ItalicTextInline element, IRenderContext context);

        /// <summary>
        ///     Renders a strike through element.
        /// </summary>
        /// <param name="element"> The parsed inline element to render. </param>
        /// <param name="context"> Persistent state. </param>
        protected abstract void RenderStrikeThroughRun(StrikeThroughTextInline element, IRenderContext context);

        /// <summary>
        ///     Renders a superscript element.
        /// </summary>
        /// <param name="element"> The parsed inline element to render. </param>
        /// <param name="context"> Persistent state. </param>
        protected abstract void RenderSuperscriptRun(SuperscriptTextInline element, IRenderContext context);

        /// <summary>
        ///     Renders a subscript element.
        /// </summary>
        /// <param name="element"> The parsed inline element to render. </param>
        /// <param name="context"> Persistent state. </param>
        protected abstract void RenderSubscriptRun(SubscriptTextInline element, IRenderContext context);

        /// <summary>
        ///     Renders a code element
        /// </summary>
        /// <param name="element"> The parsed inline element to render. </param>
        /// <param name="context"> Persistent state. </param>
        protected abstract void RenderCodeRun(CodeInline element, IRenderContext context);
    }
}