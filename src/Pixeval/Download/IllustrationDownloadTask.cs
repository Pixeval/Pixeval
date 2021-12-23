#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/IllustrationDownloadTask.cs
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
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Pixeval.Database;
using Pixeval.UserControls;
using Pixeval.Util;
using Pixeval.Util.IO;

namespace Pixeval.Download;

public class IllustrationDownloadTask : ObservableDownloadTask, IIllustrationViewModelProvider
{
    public IllustrationDownloadTask(DownloadHistoryEntry dataBaseEntry, IllustrationViewModel illustrationViewModel) : base(dataBaseEntry)
    {
        IllustrationViewModel = illustrationViewModel;
        CurrentState = DownloadState.Created;
    }

    public IllustrationViewModel IllustrationViewModel { get; }

    public Task<IllustrationViewModel> GetViewModelAsync()
    {
        return Task.FromResult(IllustrationViewModel);
    }

    public override async void DownloadStarting(DownloadStartingEventArgs args)
    {
        var deferral = args.GetDeferral();
        if (!App.AppViewModel.AppSetting.OverwriteDownloadedFile && File.Exists(Destination))
        {
            ProgressPercentage = 100;
            CurrentState = DownloadState.Completed;
            deferral.Complete(false);
            return;
        }

        if (App.AppViewModel.AppSetting.UseFileCache && await App.AppViewModel.Cache.TryGetAsync<IRandomAccessStream>(IllustrationViewModel.Illustration.GetIllustrationOriginalImageCacheKey()) is { } stream)
        {
            // fast path
            deferral.Complete(false);
            ProgressPercentage = 100;
            try
            {
                using (stream)
                {
                    IOHelper.CreateParentDirectories(Destination);
                    await using var fs = File.Open(Destination, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
                    await stream.AsStreamForRead().CopyToAsync(fs);
                }
            }
            catch (Exception e)
            {
                CurrentState = DownloadState.Error;
                ErrorCause = e;
                return;
            }

            CurrentState = DownloadState.Completed;
        }

        // slow path
        deferral.Complete(true);
    }
}