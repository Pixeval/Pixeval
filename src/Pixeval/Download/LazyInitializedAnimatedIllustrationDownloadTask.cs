#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/LazyInitializedAnimatedIllustrationDownloadTask.cs
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
using Pixeval.Util.IO;
using IllustrationViewModel = Pixeval.UserControls.IllustrationView.IllustrationViewModel;

namespace Pixeval.Download;

public class LazyInitializedAnimatedIllustrationDownloadTask : AnimatedIllustrationDownloadTask
{
    private readonly string _illustId;

    private readonly Lazy<Task<IllustrationViewModel>> _resultGenerator;

    public LazyInitializedAnimatedIllustrationDownloadTask(DownloadHistoryEntry databaseEntry) : base(databaseEntry)
    {
        _illustId = databaseEntry.Id!;
        _resultGenerator = new Lazy<Task<IllustrationViewModel>>(async () => new IllustrationViewModel(await App.AppViewModel.MakoClient.GetIllustrationFromIdAsync(_illustId)));
    }

    public override async void Consume(IRandomAccessStream stream)
    {
        using (stream)
        {
            var metadata = await App.AppViewModel.MakoClient.GetUgoiraMetadataAsync(_illustId);
            using var gifStream = await IOHelper.GetGifStreamFromZipStreamAsync(stream.AsStreamForRead(), metadata);
            await IOHelper.CreateAndWriteToFileAsync(gifStream, Destination);
        }
    }

    public override Task<IllustrationViewModel> GetViewModelAsync()
    {
        return _resultGenerator.Value;
    }
}