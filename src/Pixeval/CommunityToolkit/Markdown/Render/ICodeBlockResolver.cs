#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/ICodeBlockResolver.cs
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

using Microsoft.UI.Xaml.Documents;

namespace Pixeval.CommunityToolkit.Markdown.Render
{
    /// <summary>
    ///     A Parser to parse code strings into Syntax Highlighted text.
    /// </summary>
    public interface ICodeBlockResolver
    {
        /// <summary>
        ///     Parses Code Block text into Rich text.
        /// </summary>
        /// <param name="inlineCollection">Block to add formatted Text to.</param>
        /// <param name="text">The raw code block text</param>
        /// <param name="codeLanguage">
        ///     The language of the Code Block, as specified by ```{Language} on the first line of the block,
        ///     e.g.
        ///     <para />
        ///     ```C#
        ///     <para />
        ///     public void Method();
        ///     <para />
        ///     ```
        ///     <para />
        /// </param>
        /// <returns>Parsing was handled Successfully</returns>
        bool ParseSyntax(InlineCollection inlineCollection, string text, string codeLanguage);
    }
}