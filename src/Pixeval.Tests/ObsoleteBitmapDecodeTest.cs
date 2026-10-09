using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Headless;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.Controls;
using Pixeval.Utilities.IO;

namespace Pixeval.Tests;

[TestClass]
[DoNotParallelize]
public sealed class ObsoleteBitmapDecodeTest
{
    [TestMethod]
    public async Task ObsoleteRequestCancelsDecodeWithoutReadingPixelsOrCancelingDownload()
    {
        using var lifetime = new SourceLoadLifetime();
        var operation = lifetime.BeginLoad();
        using var stream = new ProbeStream([]);
        operation.Abandon();

        var exception = await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await stream.DecodeBitmapImageAsync(true, token: operation.DecodeToken));

        Assert.AreEqual(operation.DecodeToken, exception.CancellationToken);
        Assert.AreEqual(0, stream.ReadCalls);
        Assert.AreEqual(1, stream.DisposeCalls);
        Assert.IsFalse(operation.Token.IsCancellationRequested);
    }

    [TestMethod]
    public async Task PageDisposalRejectsDecodeAndCallerOwnedStreamStaysOpen()
    {
        var lifetime = new SourceLoadLifetime();
        var operation = lifetime.BeginLoad();
        lifetime.Dispose();
        using var stream = new ProbeStream([]);

        _ = await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await stream.DecodeBitmapImageAsync(false, token: operation.DecodeToken));
        Assert.AreEqual(0, stream.ReadCalls);
        Assert.AreEqual(0, stream.DisposeCalls);
        Assert.IsTrue(operation.Token.IsCancellationRequested);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task RunningDecodeFinishesAndObsoleteResultIsRejected(bool closePage)
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(ImageViewerRenderingTest.ViewerTestApplication));
        _ = await session.Dispatch<bool>(async () =>
        {
            using var lifetime = new SourceLoadLifetime();
            var operation = lifetime.BeginLoad();
            using var stream = new ProbeStream(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Fixtures", "ProgressiveImages", "progressive.jpg")), () =>
            {
                if (closePage)
                    lifetime.Dispose();
                else
                    operation.Abandon();
            });

            var bitmap = await stream.DecodeBitmapImageAsync(true, token: operation.DecodeToken);

            Assert.IsNotNull(bitmap);
            Assert.IsTrue(stream.ReadCalls > 0);
            Assert.AreEqual(1, stream.DisposeCalls);
            Assert.IsFalse(operation.TrySetSource(bitmap));
            Assert.AreEqual(closePage, operation.Token.IsCancellationRequested);
            return true;
        }, CancellationToken.None);
    }

    [TestMethod]
    public async Task FailedDecodeClosesOwnedStream()
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(ImageViewerRenderingTest.ViewerTestApplication));
        _ = await session.Dispatch<bool>(async () =>
        {
            using var stream = new ProbeStream([]);
            _ = await Assert.ThrowsAsync<Exception>(async () => await stream.DecodeBitmapImageAsync(true));
            Assert.AreEqual(1, stream.DisposeCalls);
            return true;
        }, CancellationToken.None);
    }

    private sealed class ProbeStream(byte[] data, Action? onFirstRead = null) : Stream
    {
        private readonly MemoryStream _inner = new(data);
        private Action? _onFirstRead = onFirstRead;

        public int ReadCalls { get; private set; }
        public int DisposeCalls { get; private set; }
        public override bool CanRead => _inner.CanRead;
        public override bool CanSeek => _inner.CanSeek;
        public override bool CanWrite => false;
        public override long Length => _inner.Length;
        public override long Position { get => _inner.Position; set => _inner.Position = value; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            OnRead();
            return _inner.Read(buffer, offset, count);
        }

        public override int Read(Span<byte> buffer)
        {
            OnRead();
            return _inner.Read(buffer);
        }

        private void OnRead()
        {
            ReadCalls++;
            var action = _onFirstRead;
            _onFirstRead = null;
            action?.Invoke();
        }

        public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
        public override void Flush() => _inner.Flush();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeCalls++;
                _inner.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
