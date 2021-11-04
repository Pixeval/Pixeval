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
using Windows.Storage.Streams;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Options;
using Pixeval.UserControls;
using Pixeval.Util;
using Pixeval.Util.IO;

namespace Pixeval.Download
{
    public class AnimatedIllustrationDownloadTask : ObservableDownloadTask, ICustomBehaviorDownloadTask
    {
        private readonly UgoiraMetadataResponse _metadata;

        public AnimatedIllustrationDownloadTask(
            IllustrationViewModel illustration,
            string zipUrl,
            string destination,
            UgoiraMetadataResponse metadata) : base(illustration.Illustration.Title, illustration.Illustration.User?.Name, zipUrl, IOHelper.NormalizePath(destination), illustration.Illustration.GetThumbnailUrl(ThumbnailUrlOption.SquareMedium), null)
        {
            _metadata = metadata;
        }

        public override void DownloadStarting(DownloadStartingEventArgs args)
        {
            var deferral = args.GetDeferral();
            if (!App.AppViewModel.AppSetting.OverwriteDownloadedFile && File.Exists(Destination))
            {
                deferral.Complete(false);
            }
        }

        public async void Consume(IRandomAccessStream stream)
        {
            using (stream)
            {
                using var gifStream = await IOHelper.GetGifStreamFromZipStreamAsync(stream.AsStreamForRead(), _metadata);
                await IOHelper.CreateAndWriteToFileAsync(gifStream, Destination);
            }
        }
    }
}