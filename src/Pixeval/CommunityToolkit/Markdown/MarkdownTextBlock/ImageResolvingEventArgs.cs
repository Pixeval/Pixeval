#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/ImageResolvingEventArgs.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.UI.Xaml.Media;

namespace Pixeval.CommunityToolkit.Markdown.MarkdownTextBlock
{
    /// <summary>
    ///     Arguments for the <see cref="MarkdownTextBlock.ImageResolving" /> event which is called when a url needs to be
    ///     resolved to a <see cref="ImageSource" />.
    /// </summary>
    public class ImageResolvingEventArgs : EventArgs
    {
        private readonly IList<TaskCompletionSource<object?>>? _deferrals;

        internal ImageResolvingEventArgs(string url, string tooltip)
        {
            _deferrals = new List<TaskCompletionSource<object?>>();
            Url = url;
            Tooltip = tooltip;
        }

        /// <summary>
        ///     Gets the url of the image in the markdown document.
        /// </summary>
        public string Url { get; }

        /// <summary>
        ///     Gets the tooltip of the image in the markdown document.
        /// </summary>
        public string Tooltip { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether this event was handled successfully.
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        ///     Gets or sets the image to display in the <see cref="MarkdownTextBlock" />.
        /// </summary>
        public ImageSource? Image { get; set; }

        /// <summary>
        ///     Informs the <see cref="MarkdownTextBlock" /> that the event handler might run asynchronously.
        /// </summary>
        /// <returns>Deferral</returns>
        public Deferral GetDeferral()
        {
            var task = new TaskCompletionSource<object?>();
            _deferrals?.Add(task);

            return new Deferral(() =>
            {
                task.SetResult(null);
            });
        }

        /// <summary>
        ///     Returns a <see cref="Task" /> that completes when all <see cref="Deferral" />s have completed.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        internal Task WaitForDeferrals()
        {
            return Task.WhenAll(_deferrals!.Select(f => f.Task));
        }
    }
}