#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/InlineRenderContext.cs
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

using Microsoft.UI.Xaml.Documents;
using Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Render;

namespace Pixeval.CommunityToolkit.Markdown.Render
{
    /// <summary>
    ///     The Context of the Current Document Rendering.
    /// </summary>
    public class InlineRenderContext : RenderContext
    {
        internal InlineRenderContext(InlineCollection inlineCollection, IRenderContext context)
        {
            InlineCollection = inlineCollection;
            TrimLeadingWhitespace = context.TrimLeadingWhitespace;
            Parent = context.Parent;

            if (context is RenderContext localContext)
            {
                Foreground = localContext.Foreground;
                OverrideForeground = localContext.OverrideForeground;
            }

            if (context is InlineRenderContext inlineContext)
            {
                WithinBold = inlineContext.WithinBold;
                WithinItalics = inlineContext.WithinItalics;
                WithinHyperlink = inlineContext.WithinHyperlink;
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the Current Element is being rendered inside an Italics Run.
        /// </summary>
        public bool WithinItalics { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the Current Element is being rendered inside a Bold Run.
        /// </summary>
        public bool WithinBold { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the Current Element is being rendered inside a Link.
        /// </summary>
        public bool WithinHyperlink { get; set; }

        /// <summary>
        ///     Gets or sets the list to add to.
        /// </summary>
        public InlineCollection InlineCollection { get; set; }
    }
}