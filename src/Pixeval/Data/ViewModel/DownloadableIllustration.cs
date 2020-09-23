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
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private bool _modifiable = true;

        private bool _retried;

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

        public DownloadOption Option { get; set; }

        public string GetPath()
        {
            if (DownloadContent.IsUgoira) return Path.Combine(Directory.CreateDirectory(DownloadPathProvider.GetIllustrationPath(Option)).FullName, FileNameFormatter.FormatGif(DownloadContent));
            if (DownloadContent.FromSpotlight) return IsFromManga ? Path.Combine(Directory.CreateDirectory(DownloadPathProvider.GetSpotlightPath(DownloadContent.SpotlightTitle, Option)).FullName, DownloadContent.Id, FileNameFormatter.FormatManga(DownloadContent, MangaIndex)) : Path.Combine(Directory.CreateDirectory(DownloadPathProvider.GetSpotlightPath(DownloadContent.SpotlightTitle, Option)).FullName, FileNameFormatter.Format(DownloadContent));
            return IsFromManga ? Path.Combine(Directory.CreateDirectory(DownloadPathProvider.GetMangaPath(DownloadContent.Id, Option)).FullName, FileNameFormatter.FormatManga(DownloadContent, MangaIndex)) : Path.Combine(Directory.CreateDirectory(DownloadPathProvider.GetIllustrationPath(Option)).FullName, FileNameFormatter.Format(DownloadContent));
        }


        public void Freeze()
        {
            _modifiable = false;
        }

        public void Cancel()
        {
            if (_modifiable)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();
                Progress = 0;
                ReasonPhase = null;
                DownloadFailed = false;
                DownloadState.Value = DownloadStateEnum.Canceled;
            }
        }

        public void Restart()
        {
            if (_modifiable)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();
                Progress = 0;
                ReasonPhase = null;
                DownloadFailed = false;
                Download();
            }
        }

        public async void Download()
        {
            if (!_modifiable) return;

            DownloadState.Value = DownloadStateEnum.Downloading;
            var downloadPath = GetPath();
            if (DownloadContent.IsUgoira)
            {
                DownloadGif();
                return;
            }

            try
            {
                await using var memory = await PixivIO.Download(DownloadContent.GetDownloadUrl(), new Progress<double>(d => Progress = d), _cancellationTokenSource.Token);
                if (_cancellationTokenSource.IsCancellationRequested) return;
                await using var fileStream = new FileStream(downloadPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                memory.WriteTo(fileStream);
                DownloadState.Value = DownloadStateEnum.Finished;
            }
            catch (OperationCanceledException)
            {
                if (downloadPath != null && File.Exists(downloadPath)) File.Delete(downloadPath);
            }
            catch (Exception e)
            {
                if (!_retried)
                {
                    Restart();
                    _retried = true;
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
                ugoiraUrl = !ugoiraUrl.EndsWith("ugoira1920x1080.zip") ? Regex.Replace(ugoiraUrl, "ugoira(\\d+)x(\\d+).zip", "ugoira1920x1080.zip") : ugoiraUrl;
                var delay = metadata.UgoiraMetadataInfo.Frames.Select(f => f.Delay / 10).ToArray();
                if (_cancellationTokenSource.IsCancellationRequested) return;
                await using var memory = await PixivIO.Download(ugoiraUrl, new Progress<double>(d => Progress = d), _cancellationTokenSource.Token);
                await using var gifStream = (MemoryStream) InternalIO.MergeGifStream(InternalIO.ReadGifZipEntries(memory), delay);
                if (_cancellationTokenSource.IsCancellationRequested) return;
                await using var fileStream = new FileStream(downloadPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                gifStream.WriteTo(fileStream);
                DownloadState.Value = DownloadStateEnum.Finished;
            }
            catch (TaskCanceledException)
            {
                if (downloadPath != null && File.Exists(downloadPath)) File.Delete(downloadPath);
            }
            catch (Exception e)
            {
                if (!_retried)
                {
                    Restart();
                    _retried = true;
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
        Queue, Downloading, Exceptional, Finished, Canceled
    }
}