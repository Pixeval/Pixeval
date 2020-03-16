// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019 Dylech30th
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

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;
using Pixeval.Core;
using Pixeval.Data.Web.Delegation;
using PropertyChanged;

namespace Pixeval.Data.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class DownloadableIllustrationViewModel
    {
        [DoNotNotify]
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private bool retried;

        public DownloadableIllustrationViewModel(Illustration downloadContent, bool isFromManga, int mangaIndex = -1)
        {
            DownloadContent = downloadContent;
            IsFromManga = isFromManga;
            MangaIndex = mangaIndex;
        }

        public Illustration DownloadContent { get; set; }

        public bool IsFromManga { get; set; }

        public int MangaIndex { get; set; }

        public bool DownloadFailed { get; set; }

        public double Progress { get; set; }

        public string ReasonPhase { get; set; }

        [DoNotNotify]
        public Action<DownloadableIllustrationViewModel> DownloadFinished { get; set; }

        public void Cancel()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource = new CancellationTokenSource();
        }

        public void Restart()
        {
            Progress = 0;
            ReasonPhase = null;
            DownloadFailed = false;
            Cancel();
            Download();
        }


        // 3/10/2020 I wish that we could both be there
        public async void Download()
        {
            if (DownloadContent.IsUgoira)
            {
                DownloadGif();
                return;
            }

            string path = null;
            try
            {
                await using var memory = await PixivIO.Download(DownloadContent.GetDownloadUrl(), new Progress<double>(d => Progress = d), cancellationTokenSource.Token);
                if (DownloadContent.FromSpotlight)
                    path = IsFromManga
                        ? Path.Combine(Directory.CreateDirectory(AppContext.DownloadPathProvider.GetSpotlightPath(DownloadContent.SpotlightTitle)).FullName, DownloadContent.Id, AppContext.FileNameFormatter.FormatManga(DownloadContent, MangaIndex))
                        : Path.Combine(Directory.CreateDirectory(AppContext.DownloadPathProvider.GetSpotlightPath(DownloadContent.SpotlightTitle)).FullName, AppContext.FileNameFormatter.Format(DownloadContent));
                else if (IsFromManga)
                    path = Path.Combine(Directory.CreateDirectory(AppContext.DownloadPathProvider.GetMangaPath(DownloadContent.Id)).FullName, AppContext.FileNameFormatter.FormatManga(DownloadContent, MangaIndex));
                else
                    path = Path.Combine(Directory.CreateDirectory(AppContext.DownloadPathProvider.GetIllustrationPath()).FullName, AppContext.FileNameFormatter.Format(DownloadContent));
                if (cancellationTokenSource.IsCancellationRequested) return;
                await using var fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                memory.WriteTo(fileStream);
                Application.Current.Invoke(() => DownloadFinished?.Invoke(this));
            }
            catch (TaskCanceledException)
            {
                if (path != null && File.Exists(path)) File.Delete(path);
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
                    HandleError(e, path);
                }
            }
        }

        private async void DownloadGif()
        {
            var path = Path.Combine(Directory.CreateDirectory(AppContext.DownloadPathProvider.GetIllustrationPath()).FullName, AppContext.FileNameFormatter.FormatGif(DownloadContent));
            try
            {
                var metadata = await HttpClientFactory.AppApiService().GetUgoiraMetadata(DownloadContent.Id);
                var ugoiraUrl = metadata.UgoiraMetadataInfo.ZipUrls.Medium;
                ugoiraUrl = !ugoiraUrl.EndsWith("ugoira1920x1080.zip") ? Regex.Replace(ugoiraUrl, "ugoira(\\d+)x(\\d+).zip", "ugoira1920x1080.zip") : ugoiraUrl;
                var delay = metadata.UgoiraMetadataInfo.Frames.Select(f => f.Delay / 10).ToArray();
                if (cancellationTokenSource.IsCancellationRequested) return;
                await using var memory = await PixivIO.Download(ugoiraUrl, new Progress<double>(d => Progress = d), cancellationTokenSource.Token);
                await using var gifStream = (MemoryStream) PixivIO.MergeGifStream(PixivIO.ReadGifZipEntries(memory), delay);
                if (cancellationTokenSource.IsCancellationRequested) return;
                await using var fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                gifStream.WriteTo(fileStream);
                Application.Current.Invoke(() => DownloadFinished?.Invoke(this));
            }
            catch (TaskCanceledException)
            {
                if (path != null && File.Exists(path)) File.Delete(path);
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
                    HandleError(e, path);
                }
            }
        }

        private void HandleError(Exception e, string path)
        {
            DownloadFailed = true;
            ReasonPhase = e.Message;
            if (path != null && File.Exists(path)) File.Delete(path);
        }
    }
}