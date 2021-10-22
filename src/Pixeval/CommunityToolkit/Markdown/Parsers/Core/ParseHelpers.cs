#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/ParseHelpers.cs
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

using System.Linq;

namespace Pixeval.CommunityToolkit.Markdown.Parsers.Core
{
    /// <summary>
    ///     This class offers helpers for Parsing.
    /// </summary>
    public static class ParseHelpers
    {
        /// <summary>
        ///     Determines if a Markdown string is blank or comprised entirely of whitespace characters.
        /// </summary>
        /// <returns>true if blank or white space</returns>
        public static bool IsMarkdownBlankOrWhiteSpace(string str)
        {
            return str.All(IsMarkdownWhiteSpace);
        }

        /// <summary>
        ///     Determines if a character is a Markdown whitespace character.
        /// </summary>
        /// <returns>true if is white space</returns>
        public static bool IsMarkdownWhiteSpace(char c)
        {
            return c is ' ' or '\t' or '\r' or '\n';
        }
    }
}