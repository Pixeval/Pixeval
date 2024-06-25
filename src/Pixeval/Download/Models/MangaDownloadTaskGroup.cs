#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/MangaDownloadTaskGroup.cs
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
using System.IO;
using System.Threading.Tasks;
using Pixeval.CoreApi.Model;
using Pixeval.Database;
using Pixeval.Download.Macros;
using Pixeval.Options;
using Pixeval.Util;
using Pixeval.Util.IO;
using WinUI3Utilities;

namespace Pixeval.Download.Models;

public class MangaDownloadTaskGroup : DownloadTaskGroup, IImageDownloadTaskGroup
{
    public Illustration Entry => DatabaseEntry.Entry.To<Illustration>();

    public MangaDownloadTaskGroup(DownloadHistoryEntry entry) : base(entry)
    {
    }

    public MangaDownloadTaskGroup(Illustration entry, string destination, IReadOnlyList<Stream>? streams = null) : base(entry, destination, DownloadItemType.Manga)
    {
        SetTasksSet(streams);
    }

    public override ValueTask InitializeTaskGroupAsync()
    {
        SetTasksSet();
        return ValueTask.CompletedTask;
    }

    private void SetTasksSet(IReadOnlyList<Stream>? streams = null)
    {
        var mangaOriginalUrls = Entry.MangaOriginalUrls;
        for (var i = 0; i < mangaOriginalUrls.Count; ++i)
        {
            var imageDownloadTask = new ImageDownloadTask(new(mangaOriginalUrls[i]), IoHelper.ReplaceTokenExtensionFromUrl(TokenizedDestination, mangaOriginalUrls[i]).Replace(MangaIndexMacro.NameConstToken, i.ToString()))
            {
                Stream = streams?[i]
            };
            AddToTasksSet(imageDownloadTask);
        }
        SetNotCreateFromEntry();
    }

    protected override async Task AfterAllDownloadAsyncOverride(DownloadTaskGroup sender)
    {
        if (App.AppViewModel.AppSettings.IllustrationDownloadFormat is IllustrationDownloadFormat.Original)
            return;
        foreach (var destination in Destinations)
            await TagsManager.SetTagsAsync(destination, Entry);
    }

    public override string OpenLocalDestination => Path.GetDirectoryName(TasksSet[0].Destination)!;

    public override void Delete()
    {
        foreach (var task in TasksSet)
            task.Delete();
        IoHelper.DeleteEmptyFolder(OpenLocalDestination);
    }
}
