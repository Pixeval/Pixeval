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
        [DoNotNotify] private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private bool modifiable = true;

        private bool retried;

        public DownloadableIllustration(Illustration downloadContent, bool isFromManga, int mangaIndex = -1)
        {
            DownloadContent = downloadContent;
            IsFromManga = isFromManga;
            MangaIndex = mangaIndex;
            GetPath();
        }

        public Illustration DownloadContent { get; set; }

        public bool IsFromManga { get; set; }

        public int MangaIndex { get; set; }

        public bool DownloadFailed { get; set; }

        public double Progress { get; set; }

        public string ReasonPhase { get; set; }

        public Observable<DownloadStatEnum> DownloadStat { get; set; } =
            new Observable<DownloadStatEnum>(DownloadStatEnum.Queue);

        public string DownloadPath { get; private set; }

        private void GetPath()
        {
            if (DownloadContent.IsUgoira)
                DownloadPath =
                    Path.Combine(
                        Directory.CreateDirectory(DownloadManager.DownloadPathProvider.GetIllustrationPath()).FullName,
                        DownloadManager.FileNameFormatter.FormatGif(DownloadContent));
            else if (DownloadContent.FromSpotlight)
                DownloadPath = IsFromManga
                    ? Path.Combine(
                        Directory.CreateDirectory(
                                DownloadManager.DownloadPathProvider.GetSpotlightPath(DownloadContent.SpotlightTitle))
                            .FullName, DownloadContent.Id,
                        DownloadManager.FileNameFormatter.FormatManga(DownloadContent, MangaIndex))
                    : Path.Combine(
                        Directory.CreateDirectory(
                                DownloadManager.DownloadPathProvider.GetSpotlightPath(DownloadContent.SpotlightTitle))
                            .FullName, DownloadManager.FileNameFormatter.Format(DownloadContent));
            else if (IsFromManga)
                DownloadPath =
                    Path.Combine(
                        Directory.CreateDirectory(DownloadManager.DownloadPathProvider.GetMangaPath(DownloadContent.Id))
                            .FullName, DownloadManager.FileNameFormatter.FormatManga(DownloadContent, MangaIndex));
            else
                DownloadPath =
                    Path.Combine(
                        Directory.CreateDirectory(DownloadManager.DownloadPathProvider.GetIllustrationPath()).FullName,
                        DownloadManager.FileNameFormatter.Format(DownloadContent));
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
                DownloadStat.Value = DownloadStatEnum.Canceled;
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

            DownloadStat.Value = DownloadStatEnum.Downloading;
            if (DownloadContent.IsUgoira)
            {
                DownloadGif();
                return;
            }

            try
            {
                await using var memory = await PixivIO.Download(DownloadContent.GetDownloadUrl(),
                    new Progress<double>(d => Progress = d), cancellationTokenSource.Token);
                if (cancellationTokenSource.IsCancellationRequested) return;
                await using var fileStream = new FileStream(DownloadPath, FileMode.OpenOrCreate, FileAccess.ReadWrite,
                    FileShare.None);
                memory.WriteTo(fileStream);
                DownloadStat.Value = DownloadStatEnum.Finished;
            }
            catch (OperationCanceledException)
            {
                if (DownloadPath != null && File.Exists(DownloadPath)) File.Delete(DownloadPath);
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
                    HandleError(e, DownloadPath);
                }
            }
        }

        private async void DownloadGif()
        {
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
                await using var fileStream = new FileStream(DownloadPath, FileMode.OpenOrCreate, FileAccess.ReadWrite,
                    FileShare.None);
                gifStream.WriteTo(fileStream);
                DownloadStat.Value = DownloadStatEnum.Finished;
            }
            catch (TaskCanceledException)
            {
                if (DownloadPath != null && File.Exists(DownloadPath)) File.Delete(DownloadPath);
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
                    HandleError(e, DownloadPath);
                }
            }
        }

        private void HandleError(Exception e, string path)
        {
            DownloadStat.Value = DownloadStatEnum.Exceptional;
            DownloadFailed = true;
            ReasonPhase = e.Message;
            if (path != null && File.Exists(path)) File.Delete(path);
        }
    }

    [Flags]
    public enum DownloadStatEnum
    {
        Queue,
        Downloading,
        Exceptional,
        Finished,
        Canceled
    }
}