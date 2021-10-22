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
using Pixeval.Download.MacroParser;
using Pixeval.UserControls;
using Pixeval.Util;
using Pixeval.Utilities;

namespace Pixeval.Download
{
    public class IllustrationDownloadTaskFactory : IDownloadTaskFactory<IllustrationViewModel>
    {
        public IllustrationDownloadTaskFactory()
        {
            PathParser = new IllustrationMetaPathParser();
        }

        public IMetaPathParser<IllustrationViewModel> PathParser { get; }

        public async Task<IDownloadTask> CreateAsync(IllustrationViewModel context, string rawPath)
        {
            if (context.Illustration.IsUgoira())
            {
                var ugoiraMetadata = await App.AppViewModel.MakoClient.GetUgoiraMetadataAsync(context.Id);
                return ugoiraMetadata.UgoiraMetadataInfo?.ZipUrls?.Medium is { } url
                    ? new AnimatedIllustrationDownloadTask(url, PathParser.Reduce(rawPath, context), ugoiraMetadata)
                    : throw new DownloadTaskInitializationException(DownloadTaskResources.GifSourceUrlNotFoundFormatted.Format(context.Id));
            }

            return new ObservableDownloadTask(context.Illustration.GetOriginalUrl()!, PathParser.Reduce(rawPath, context));
        }
    }
}