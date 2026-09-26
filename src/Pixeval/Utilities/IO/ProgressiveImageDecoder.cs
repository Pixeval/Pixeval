// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.IO;
using SkiaSharp;

namespace Pixeval.Utilities.IO;

/// <summary>
/// Decodes a borrowed, growing stream. Calls must be serialized with writes to the stream.
/// Returned pixels belong to this decoder and must be copied before the next call.
/// </summary>
internal sealed class ProgressiveImageDecoder : IDisposable
{
    internal const long MaximumPixelCount = 4 * 1024 * 1024;
    internal const int PreviewDimension = 1024;
    private Stream? _source;
    private SKCodec? _codec;
    private SKBitmap? _pixels;
    private long _decodePosition;
    private bool _incremental;
    private bool _complete;
    private bool _unsupported;
    private int _lastCompleteFrame = -1;

    public SKBitmap? Decode(Stream source)
    {
        if (!ReferenceEquals(source, _source))
        {
            Reset();
            _source = source;
        }

        if (_complete || _unsupported || source.Length < SKCodec.MinBufferedBytesNeeded)
            return null;

        var position = source.Position;
        try
        {
            source.Position = _codec is null ? 0 : _decodePosition;
            if (_codec is null)
            {
                // SKCodec owns the adapter, but never the download buffer.
                _codec = SKCodec.Create(new SKManagedStream(source, false));
                if (_codec is null)
                    return null;

                var info = _codec.Info;
                if ((long) info.Width * info.Height > 64 * 1024 * 1024)
                {
                    _unsupported = true;
                    return null;
                }
                var scale = Math.Min(1f, PreviewDimension / (float) Math.Max(info.Width, info.Height));
                var size = _codec.GetScaledDimensions(scale);
                // PNG/GIF decoders may not support scaled decoding. Bound their full-size workspace.
                if (size.Width <= 0 || size.Height <= 0 || (long) size.Width * size.Height > MaximumPixelCount)
                {
                    _unsupported = true;
                    return null;
                }

                var target = new SKImageInfo(size.Width, size.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
                if (_pixels is null || _pixels.Info != target)
                {
                    _pixels?.Dispose();
                    _pixels = new SKBitmap(target);
                }
                _pixels.Erase(SKColors.Transparent);

                // PNG supports suspending at incomplete input. JPEG requires a fresh bounded decode.
                _incremental = _codec.EncodedFormat is SKEncodedImageFormat.Png
                    && _codec.StartIncrementalDecode(target, _pixels.GetPixels(), _pixels.RowBytes) is SKCodecResult.Success;
            }

            if (_pixels is null)
                return null;

            SKCodecResult result;
            if (_incremental)
            {
                result = _codec.IncrementalDecode(out var rows);
                _decodePosition = source.Position;
                _complete = result is SKCodecResult.Success;
                if (result is SKCodecResult.IncompleteInput && rows is 0)
                    return null;
            }
            else
            {
                var frame = 0;
                var animated = _codec.EncodedFormat is SKEncodedImageFormat.Gif or SKEncodedImageFormat.Webp;
                if (animated)
                {
                    // Show the latest complete frame; do not allocate an ever-growing frame collection.
                    var count = Math.Min(_codec.FrameCount, 10000);
                    for (var index = count - 1; index > 0; --index)
                        if (_codec.GetFrameInfo(index, out var info) && info.FullyRecieved)
                        {
                            frame = index;
                            break;
                        }
                    if (frame == _lastCompleteFrame)
                        return null;
                }
                if (_codec.EncodedFormat is SKEncodedImageFormat.Jpeg
                    && _codec.StartScanlineDecode(_pixels.Info) is SKCodecResult.Success)
                {
                    var rows = _codec.GetScanlines(_pixels.GetPixels(), _pixels.Height, _pixels.RowBytes);
                    if (rows < _pixels.Height)
                    {
                        // GetPixels fills missing JPEG rows. Only publish rows the decoder actually produced.
                        _pixels.Erase(SKColors.Transparent, new SKRectI(0, rows, _pixels.Width, _pixels.Height));
                        if (rows is 0)
                            return null;
                    }
                    result = rows == _pixels.Height ? SKCodecResult.Success : SKCodecResult.IncompleteInput;
                }
                else
                    result = _codec.GetPixels(_pixels.Info, _pixels.GetPixels(), new SKCodecOptions(frame));
                if (animated && result is SKCodecResult.Success)
                    _lastCompleteFrame = frame;
            }

            return result is SKCodecResult.Success or SKCodecResult.IncompleteInput ? _pixels : null;
        }
        finally
        {
            if (!_incremental || _complete || _unsupported)
            {
                _codec?.Dispose();
                _codec = null;
            }
            source.Position = position;
        }
    }

    private void Reset()
    {
        _codec?.Dispose();
        _codec = null;
        _pixels?.Dispose();
        _pixels = null;
        _incremental = false;
        _complete = false;
        _unsupported = false;
        _source = null;
        _lastCompleteFrame = -1;
    }

    public void Dispose() => Reset();
}
