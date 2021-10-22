#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/ILinkElement.cs
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

namespace Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Inlines
{
    /// <summary>
    ///     Implemented by all inline link elements.
    /// </summary>
    internal interface ILinkElement
    {
        /// <summary>
        ///     Gets the link URL.  This can be a relative URL, but note that subreddit links will always
        ///     have the leading slash (i.e. the Url will be "/r/baconit" even if the text is
        ///     "r/baconit").
        /// </summary>
        string? Url { get; }

        /// <summary>
        ///     Gets a tooltip to display on hover.  Can be <c>null</c>.
        /// </summary>
        string? Tooltip { get; }
    }
}