#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/AnimatedIllustrationDownloadTask.cs
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
using Windows.Storage.Streams;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Database;
using Pixeval.UserControls;
using Pixeval.Util.IO;

namespace Pixeval.Download;

public class AnimatedIllustrationDownloadTask : ObservableDownloadTask, ICustomBehaviorDownloadTask, IIllustrationViewModelProvider
{
    private readonly IllustrationViewModel _illustration;
    private readonly UgoiraMetadataResponse _metadata;

    public AnimatedIllustrationDownloadTask(
        DownloadHistoryEntry databaseEntry,
        IllustrationViewModel illustration,
        UgoiraMetadataResponse metadata) : base(databaseEntry)
    {
        _illustration = illustration;
        _metadata = metadata;
    }

    protected AnimatedIllustrationDownloadTask(DownloadHistoryEntry databaseEntry)
        : base(databaseEntry)
    {
        // derived classes won't need them
        _metadata = null!;
        _illustration = null!;
    }

    public override void DownloadStarting(DownloadStartingEventArgs args)
    {
        args.GetDeferral().Complete(App.AppViewModel.AppSetting.OverwriteDownloadedFile || !File.Exists(Destination));
    }

    public virtual async void Consume(IRandomAccessStream stream)
    {
        using (stream)
        {
            using var gifStream = await IOHelper.GetGifStreamFromZipStreamAsync(stream.AsStreamForRead(), _metadata);
            await IOHelper.CreateAndWriteToFileAsync(gifStream, Destination);
        }
    }

    public virtual Task<IllustrationViewModel> GetViewModelAsync()
    {
        return Task.FromResult(_illustration);
    }
}