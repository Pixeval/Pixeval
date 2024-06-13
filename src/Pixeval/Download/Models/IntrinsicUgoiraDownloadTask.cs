#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/IntrinsicAnimatedIllustrationDownloadTask.cs
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

using System.IO;
using System.Threading.Tasks;
using Pixeval.Controls;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Database;
using SixLabors.ImageSharp;

namespace Pixeval.Download.Models;

public sealed partial class IntrinsicUgoiraDownloadTask : UgoiraDownloadTask
{
    /// <summary>
    /// The disposal of <paramref name="stream" /> is not handled
    /// </summary>
    public IntrinsicUgoiraDownloadTask(DownloadHistoryEntry entry, IllustrationItemViewModel illustrationViewModel, UgoiraMetadataResponse metadata, Stream stream) : base(entry, illustrationViewModel, metadata)
    {
        Report(100);
        Stream = stream;
    }

    public Stream Stream { get; }

    public override async Task DownloadAsync(Downloader downloadStreamAsync)
    {
        var actualDestination = ActualDestination;

        if (!ShouldOverwrite(actualDestination))
            return;

        using var image = await Image.LoadAsync(Stream);
        await ManageImageAsync(image, actualDestination);
    }
}
