#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/IImageResolver.cs
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

using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media;

namespace Pixeval.CommunityToolkit.Markdown.Render
{
    /// <summary>
    ///     An interface used to resolve images in the markdown.
    /// </summary>
    public interface IImageResolver
    {
        /// <summary>
        ///     Resolves an Image from a Url.
        /// </summary>
        /// <param name="url">Url to Resolve.</param>
        /// <param name="tooltip">Tooltip for Image.</param>
        /// <returns>Image</returns>
        Task<ImageSource?> ResolveImageAsync(string url, string tooltip);
    }
}