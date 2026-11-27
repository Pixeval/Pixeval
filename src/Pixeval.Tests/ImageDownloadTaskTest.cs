using System;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.Download;
using Pixeval.Models.Download.Tasks;
using Pixeval.Utilities.IO;

namespace Pixeval.Tests;

[TestClass]
public sealed class ImageDownloadTaskTest
{
    [TestMethod]
    public async Task BodyTimeoutRetriesFromTheBeginningWithoutReportingFailure()
    {
        var directory = Directory.CreateTempSubdirectory().FullName;
        try
        {
            using var task = new TestImageDownloadTask(new("test://download/retry"), Path.Combine(directory, "image.bin"), false);
            using var firstResponse = new ControlledDownloadStream();
            firstResponse.Send([1, 2, 3, 4]);
            firstResponse.Complete(new TimeoutException());
            using var handler = new RetryDownloadHandler(firstResponse);
            using var client = new HttpClient(handler, disposeHandler: false);
            var reportedFailure = false;
            task.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName is nameof(task.CurrentState) && task.CurrentState is DownloadState.Error)
                    reportedFailure = true;
            };

            await task.StartAsync(client).WaitAsync(TimeSpan.FromSeconds(5));

            Assert.AreEqual(2, handler.RequestCount);
            Assert.IsFalse(reportedFailure);
            Assert.AreEqual(DownloadState.Completed, task.CurrentState);
            Assert.IsNull(task.ErrorMessage);
            CollectionAssert.AreEqual(new byte[] { 5, 6 }, await File.ReadAllBytesAsync(task.Destination));
            Assert.IsFalse(File.Exists(task.Destination + IoHelper.PixevalTempExtension));
        }
        finally
        {
            DeleteTestDirectory(directory);
        }
    }

    [TestMethod]
    public async Task HttpTimeoutMarksTaskAsFailedAndAllowsRetry()
    {
        var directory = Directory.CreateTempSubdirectory().FullName;
        try
        {
            using var task = new TestImageDownloadTask(new("test://download/timeout"), Path.Combine(directory, "image.bin"), false);
            using var handler = new HeaderTimeoutHandler();
            using var client = new HttpClient(handler, disposeHandler: false);
            client.Timeout = TimeSpan.FromMilliseconds(100);
            await task.StartAsync(client).WaitAsync(TimeSpan.FromSeconds(10));

            Assert.AreEqual(3, handler.RequestCount);
            Assert.AreEqual(DownloadState.Error, task.CurrentState);
            Assert.IsNotNull(task.ErrorMessage);
            Assert.IsFalse(File.Exists(task.Destination + IoHelper.PixevalTempExtension));

            task.Reset();
            using var successClient = new HttpClient(new DownloadStreamHandler(new MemoryStream([1, 2, 3])));
            await task.StartAsync(successClient);
            Assert.AreEqual(DownloadState.Completed, task.CurrentState);
            Assert.IsNull(task.ErrorMessage);
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, await File.ReadAllBytesAsync(task.Destination));
        }
        finally
        {
            DeleteTestDirectory(directory);
        }
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task UserCancellationDoesNotBecomeDownloadError(bool pause)
    {
        var directory = Directory.CreateTempSubdirectory().FullName;
        try
        {
            using var task = new TestImageDownloadTask(new("test://download/cancel"), Path.Combine(directory, "image.bin"), false);
            using var source = new ControlledDownloadStream();
            using var client = new HttpClient(new DownloadStreamHandler(source));
            var downloading = task.StartAsync(client);
            await source.WaitForReadAsync();
            if (pause)
                task.Pause();
            else
                task.Cancel();
            await downloading.WaitAsync(TimeSpan.FromSeconds(5));

            Assert.AreEqual(pause ? DownloadState.Paused : DownloadState.Cancelled, task.CurrentState);
            Assert.IsNull(task.ErrorMessage);
            Assert.IsFalse(File.Exists(task.Destination));
        }
        finally
        {
            DeleteTestDirectory(directory);
        }
    }

    [TestMethod]
    public async Task ResumingWaitsForThePausedRunToStop()
    {
        var directory = Directory.CreateTempSubdirectory().FullName;
        try
        {
            using var task = new TestImageDownloadTask(new("test://download/resume"), Path.Combine(directory, "image.bin"), false);
            using var source = new ControlledDownloadStream();
            using var client = new HttpClient(new DownloadStreamHandler(source));
            using var successClient = new HttpClient(new DownloadStreamHandler(new MemoryStream([1, 2, 3])));
            var stopping = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var allowStop = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            task.DownloadStoppedAsync += sender =>
            {
                _ = stopping.TrySetResult();
                return allowStop.Task;
            };

            var firstRun = task.StartAsync(client);
            await source.WaitForReadAsync();
            task.Pause();
            await stopping.Task.WaitAsync(TimeSpan.FromSeconds(5));
            task.Resume();
            var resumedRun = task.StartAsync(successClient);

            var stateWhileStopping = task.CurrentState;
            allowStop.SetResult();
            await Task.WhenAll(firstRun, resumedRun).WaitAsync(TimeSpan.FromSeconds(5));

            Assert.AreEqual(DownloadState.Queued, stateWhileStopping);
            Assert.AreEqual(DownloadState.Completed, task.CurrentState);
            Assert.IsNull(task.ErrorMessage);
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, await File.ReadAllBytesAsync(task.Destination));
        }
        finally
        {
            DeleteTestDirectory(directory);
        }
    }

    [TestMethod]
    [DataRow(false, "old", true, 0)]
    [DataRow(true, "new", false, 1)]
    public async Task ExistingDestinationShouldRespectOverwriteSetting(
        bool overwrite,
        string expectedContent,
        bool expectedSkipped,
        int expectedPostProcessCount)
    {
        var directory = Directory.CreateTempSubdirectory().FullName;
        try
        {
            var source = Path.Combine(directory, "source.txt");
            var destination = Path.Combine(directory, "destination.txt");
            var temporaryFile = destination + IoHelper.PixevalTempExtension;
            await File.WriteAllTextAsync(source, "new");
            await File.WriteAllTextAsync(destination, "old");
            await File.WriteAllTextAsync(temporaryFile, "partial");

            using var task = new TestImageDownloadTask(new(source), destination, overwrite);
            using var httpClient = new HttpClient();
            await task.StartAsync(httpClient);

            Assert.AreEqual(DownloadState.Completed, task.CurrentState);
            Assert.AreEqual(expectedSkipped, task.WasDownloadSkipped);
            Assert.AreEqual(expectedPostProcessCount, task.PostProcessCount);
            Assert.AreEqual(expectedContent, await File.ReadAllTextAsync(destination));
            Assert.IsFalse(File.Exists(temporaryFile));
        }
        finally
        {
            DeleteTestDirectory(directory);
        }
    }

    [TestMethod]
    [DataRow(false, "old", false)]
    [DataRow(true, "new", true)]
    public async Task CommitShouldRespectDestinationCreatedDuringDownload(
        bool overwrite,
        string expectedContent,
        bool expectedCommitted)
    {
        var directory = Directory.CreateTempSubdirectory().FullName;
        try
        {
            var temporaryFile = Path.Combine(directory, "download.tmp");
            var destination = Path.Combine(directory, "destination.txt");
            await File.WriteAllTextAsync(temporaryFile, "new");
            await File.WriteAllTextAsync(destination, "old");

            var committed = DownloadTaskFileHelper.CommitDownloadedFile(
                temporaryFile,
                destination,
                overwrite);

            Assert.AreEqual(expectedCommitted, committed);
            Assert.AreEqual(expectedContent, await File.ReadAllTextAsync(destination));
            Assert.IsFalse(File.Exists(temporaryFile));
        }
        finally
        {
            DeleteTestDirectory(directory);
        }
    }

    [TestMethod]
    public async Task MissingDestinationDirectoryShouldNotFailTemporaryFileCleanup()
    {
        var directory = Directory.CreateTempSubdirectory().FullName;
        var destinationDirectory = Path.Combine(directory, "missing");
        var destination = Path.Combine(destinationDirectory, "destination.txt");
        try
        {
            var source = Path.Combine(directory, "source.txt");
            await File.WriteAllTextAsync(source, "content");

            using var task = new TestImageDownloadTask(new(source), destination, false);
            using var httpClient = new HttpClient();
            await task.StartAsync(httpClient);

            Assert.AreEqual(DownloadState.Completed, task.CurrentState);
            Assert.AreEqual("content", await File.ReadAllTextAsync(destination));
            Assert.IsFalse(File.Exists(destination + IoHelper.PixevalTempExtension));
        }
        finally
        {
            if (File.Exists(destination))
                File.Delete(destination);
            var temporaryFile = destination + IoHelper.PixevalTempExtension;
            if (File.Exists(temporaryFile))
                File.Delete(temporaryFile);
            if (Directory.Exists(destinationDirectory))
                Directory.Delete(destinationDirectory);
            DeleteTestDirectory(directory);
        }
    }

    private static void DeleteTestDirectory(string directory)
    {
        foreach (var file in Directory.EnumerateFiles(directory))
            File.Delete(file);
        Directory.Delete(directory);
    }

    private sealed class TestImageDownloadTask(Uri uri, string destination, bool overwrite)
        : ImageDownloadTask(uri, destination)
    {
        public int PostProcessCount { get; private set; }

        protected override bool OverwriteDownloadedFile => overwrite;

        protected override Task AfterDownloadAsyncOverride(
            ImageDownloadTask sender,
            CancellationToken token = default)
        {
            ++PostProcessCount;
            return Task.CompletedTask;
        }
    }

    private sealed class HeaderTimeoutHandler : HttpMessageHandler
    {
        public int RequestCount { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            ++RequestCount;
            await Task.Delay(Timeout.Infinite, token);
            return new(HttpStatusCode.OK);
        }
    }

    private sealed class RetryDownloadHandler(Stream firstResponse) : HttpMessageHandler
    {
        public int RequestCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Assert.IsNull(request.Headers.Range);
            var content = ++RequestCount is 1
                ? new StreamContent(firstResponse)
                : (HttpContent) new ByteArrayContent([5, 6]);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
        }
    }
}
