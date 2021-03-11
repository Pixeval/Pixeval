#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.
// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019-2020 Dylech30th
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using MaterialDesignExtensions.Model;
using Pixeval.Core;

namespace Pixeval.Objects.SuggestionProvider
{
    public class DownloadPathSuggestionProvider : TextBoxSuggestionsSource
    {
        public static DownloadPathSuggestionProvider Instance = new DownloadPathSuggestionProvider();
        
        public static Lazy<IReadOnlyList<string>> Macros = new Lazy<IReadOnlyList<string>>(DownloadPathMacros.GetMacros);
        
        /// <summary>
        /// Find the nearest left curly brace relative to the end of <see cref="searchTerm"/>
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        public override IEnumerable<string> Search(string searchTerm)
        {
            return Macros.Value;
        }
    }
}