#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.

// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019-2020 Dylech30th
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Pixeval.Core;
using Pixeval.Data.Web.Delegation;
using Pixeval.Objects.Generic;
using Pixeval.Objects.Primitive;
using PropertyChanged;

namespace Pixeval.Data.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class DownloadableIllustration
    {
        [DoNotNotify]
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private bool modifiable = true;

        private bool retried;

        public DownloadableIllustration(Illustration downloadContent, IIllustrationFileNameFormatter fileNameFormatter, IDownloadPathProvider downloadPathProvider, bool isFromManga, int mangaIndex = -1)
        {
            DownloadContent = downloadContent;
            FileNameFormatter = fileNameFormatter;
            DownloadPathProvider = downloadPathProvider;
            IsFromManga = isFromManga;
            MangaIndex = mangaIndex;
        }

        public Illustration DownloadContent { get; set; }

        public IIllustrationFileNameFormatter FileNameFormatter { get; set; }

        public IDownloadPathProvider DownloadPathProvider { get; set; }

        public bool IsFromManga { get; set; }

        public int MangaIndex { get; set; }

        public bool DownloadFailed { get; set; }

        public double Progress { get; set; }

        public string ReasonPhase { get; set; }

        public Observable<DownloadStateEnum> DownloadState { get; set; } = new Observable<DownloadStateEnum>(DownloadStateEnum.Queue);

        public string RootDirectory { get; set; }

        public string GetPath()
        {
            if (DownloadContent.IsUgoira)
                return Path.Combine(Directory.CreateDirectory(RootDirectory ?? DownloadPathProvider.GetIllustrationPath()).FullName, FileNameFormatter.FormatGif(DownloadContent));
            if (DownloadContent.FromSpotlight)
                return IsFromManga
                    ? Path.Combine(Directory.CreateDirectory(RootDirectory ?? DownloadPathProvider.GetSpotlightPath(DownloadContent.SpotlightTitle)).FullName, DownloadContent.Id, FileNameFormatter.FormatManga(DownloadContent, MangaIndex))
                    : Path.Combine(Directory.CreateDirectory(RootDirectory ?? DownloadPathProvider.GetSpotlightPath(DownloadContent.SpotlightTitle)).FullName, FileNameFormatter.Format(DownloadContent));
            if (IsFromManga)
                return Path.Combine(
                    Directory.CreateDirectory(RootDirectory ?? DownloadPathProvider.GetMangaPath(DownloadContent.Id)).FullName, FileNameFormatter.FormatManga(DownloadContent, MangaIndex));
            return Path.Combine(Directory.CreateDirectory(RootDirectory ?? DownloadPathProvider.GetIllustrationPath()).FullName, FileNameFormatter.Format(DownloadContent));
        }


        public void Freeze()
        {
            modifiable = false;
        }

        public void Cancel()
        {
            if (modifiable)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource = new CancellationTokenSource();
                Progress = 0;
                ReasonPhase = null;
                DownloadFailed = false;
                DownloadState.Value = DownloadStateEnum.Canceled;
            }
        }

        public void Restart()
        {
            if (modifiable)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource = new CancellationTokenSource();
                Progress = 0;
                ReasonPhase = null;
                DownloadFailed = false;
                Download();
            }
        }

        public async void Download()
        {
            if (!modifiable) return;

            DownloadState.Value = DownloadStateEnum.Downloading;
            var downloadPath = GetPath();
            if (DownloadContent.IsUgoira)
            {
                DownloadGif();
                return;
            }

            try
            {
                await using var memory = await PixivIO.Download(DownloadContent.GetDownloadUrl(),
                                                                new Progress<double>(d => Progress = d),
                                                                cancellationTokenSource.Token);
                if (cancellationTokenSource.IsCancellationRequested) return;
                await using var fileStream = new FileStream(downloadPath, FileMode.OpenOrCreate, FileAccess.ReadWrite,
                                                            FileShare.None);
                memory.WriteTo(fileStream);
                DownloadState.Value = DownloadStateEnum.Finished;
            }
            catch (OperationCanceledException)
            {
                if (downloadPath != null && File.Exists(downloadPath)) File.Delete(downloadPath);
            }
            catch (Exception e)
            {
                if (!retried)
                {
                    Restart();
                    retried = true;
                }
                else
                {
                    HandleError(e, downloadPath);
                }
            }
        }

        private async void DownloadGif()
        {
            var downloadPath = GetPath();
            try
            {
                var metadata = await HttpClientFactory.AppApiService().GetUgoiraMetadata(DownloadContent.Id);
                var ugoiraUrl = metadata.UgoiraMetadataInfo.ZipUrls.Medium;
                ugoiraUrl = !ugoiraUrl.EndsWith("ugoira1920x1080.zip")
                    ? Regex.Replace(ugoiraUrl, "ugoira(\\d+)x(\\d+).zip", "ugoira1920x1080.zip")
                    : ugoiraUrl;
                var delay = metadata.UgoiraMetadataInfo.Frames.Select(f => f.Delay / 10).ToArray();
                if (cancellationTokenSource.IsCancellationRequested) return;
                await using var memory = await PixivIO.Download(ugoiraUrl, new Progress<double>(d => Progress = d),
                                                                cancellationTokenSource.Token);
                await using var gifStream =
                    (MemoryStream) InternalIO.MergeGifStream(InternalIO.ReadGifZipEntries(memory), delay);
                if (cancellationTokenSource.IsCancellationRequested) return;
                await using var fileStream = new FileStream(downloadPath, FileMode.OpenOrCreate, FileAccess.ReadWrite,
                                                            FileShare.None);
                gifStream.WriteTo(fileStream);
                DownloadState.Value = DownloadStateEnum.Finished;
            }
            catch (TaskCanceledException)
            {
                if (downloadPath != null && File.Exists(downloadPath)) File.Delete(downloadPath);
            }
            catch (Exception e)
            {
                if (!retried)
                {
                    Restart();
                    retried = true;
                }
                else
                {
                    HandleError(e, downloadPath);
                }
            }
        }

        private void HandleError(Exception e, string path)
        {
            DownloadState.Value = DownloadStateEnum.Exceptional;
            DownloadFailed = true;
            ReasonPhase = e.Message;
            if (path != null && File.Exists(path)) File.Delete(path);
        }
    }

    [Flags]
    public enum DownloadStateEnum
    {
        Queue,
        Downloading,
        Exceptional,
        Finished,
        Canceled
    }
}