// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SkiaSharp;

namespace Pixeval.Utilities.IO;

/// <summary>
/// A serialized preview pump with no queued frames or copies of the encoded download.
/// </summary>
internal sealed class ProgressiveImagePreview(Func<Bitmap, Task> publish, Func<bool> isVisible) : IDisposable
{
    private readonly ProgressiveImageDecoder _decoder = new();
    private readonly ZipImagePreviewReader _zipReader = new();
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private TimeSpan _nextUpdate;
    private bool _failed;

    public Task UpdateAsync(Stream source, CancellationToken token) => UpdateAsync(source, false, token);

    public Task UpdateZipAsync(Stream source, CancellationToken token) => UpdateAsync(source, true, token);

    private async Task UpdateAsync(Stream source, bool zip, CancellationToken token)
    {
        // Neighboring pages are prefetched too; do not decode previews with no attached viewer.
        if (!isVisible())
        {
            _decoder.Dispose();
            _zipReader.Dispose();
            return;
        }
        if (_failed || _clock.Elapsed < _nextUpdate)
            return;

        var started = _clock.Elapsed;
        Bitmap? preview = null;
        try
        {
            preview = await Task.Run(() => zip
                ? _zipReader.ReadLatestFrame(source) is { } frame ? Decode(frame) : null
                : Decode(source), token).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            if (preview is not null)
            {
                await publish(preview).ConfigureAwait(false);
                preview = null;
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            // Preview failure must not turn a valid full download into a failed image load.
            _failed = true;
        }
        finally
        {
            preview?.Dispose();
            // At most two previews per second; expensive codecs get a lower CPU duty cycle.
            var duration = _clock.Elapsed - started;
            _nextUpdate = _clock.Elapsed + TimeSpan.FromMilliseconds(Math.Max(500, duration.TotalMilliseconds * 9));
        }
    }

    private Bitmap? Decode(Stream source)
    {
        if (_decoder.Decode(source) is not { } pixels)
            return null;

        var scale = Math.Min(1d, ProgressiveImageDecoder.PreviewDimension / (double) Math.Max(pixels.Width, pixels.Height));
        using var resized = scale < 1
            ? pixels.Resize(new SKImageInfo(Math.Max(1, (int) (pixels.Width * scale)), Math.Max(1, (int) (pixels.Height * scale)),
                SKColorType.Bgra8888, SKAlphaType.Premul), new SKSamplingOptions(SKFilterMode.Linear))
            : null;
        var image = resized ?? pixels;
        return new Bitmap(PixelFormat.Bgra8888, AlphaFormat.Premul, image.GetPixels(),
            new PixelSize(image.Width, image.Height), new Vector(96, 96), image.RowBytes);
    }

    public void Dispose()
    {
        _decoder.Dispose();
        _zipReader.Dispose();
    }
}
