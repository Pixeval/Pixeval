using System;
using System.IO;
using System.IO.Compression;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.Utilities.IO;
using SkiaSharp;

namespace Pixeval.Tests;

[TestClass]
public sealed class ProgressiveImageDecoderTest
{
    [TestMethod]
    public void ProgressiveJpegChangesAsScansArrive()
    {
        var encoded = File.ReadAllBytes(Fixture("progressive.jpg"));
        using var source = new MemoryStream();
        using var decoder = new ProgressiveImageDecoder();
        source.Write(encoded.AsSpan(0, encoded.Length / 3));
        var partial = decoder.Decode(source);
        Assert.IsNotNull(partial);
        var before = partial.Bytes;
        source.Write(encoded.AsSpan(encoded.Length / 3));
        var complete = decoder.Decode(source);
        Assert.IsNotNull(complete);
        using var expected = SKBitmap.Decode(encoded);
        CollectionAssert.AreEqual(expected.Bytes, complete.Bytes);
        CollectionAssert.AreNotEqual(before, complete.Bytes);
    }

    [TestMethod]
    public void PartialJpegDoesNotPublishUndecodedRows()
    {
        using var bitmap = CreateImage(256, 256);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 90);
        var bytes = data.ToArray();
        using var source = new MemoryStream(bytes, 0, bytes.Length / 2);
        using var decoder = new ProgressiveImageDecoder();
        var partial = decoder.Decode(source);
        Assert.IsNotNull(partial);
        Assert.AreEqual((byte) 255, partial.GetPixel(0, 0).Alpha);
        for (var x = 0; x < partial.Width; x++)
            Assert.AreEqual(new SKColor(0), partial.GetPixel(x, partial.Height - 1));
    }

    [TestMethod]
    [DataRow("frames.gif")]
    [DataRow("frames.webp")]
    public void AnimationShowsNewFramesBeforeDownloadCompletes(string file)
    {
        var encoded = File.ReadAllBytes(Fixture(file));
        using var source = new MemoryStream();
        using var decoder = new ProgressiveImageDecoder();
        var sawRed = false;
        var sawGreen = false;
        for (var index = 0; index < encoded.Length - 1; index++)
        {
            source.WriteByte(encoded[index]);
            if (decoder.Decode(source) is not { } frame)
                continue;
            var color = frame.GetPixel(0, 0);
            sawRed |= color.Red > 200 && color.Green < 20;
            sawGreen |= color.Green > 100 && color.Red < 20;
        }
        Assert.IsTrue(sawRed, $"No first frame for {file}.");
        Assert.IsTrue(sawGreen, $"No next frame before EOF for {file}.");
    }

    private static string Fixture(string name) => Path.Combine(AppContext.BaseDirectory, "Fixtures", "ProgressiveImages", name);

    [TestMethod]
    [DataRow(SKEncodedImageFormat.Jpeg)]
    [DataRow(SKEncodedImageFormat.Png)]
    public void GrowingInputDisplaysPixelsBeforeCompletionAndConverges(SKEncodedImageFormat format)
    {
        using var original = CreateImage(256, 256);
        using var image = SKImage.FromBitmap(original);
        using var data = image.Encode(format, 90);
        var encoded = data.ToArray();
        using var stream = new MemoryStream();
        using var decoder = new ProgressiveImageDecoder();
        var prefix = encoded.Length / 2;
        stream.Write(encoded.AsSpan(0, prefix));
        var partial = decoder.Decode(stream);
        Assert.IsNotNull(partial, $"No partial {format} image.");
        var before = partial.Bytes;
        Assert.IsTrue(Array.Exists(before, b => b is not 0));
        Assert.AreEqual(prefix, stream.Position);
        Assert.IsTrue(stream.CanWrite);

        stream.Write(encoded.AsSpan(prefix));
        var complete = decoder.Decode(stream);
        Assert.IsNotNull(complete);
        using var expected = SKBitmap.Decode(encoded);
        CollectionAssert.AreEqual(expected.Bytes, complete.Bytes);
        CollectionAssert.AreNotEqual(before, complete.Bytes);
        Assert.AreEqual(encoded.Length, stream.Position);
    }

    [TestMethod]
    public void ShortHeaderCanBeRetriedAndDifferentStreamsResetDecoder()
    {
        using var bitmap = CreateImage(128, 128);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        var encoded = data.ToArray();
        using var source = new MemoryStream();
        using var decoder = new ProgressiveImageDecoder();
        source.Write(encoded.AsSpan(0, 8));
        Assert.IsNull(decoder.Decode(source));
        source.Write(encoded.AsSpan(8));
        Assert.IsNotNull(decoder.Decode(source));
        using var second = new MemoryStream(encoded);
        Assert.IsNotNull(decoder.Decode(second));
        decoder.Dispose();
        Assert.IsTrue(source.CanRead);
        Assert.IsTrue(second.CanRead);
    }

    [TestMethod]
    public void PngContinuesAcrossSmallNetworkChunks()
    {
        using var bitmap = CreateImage(128, 128);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        var encoded = data.ToArray();
        using var source = new MemoryStream();
        using var decoder = new ProgressiveImageDecoder();
        byte[]? lastPixels = null;
        var updates = 0;
        for (var offset = 0; offset < encoded.Length; offset += 997)
        {
            source.Write(encoded.AsSpan(offset, Math.Min(997, encoded.Length - offset)));
            if (decoder.Decode(source) is { } preview)
            {
                updates++;
                lastPixels = preview.Bytes;
            }
        }
        Assert.IsGreaterThan(2, updates);
        Assert.IsNotNull(lastPixels);
        CollectionAssert.AreEqual(bitmap.Bytes, lastPixels);
    }

    [TestMethod]
    public void OversizedUnscalableImageDoesNotAllocatePreviewPixels()
    {
        using var bitmap = new SKBitmap(2049, 2049);
        bitmap.Erase(SKColors.Red);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var source = new MemoryStream(data.ToArray());
        using var decoder = new ProgressiveImageDecoder();
        Assert.IsNull(decoder.Decode(source));
        Assert.AreEqual(0, source.Position);
    }

    [TestMethod]
    [DataRow(CompressionLevel.NoCompression)]
    [DataRow(CompressionLevel.Optimal)]
    public void ZipFrameIsAvailableBeforeCentralDirectory(CompressionLevel compression)
    {
        using var zip = new MemoryStream();
        byte[] first = [1, 2, 3, 4];
        byte[] second = [5, 6, 7, 8];
        long firstEnd;
        long secondEnd;
        using (var archive = new ZipArchive(zip, ZipArchiveMode.Create, true))
        {
            using (var entry = archive.CreateEntry("000000.jpg", compression).Open())
                entry.Write(first);
            firstEnd = zip.Position;
            using (var entry = archive.CreateEntry("000001.jpg", compression).Open())
                entry.Write(second);
            secondEnd = zip.Position;
        }
        using var growing = new MemoryStream();
        using var reader = new ZipImagePreviewReader();
        growing.Write(zip.GetBuffer().AsSpan(0, (int) firstEnd - 1));
        Assert.IsNull(reader.ReadLatestFrame(growing));
        growing.WriteByte(zip.GetBuffer()[firstEnd - 1]);
        CollectionAssert.AreEqual(first, ((MemoryStream) reader.ReadLatestFrame(growing)!).ToArray());
        growing.Write(zip.GetBuffer().AsSpan((int) firstEnd, (int) (secondEnd - firstEnd)));
        CollectionAssert.AreEqual(second, ((MemoryStream) reader.ReadLatestFrame(growing)!).ToArray());
        Assert.AreEqual(secondEnd, growing.Position);
        Assert.IsNull(reader.ReadLatestFrame(growing));
    }

    private static SKBitmap CreateImage(int width, int height)
    {
        var bitmap = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
        var random = new Random(42);
        for (var y = 0; y < height; ++y)
            for (var x = 0; x < width; ++x)
                bitmap.SetPixel(x, y, new SKColor((byte) random.Next(256), (byte) random.Next(256), (byte) random.Next(256)));
        return bitmap;
    }
}
