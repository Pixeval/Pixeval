#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/MarkdownRenderer.Properties.cs
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

using System.Reflection;
using Windows.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Pixeval.CommunityToolkit.Markdown.Render
{
    /// <summary>
    ///     Properties for the UWP Markdown Renderer
    /// </summary>
    public partial class MarkdownRenderer
    {
        /// <summary>
        ///     Super Hack to retain inertia and passing the Scroll data onto the Parent ScrollViewer.
        /// </summary>
        private static readonly MethodInfo pointerWheelChanged = typeof(ScrollViewer).GetMethod("OnPointerWheelChanged", BindingFlags.NonPublic | BindingFlags.Instance)!;

        /// <summary>
        ///     Gets or sets the Root Framework Element.
        /// </summary>
        private FrameworkElement? RootElement { get; set; }

        /// <summary>
        ///     Gets the interface that is used to register hyperlinks.
        /// </summary>
        protected ILinkRegister LinkRegister { get; }

        /// <summary>
        ///     Gets the interface that is used to resolve images.
        /// </summary>
        protected IImageResolver ImageResolver { get; }

        /// <summary>
        ///     Gets the Parser to parse code strings into Syntax Highlighted text.
        /// </summary>
        protected ICodeBlockResolver CodeBlockResolver { get; }

        /// <summary>
        ///     Gets the Default Emoji Font.
        /// </summary>
        protected FontFamily DefaultEmojiFont { get; }

        /// <summary>
        ///     Gets or sets a brush that provides the background of the control.
        /// </summary>
        public Brush? Background { get; set; }

        /// <summary>
        ///     Gets or sets a brush that describes the border fill of a control.
        /// </summary>
        public Brush? BorderBrush { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref="FlowDirection" /> of the markdown.
        /// </summary>
        public FlowDirection FlowDirection { get; set; }

        /// <summary>
        ///     Gets or sets the font used to display text in the control.
        /// </summary>
        public FontFamily? FontFamily { get; set; }

        /// <summary>
        ///     Gets or sets the style in which the text is rendered.
        /// </summary>
        public FontStyle FontStyle { get; set; }

        /// <summary>
        ///     Gets or sets the thickness of the specified font.
        /// </summary>
        public FontWeight FontWeight { get; set; }

        /// <summary>
        ///     Gets or sets a brush that describes the foreground color.
        /// </summary>
        public Brush? Foreground { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether text selection is enabled.
        /// </summary>
        public bool IsTextSelectionEnabled { get; set; }

        /// <summary>
        ///     Gets or sets the brush used to fill the background of a code block.
        /// </summary>
        public Brush? CodeBackground { get; set; }

        /// <summary>
        ///     Gets or sets the brush used to fill the background of inline code.
        /// </summary>
        public Brush? InlineCodeBackground { get; set; }

        /// <summary>
        ///     Gets or sets the brush used to fill the foreground of inline code.
        /// </summary>
        public Brush? InlineCodeForeground { get; set; }

        /// <summary>
        ///     Gets or sets the brush used to fill the border of inline code.
        /// </summary>
        public Brush? InlineCodeBorderBrush { get; set; }

        /// <summary>
        ///     Gets or sets the brush used to render the border fill of a code block.
        /// </summary>
        public Brush? CodeBorderBrush { get; set; }

        /// <summary>
        ///     Gets or sets the brush used to render the text inside a code block.  If this is
        ///     <c>null</c>, then <see cref="Foreground" /> is used.
        /// </summary>
        public Brush? CodeForeground { get; set; }

        /// <summary>
        ///     Gets or sets the font used to display code.  If this is <c>null</c>, then
        ///     <see cref="FontFamily" /> is used.
        /// </summary>
        public FontFamily? CodeFontFamily { get; set; }

        /// <summary>
        ///     Gets or sets the font used to display code.  If this is <c>null</c>, then
        ///     <see cref="FontFamily" /> is used.
        /// </summary>
        public FontFamily? InlineCodeFontFamily { get; set; }

        /// <summary>
        ///     Gets or sets the font used to display emojis.  If this is <c>null</c>, then
        ///     Segoe UI Emoji font is used.
        /// </summary>
        public FontFamily? EmojiFontFamily { get; set; }

        /// <summary>
        ///     Gets or sets the font weight to use for level 1 headers.
        /// </summary>
        public FontWeight Header1FontWeight { get; set; }

        /// <summary>
        ///     Gets or sets the foreground brush for level 1 headers.
        /// </summary>
        public Brush? Header1Foreground { get; set; }

        /// <summary>
        ///     Gets or sets the font weight to use for level 2 headers.
        /// </summary>
        public FontWeight Header2FontWeight { get; set; }

        /// <summary>
        ///     Gets or sets the foreground brush for level 2 headers.
        /// </summary>
        public Brush? Header2Foreground { get; set; }

        /// <summary>
        ///     Gets or sets the font weight to use for level 3 headers.
        /// </summary>
        public FontWeight Header3FontWeight { get; set; }

        /// <summary>
        ///     Gets or sets the foreground brush for level 3 headers.
        /// </summary>
        public Brush? Header3Foreground { get; set; }

        /// <summary>
        ///     Gets or sets the font weight to use for level 4 headers.
        /// </summary>
        public FontWeight Header4FontWeight { get; set; }

        /// <summary>
        ///     Gets or sets the foreground brush for level 4 headers.
        /// </summary>
        public Brush? Header4Foreground { get; set; }

        /// <summary>
        ///     Gets or sets the font weight to use for level 5 headers.
        /// </summary>
        public FontWeight Header5FontWeight { get; set; }

        /// <summary>
        ///     Gets or sets the foreground brush for level 5 headers.
        /// </summary>
        public Brush? Header5Foreground { get; set; }

        /// <summary>
        ///     Gets or sets the font weight to use for level 6 headers.
        /// </summary>
        public FontWeight Header6FontWeight { get; set; }

        /// <summary>
        ///     Gets or sets the foreground brush for level 6 headers.
        /// </summary>
        public Brush? Header6Foreground { get; set; }

        /// <summary>
        ///     Gets or sets the brush used to render a horizontal rule.  If this is <c>null</c>, then
        ///     <see cref="Foreground" /> is used.
        /// </summary>
        public Brush? HorizontalRuleBrush { get; set; }

        /// <summary>
        ///     Gets or sets the brush used to fill the background of a quote block.
        /// </summary>
        public Brush? QuoteBackground { get; set; }

        /// <summary>
        ///     Gets or sets the brush used to render a quote border.  If this is <c>null</c>, then
        ///     <see cref="Foreground" /> is used.
        /// </summary>
        public Brush? QuoteBorderBrush { get; set; }

        /// <summary>
        ///     Gets or sets the brush used to render the text inside a quote block.  If this is
        ///     <c>null</c>, then <see cref="Foreground" /> is used.
        /// </summary>
        public Brush? QuoteForeground { get; set; }

        /// <summary>
        ///     Gets or sets the brush used to render table borders.  If this is <c>null</c>, then
        ///     <see cref="Foreground" /> is used.
        /// </summary>
        public Brush? TableBorderBrush { get; set; }

        /// <summary>
        ///     Gets or sets the brush used to render table borders.  If this is <c>null</c>, then
        ///     <see cref="Foreground" /> is used.
        /// </summary>
        public Brush? YamlBorderBrush { get; set; }

        /// <summary>
        ///     Gets or sets the brush used to render links.  If this is <c>null</c>, then
        ///     <see cref="Foreground" /> is used.
        /// </summary>
        public Brush? LinkForeground { get; set; }
    }
}