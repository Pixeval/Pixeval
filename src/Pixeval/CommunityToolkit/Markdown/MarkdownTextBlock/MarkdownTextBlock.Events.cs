#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/MarkdownTextBlock.Events.cs
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
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;

namespace Pixeval.CommunityToolkit.Markdown.MarkdownTextBlock
{
    /// <summary>
    ///     An efficient and extensible control that can parse and render markdown.
    /// </summary>
    public partial class MarkdownTextBlock
    {
        /// <summary>
        ///     Calls OnPropertyChanged.
        /// </summary>
        private static void OnPropertyChangedStatic(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var instance = d as MarkdownTextBlock;

            // Defer to the instance method.
            instance?.OnPropertyChanged(d, e.Property);
        }

        /// <summary>
        ///     Fired when the value of a DependencyProperty is changed.
        /// </summary>
        private void OnPropertyChanged(DependencyObject d, DependencyProperty prop)
        {
            RenderMarkdown();
        }

        /// <summary>
        ///     Fired when a user taps one of the link elements
        /// </summary>
        private void Hyperlink_Click(DependencyObject sender, HyperlinkClickEventArgs args)
        {
            LinkHandled((string) sender.GetValue(HyperlinkUrlProperty), true);
        }

        /// <summary>
        ///     Fired when a user taps one of the image elements
        /// </summary>
        // ReSharper disable once IdentifierTypo
        private void NewImagelink_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var hyperLink = (string) (sender as Image)?.GetValue(HyperlinkUrlProperty)!;
            var isHyperLink = (bool) (sender as Image)?.GetValue(IsHyperlinkProperty)!;
            LinkHandled(hyperLink, isHyperLink);
        }

        /// <summary>
        ///     Fired when the text is done parsing and formatting. Fires each time the markdown is rendered.
        /// </summary>
        public event EventHandler<MarkdownRenderedEventArgs>? MarkdownRendered;

        /// <summary>
        ///     Fired when a link element in the markdown was tapped.
        /// </summary>
        public event EventHandler<LinkClickedEventArgs>? LinkClicked;

        /// <summary>
        ///     Fired when an image element in the markdown was tapped.
        /// </summary>
        public event EventHandler<LinkClickedEventArgs>? ImageClicked;

        /// <summary>
        ///     Fired when an image from the markdown document needs to be resolved.
        ///     The default implementation is basically <code>new BitmapImage(new Uri(e.Url));</code>.
        ///     <para />
        ///     You must set <see cref="ImageResolvingEventArgs.Handled" /> to true in order to process your changes.
        /// </summary>
        public event EventHandler<ImageResolvingEventArgs>? ImageResolving;

        /// <summary>
        ///     Fired when a Code Block is being Rendered.
        ///     The default implementation is to output the CodeBlock as Plain Text.
        ///     <para />
        ///     You must set <see cref="CodeBlockResolvingEventArgs.Handled" /> to true in order to process your changes.
        /// </summary>
#pragma warning disable CS0067
        public event EventHandler<CodeBlockResolvingEventArgs>? CodeBlockResolving;
#pragma warning restore CS0067
    }
}