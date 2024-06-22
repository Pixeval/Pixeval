#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/INovelParserViewModel.cs
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
using Pixeval.CoreApi.Model;

namespace Pixeval.Controls;

public interface INovelParserViewModel<TImage> : IDisposable
{
    NovelContent NovelContent { get; } 

    Dictionary<(long, int), NovelIllustInfo> IllustrationLookup { get; }

    Dictionary<(long, int), TImage> IllustrationImages { get; }

    Dictionary<long, TImage> UploadedImages { get; }

    /// <summary>
    /// 此处默认所有图片扩展名都相同
    /// </summary>
    /// <remarks>
    /// 包含前缀点，例如".png"
    /// </remarks>
    public string? ImageExtension { get; }
}
