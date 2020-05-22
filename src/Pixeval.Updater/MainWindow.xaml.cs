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
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.VisualBasic.FileIO;

namespace Pixeval.Updater
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {

        private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        private static readonly string CurrentDir =
            Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase!).LocalPath);

        private static readonly DirectoryInfo PixevalDirectory = Directory.GetParent(CurrentDir);

        private static readonly string ZipTmpPath = Path.Combine(PixevalDirectory.FullName, "Pixeval.zip");

        private static readonly string ExtractedTmpPath = Path.Combine(PixevalDirectory.FullName, "Pixeval");

        public MainWindow()
        {
            InitializeComponent();
             if (!CheckIsPixevalDirectory(PixevalDirectory)) Exit("Pixeval更新器必须位于Pixeval目录中");
        }

        private void MainWindow_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await using var memory = await Download(RelevantURL.ZipArchive,
                                                        new Progress<double>(p => DownloadProgressIndicator.Value = p),
                                                        CancellationTokenSource.Token);
                memory.Position = 0L;
                if (memory.Checksum<SHA256Managed>() != (await GetRemoteChecksum()).ToLower())
                {
                    Clipboard.SetText("http://119.188.246.6/dc/Pixeval.zip");
                    throw new ChecksumFailedException("文件hash校验失败，请前往http://119.188.246.6/dc/Pixeval.zip手动下载，链接已经复制到剪切板");
                }
                RmFiles();
                await using (var fileStream =
                    new FileStream(ZipTmpPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                {
                    memory.WriteTo(fileStream);
                }

                Extract();
                File.Delete(ZipTmpPath);
                Directory.Delete(Path.Combine(ExtractedTmpPath, "updater"), true);
                FileSystem.CopyDirectory(ExtractedTmpPath, PixevalDirectory.FullName);
                Directory.Delete(ExtractedTmpPath, true);
                Process.Start(Path.Combine(PixevalDirectory.FullName, "Pixeval.exe"));
                Exit();
            }
            catch (TaskCanceledException)
            {
                Exit();
            }
            catch (Exception ex)
            {
                Exit(ex.ToString());
            }
        }

        private static Task<string> GetRemoteChecksum()
        {
            return new HttpClient().GetStringAsync(RelevantURL.Checksum);
        }

        private static void RmFiles()
        {
            var rmList = PixevalDirectory.GetFileSystemInfos().Where(fs => fs.FullName != CurrentDir);
            foreach (var fileSystemInfo in rmList)
                if (File.GetAttributes(fileSystemInfo.FullName).HasFlag(FileAttributes.Directory))
                    Directory.Delete(fileSystemInfo.FullName, true);
                else
                    fileSystemInfo.Delete();
        }

        private static void Extract()
        {
            ZipFile.ExtractToDirectory(ZipTmpPath, PixevalDirectory.FullName);
        }

        private void Exit(string message = null)
        {
            if (message != null) MessageBox.Show(message);

            Close();
            Environment.Exit(0);
        }

        private void DownloadProgressIndicator_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ProgressIndicatorText.Text = $"{e.NewValue * 100:F}%";
        }

        private void CancelButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CancellationTokenSource.Cancel();
        }

        private static bool CheckIsPixevalDirectory(DirectoryInfo dir)
        {
            return dir.GetFiles().Any(_ => _.Name == "Pixeval.dll");
        }

        private static async Task<MemoryStream> Download(string url, IProgress<double> progress,
                                                         CancellationToken cancellationToken = default)
        {
            using var response = await GetResponseHeader(new HttpClient(), url);

            var contentLength = response.Content.Headers.ContentLength;
            if (!contentLength.HasValue)
                throw new InvalidOperationException("cannot retrieve the content length of the request uri");

            response.EnsureSuccessStatusCode();

            long bytesRead, totalRead = 0L;
            var byteBuffer = ArrayPool<byte>.Shared.Rent(4096);

            var memoryStream = new MemoryStream();
            await using var contentStream = await response.Content.ReadAsStreamAsync();
            while ((bytesRead = await contentStream.ReadAsync(byteBuffer, 0, byteBuffer.Length, cancellationToken)) !=
                0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                totalRead += bytesRead;
                await memoryStream.WriteAsync(byteBuffer, 0, (int) bytesRead, cancellationToken);
                progress.Report(totalRead / (double) contentLength);
            }

            ArrayPool<byte>.Shared.Return(byteBuffer, true);

            return memoryStream;
        }

        private static Task<HttpResponseMessage> GetResponseHeader(HttpClient client, string uri)
        {
            return client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
        }
    }
}