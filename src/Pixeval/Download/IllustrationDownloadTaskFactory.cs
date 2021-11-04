#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/IllustrationDownloadTaskFactory.cs
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
using Windows.Storage.Streams;
using Pixeval.Download.MacroParser;
using Pixeval.UserControls;
using Pixeval.Util;
using Pixeval.Utilities;

namespace Pixeval.Download
{
    public class IllustrationDownloadTaskFactory : IDownloadTaskFactory<IllustrationViewModel, ObservableDownloadTask>
    {
        public IllustrationDownloadTaskFactory()
        {
            PathParser = new IllustrationMetaPathParser();
        }

        public IMetaPathParser<IllustrationViewModel> PathParser { get; }

        public async Task<ObservableDownloadTask> CreateAsync(IllustrationViewModel context, string rawPath)
        {
            if (context.Illustration.IsUgoira())
            {
                var ugoiraMetadata = await App.AppViewModel.MakoClient.GetUgoiraMetadataAsync(context.Id);
                return ugoiraMetadata.UgoiraMetadataInfo?.ZipUrls?.Medium is { } url
                    ? new AnimatedIllustrationDownloadTask(context, url, PathParser.Reduce(rawPath, context), ugoiraMetadata)
                    : throw new DownloadTaskInitializationException(DownloadTaskResources.GifSourceUrlNotFoundFormatted.Format(context.Id));
            }

            return new IllustrationDownloadTask(context, PathParser.Reduce(rawPath, context));
        }

        /// <summary>
        /// Try to create an <see cref="IntrinsicIllustrationDownloadTask"/>, if the context is of type 'ugoira' or
        /// the <paramref name="imageStream"/> is unreadable, it decays to <see cref="AnimatedIllustrationDownloadTask"/>
        /// </summary>
        public Task<ObservableDownloadTask> TryCreateIntrinsicAsync(IllustrationViewModel context, IRandomAccessStream imageStream, string rawPath)
        {
            return context.Illustration.IsUgoira() || !imageStream.CanRead 
                ? CreateAsync(context, rawPath) 
                : Task.FromResult<ObservableDownloadTask>(new IntrinsicIllustrationDownloadTask(context, imageStream, rawPath));
        }
    }
}