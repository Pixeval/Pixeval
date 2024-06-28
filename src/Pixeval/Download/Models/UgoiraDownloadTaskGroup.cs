#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/UgoiraDownloadTaskGroup.cs
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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Database;
using Pixeval.Options;
using Pixeval.Util;
using Pixeval.Util.IO;
using SixLabors.ImageSharp;
using WinUI3Utilities;

namespace Pixeval.Download.Models;

public class UgoiraDownloadTaskGroup : DownloadTaskGroup, IImageDownloadTaskGroup
{
    public Illustration Entry => DatabaseEntry.Entry.To<Illustration>();

    private UgoiraMetadataResponse Metadata { get; set; } = null!;

    private string TempFolderPath => $"{TokenizedDestination}.tmp";

    [MemberNotNull(nameof(Metadata))]
    private void SetMetadata(UgoiraMetadataResponse metadata, IReadOnlyList<Stream>? streams = null)
    {
        Metadata = metadata;
        var ugoiraOriginalUrls = Entry.GetUgoiraOriginalUrls(Metadata.FrameCount);
        _ = Directory.CreateDirectory(TempFolderPath);
        for (var i = 0; i < ugoiraOriginalUrls.Count; ++i)
        {
            var imageDownloadTask = new ImageDownloadTask(new(ugoiraOriginalUrls[i]), $"{TempFolderPath}\\{i}{Path.GetExtension(ugoiraOriginalUrls[i])}")
            {
                Stream = streams?[i]
            };
            AddToTasksSet(imageDownloadTask);
        }
        SetNotCreateFromEntry();
    }

    public UgoiraDownloadTaskGroup(DownloadHistoryEntry entry) : base(entry)
    {
    }

    public UgoiraDownloadTaskGroup(Illustration entry, UgoiraMetadataResponse metadata, string destination, IReadOnlyList<Stream>? streams = null) : base(entry, destination, DownloadItemType.Ugoira) => SetMetadata(metadata, streams);

    public UgoiraDownloadTaskGroup(Illustration entry, string destination) : base(entry, destination,
        DownloadItemType.Ugoira)
    {
    }

    public override async ValueTask InitializeTaskGroupAsync()
    {
        if (Metadata == null!)
            SetMetadata(await App.AppViewModel.MakoClient.GetUgoiraMetadataAsync(Entry.Id));
    }

    protected override async Task AfterAllDownloadAsyncOverride(DownloadTaskGroup sender)
    {
        await Task.Run(async () =>
        {
            if (App.AppViewModel.AppSettings.UgoiraDownloadFormat is UgoiraDownloadFormat.OriginalZip)
            {
                ZipFile.CreateFromDirectory(TempFolderPath, TokenizedDestination, CompressionLevel.Optimal, false);
            }
            else
            {
                using var image = await Destinations.UgoiraSaveToImageAsync(Metadata.Delays.ToArray());
                image.SetIdTags(Entry);
                await image.SaveAsync(TokenizedDestination, IoHelper.GetUgoiraEncoder());
            }
            foreach (var imageDownloadTask in TasksSet)
                imageDownloadTask.Delete();
            IoHelper.DeleteEmptyFolder(TempFolderPath);
        });
    }

    public override string OpenLocalDestination => TokenizedDestination;

    public override void Delete()
    {
        foreach (var task in TasksSet)
            task.Delete();
        IoHelper.DeleteEmptyFolder(TempFolderPath);
        if (File.Exists(TokenizedDestination))
            File.Delete(TokenizedDestination);
    }
}
