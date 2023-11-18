#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IOHelper.Mako.cs
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
using Pixeval.CoreApi;
using Pixeval.CoreApi.Net;
using Pixeval.Utilities;

namespace Pixeval.Util.IO;

public static partial class IoHelper
{
    public static async Task<Result<ImageSource>> DownloadSoftwareBitmapSourceResultAsync(this MakoClient client, string url)
    {
        return await (await client.GetMakoHttpClient(MakoApiKind.ImageApi).DownloadAsIRandomAccessStreamAsync(url))
            .BindAsync(async m => (ImageSource)await m.GetSoftwareBitmapSourceAsync(true));
    }

    public static async Task<Result<ImageSource>> DownloadBitmapImageResultAsync(this MakoClient client, string url, int? desiredWidth)
    {
        return await (await client.GetMakoHttpClient(MakoApiKind.ImageApi).DownloadAsIRandomAccessStreamAsync(url))
            .BindAsync(async m => (ImageSource)await m.GetBitmapImageAsync(true, desiredWidth));
    }
}