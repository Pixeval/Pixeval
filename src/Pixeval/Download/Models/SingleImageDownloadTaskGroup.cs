#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/SingleImageDownloadTaskGroup.cs
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

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.CoreApi.Model;
using Pixeval.Database;
using Pixeval.Database.Managers;
using Pixeval.Options;
using Pixeval.Util;
using Pixeval.Util.IO;
using WinUI3Utilities;

namespace Pixeval.Download.Models;

public class SingleImageDownloadTaskGroup : ImageDownloadTask, IImageDownloadTaskGroup
{
    public DownloadHistoryEntry DatabaseEntry { get; }

    public ValueTask InitializeTaskGroupAsync() => ValueTask.CompletedTask;

    public Illustration Entry => DatabaseEntry.Entry.To<Illustration>();

    public long Id => DatabaseEntry.Entry.Id;

    public SingleImageDownloadTaskGroup(Illustration entry, string destination, Stream? stream = null) : this(new(destination, DownloadItemType.Illustration, entry))
    {
        CurrentState = DownloadState.Queued;
        ProgressPercentage = 0;
        Stream = stream;
        SetNotCreateFromEntry();
    }

    public SingleImageDownloadTaskGroup(DownloadHistoryEntry entry) : base(new(entry.Entry.To<Illustration>().OriginalSingleUrl!),
        new(IoHelper.ReplaceTokenExtensionFromUrl(entry.Destination,
            entry.Entry.To<Illustration>().OriginalSingleUrl!)))
    {
        DatabaseEntry = entry;
        CurrentState = entry.State;
        IllustrationDownloadFormat = IoHelper.GetIllustrationFormat(Path.GetExtension(Destination));
    }

    private void SetNotCreateFromEntry()
    {
        if (!IsCreateFromEntry)
            return;
        IsCreateFromEntry = false;
        PropertyChanged += (sender, e) =>
        {
            var g = sender.To<SingleImageDownloadTaskGroup>();
            if (e.PropertyName is not nameof(CurrentState))
                return;
            if (g.CurrentState is DownloadState.Running or DownloadState.Paused)
                return;
            g.DatabaseEntry.State = g.CurrentState;
            var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<DownloadHistoryPersistentManager>();
            manager.Update(g.DatabaseEntry);
        };
    }

    private bool IsCreateFromEntry { get; set; } = true;

    private IllustrationDownloadFormat IllustrationDownloadFormat { get; }

    protected override async Task AfterDownloadAsyncOverride(ImageDownloadTask sender, CancellationToken token = default)
    {
        if (IllustrationDownloadFormat is IllustrationDownloadFormat.Original)
            return;
        await TagsManager.SetTagsAsync(Destination, Entry, token);
    }

    public DownloadToken GetToken() => new(this, CancellationTokenSource.Token);

    public void SubscribeProgress(ChannelWriter<DownloadToken> writer)
    {
        DownloadTryResume += OnDownloadWrite;
        DownloadTryReset += OnDownloadWrite;

        return;
        void OnDownloadWrite(ImageDownloadTask o) => writer.TryWrite(o.To<SingleImageDownloadTaskGroup>().GetToken());
    }

    public int Count => 1;

    public IEnumerator<ImageDownloadTask> GetEnumerator() => ((IReadOnlyList<ImageDownloadTask>)[this]).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
