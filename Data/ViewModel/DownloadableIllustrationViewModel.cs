using System;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using System.Windows;
using MahApps.Metro.Controls;
using Pixeval.Core;
using Pixeval.Data.Web.Delegation;
using Pixeval.Objects;
using PropertyChanged;

namespace Pixeval.Data.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class DownloadableIllustrationViewModel
    {
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

        [DoNotNotify]
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public void Cancel()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource = new CancellationTokenSource();
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
                await using var memory = await PixevalDownloadTask.Instance.Execute(DownloadContent.GetDownloadUrl(), new Progress<double>(d => Progress = d), cancellationTokenSource);
                if (DownloadContent.FromSpotlight)
                {
                    path = IsFromManga 
                        ? Path.Combine(Directory.CreateDirectory(AppContext.DownloadPathProvider.GetSpotlightPath(DownloadContent.SpotlightTitle)).FullName, AppContext.FileNameFormatter.Format(DownloadContent)) 
                        : Path.Combine(Directory.CreateDirectory(AppContext.DownloadPathProvider.GetSpotlightPath(DownloadContent.SpotlightTitle)).FullName, DownloadContent.Id, AppContext.FileNameFormatter.FormatManga(DownloadContent, MangaIndex));
                }
                else if (IsFromManga)
                    path = Path.Combine(Directory.CreateDirectory(AppContext.DownloadPathProvider.GetMangaPath(DownloadContent.Id)).FullName, AppContext.FileNameFormatter.FormatManga(DownloadContent, MangaIndex));
                else
                    path = Path.Combine(Directory.CreateDirectory(AppContext.DownloadPathProvider.GetIllustrationPath()).FullName, AppContext.FileNameFormatter.Format(DownloadContent));
                if (cancellationTokenSource.IsCancellationRequested) return;
                await using var fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                memory.WriteTo(fileStream);
                Application.Current.Invoke(() => DownloadFinished?.Invoke(this));
            }
            catch (OperationCanceledException)
            {
                if (path != null && File.Exists(path)) File.Delete(path);
            }
            catch (Exception e)
            {
                HandleError(e, path);
            }
        }

        private async void DownloadGif()
        {
            var path = Path.Combine(Directory.CreateDirectory(AppContext.DownloadPathProvider.GetIllustrationPath()).FullName, AppContext.FileNameFormatter.FormatGif(DownloadContent));
            try
            {
                var metadata = await HttpClientFactory.AppApiService.GetUgoiraMetadata(DownloadContent.Id);
                var ugoiraUrl = metadata.UgoiraMetadataInfo.ZipUrls.Medium;
                var delay = metadata.UgoiraMetadataInfo.Frames.Select(f => f.Delay / 10).ToArray();
                if (cancellationTokenSource.IsCancellationRequested) return;
                await using var memory = await PixevalDownloadTask.Instance.Execute(ugoiraUrl, new Progress<double>(d => Progress = d), cancellationTokenSource);
                await using var gifStream = (MemoryStream) IO.MergeGifStream(IO.ReadGifZipEntries(memory), delay);
                if (cancellationTokenSource.IsCancellationRequested) return;
                await using var fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                gifStream.WriteTo(fileStream);
                Application.Current.Invoke(() => DownloadFinished?.Invoke(this));
            }
            catch (OperationCanceledException)
            {
                if (path != null && File.Exists(path)) File.Delete(path);
            }
            catch (Exception e)
            {
                HandleError(e, path);
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