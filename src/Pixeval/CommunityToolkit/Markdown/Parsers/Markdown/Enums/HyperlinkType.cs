#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/HyperlinkType.cs
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

// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo

namespace Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Enums
{
    /// <summary>
    ///     Specifies the type of Hyperlink that is used.
    /// </summary>
    public enum HyperlinkType
    {
        /// <summary>
        ///     A hyperlink surrounded by angle brackets (e.g. "http://www.reddit.com").
        /// </summary>
        BracketedUrl,

        /// <summary>
        ///     A fully qualified hyperlink (e.g. "http://www.reddit.com").
        /// </summary>
        FullUrl,

        /// <summary>
        ///     A URL without a scheme (e.g. "www.reddit.com").
        /// </summary>
        PartialUrl,

        /// <summary>
        ///     An email address (e.g. "test@reddit.com").
        /// </summary>
        Email,

        /// <summary>
        ///     A subreddit link (e.g. "/r/news").
        /// </summary>
        Subreddit,

        /// <summary>
        ///     A user link (e.g. "/u/quinbd").
        /// </summary>
        User
    }
}