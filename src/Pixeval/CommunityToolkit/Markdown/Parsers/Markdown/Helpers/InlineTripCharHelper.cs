#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/InlineTripCharHelper.cs
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

using Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Enums;

namespace Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Helpers
{
    /// <summary>
    ///     A helper class for the trip chars. This is an optimization. If we ask each class to go
    ///     through the rage and look for itself we end up looping through the range n times, once
    ///     for each inline. This class represent a character that an inline needs to have a
    ///     possible match. We will go through the range once and look for everyone's trip chars,
    ///     and if they can make a match from the trip char then we will commit to them.
    /// </summary>
    internal class InlineTripCharHelper
    {
        // Note! Everything in first char and suffix should be lower case!
        public char FirstChar { get; set; }

        public InlineParseMethod Method { get; set; }
    }
}