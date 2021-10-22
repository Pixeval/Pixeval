#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/StringValueAttribute.cs
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

namespace Pixeval.CommunityToolkit.Markdown.Parsers.Core
{
    /// <summary>
    ///     The StringValue attribute is used as a helper to decorate enum values with string representations.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class StringValueAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="StringValueAttribute" /> class.
        ///     Constructor accepting string value.
        /// </summary>
        /// <param name="value">String value</param>
        public StringValueAttribute(string value)
        {
            Value = value;
        }

        /// <summary>
        ///     Gets property for string value.
        /// </summary>
        public string Value { get; }
    }
}