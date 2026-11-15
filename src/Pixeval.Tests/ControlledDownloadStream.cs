using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Pixeval.Tests;

internal sealed class ControlledDownloadStream : Stream
{
    private readonly Channel<byte[]> _chunks = Channel.CreateUnbounded<byte[]>();
    private readonly Channel<bool> _reads = Channel.CreateUnbounded<bool>();
    private ReadOnlyMemory<byte> _remaining;

    public void Send(byte[] bytes) => _chunks.Writer.TryWrite(bytes);

    public void Complete() => _chunks.Writer.TryComplete();

    public Task WaitForReadAsync() => _reads.Reader.ReadAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5));

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        _ = _reads.Writer.TryWrite(true);
        while (_remaining.IsEmpty)
        {
            if (!await _chunks.Reader.WaitToReadAsync(token).ConfigureAwait(false))
                return 0;
            if (_chunks.Reader.TryRead(out var chunk))
                _remaining = chunk;
        }

        var count = Math.Min(buffer.Length, _remaining.Length);
        _remaining[..count].CopyTo(buffer);
        _remaining = _remaining[count..];
        return count;
    }

    protected override void Dispose(bool disposing)
    {
        _ = _chunks.Writer.TryComplete();
        base.Dispose(disposing);
    }

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override void Flush() { }
}

internal sealed class DownloadStreamHandler(Stream stream, long? contentLength = null) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
    {
        var content = new StreamContent(stream);
        content.Headers.ContentLength = contentLength;
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
    }
}
