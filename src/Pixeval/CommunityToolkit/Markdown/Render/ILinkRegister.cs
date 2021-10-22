#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/ILinkRegister.cs
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

// ReSharper disable IdentifierTypo

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;

namespace Pixeval.CommunityToolkit.Markdown.Render
{
    /// <summary>
    ///     An interface used to handle links in the markdown.
    /// </summary>
    public interface ILinkRegister
    {
        /// <summary>
        ///     Registers a Hyperlink with a LinkUrl.
        /// </summary>
        /// <param name="newHyperlink">Hyperlink to Register.</param>
        /// <param name="linkUrl">Url to Register.</param>
        void RegisterNewHyperLink(Hyperlink newHyperlink, string linkUrl);

        /// <summary>
        ///     Registers a Hyperlink with a LinkUrl.
        /// </summary>
        /// <param name="newImagelink">ImageLink to Register.</param>
        /// <param name="linkUrl">Url to Register.</param>
        /// <param name="isHyperLink">Is Image an IsHyperlink.</param>
        void RegisterNewHyperLink(Image newImagelink, string linkUrl, bool isHyperLink);
    }
}