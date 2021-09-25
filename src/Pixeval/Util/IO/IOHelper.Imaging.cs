using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Utilities;
using Pixeval.Util.Generic;

namespace Pixeval.Util.IO
{
    public static partial class IOHelper
    {
        /// <summary>
        /// Re-encode and decode the image that wrapped in <paramref name="imageStream"/>. Note that this function
        /// is intended to be used when the image is about to be displayed on the screen instead of saving to file
        /// </summary>
        /// <returns></returns>
        public static async Task<SoftwareBitmapSource> EncodeSoftwareBitmapSourceAsync(this IRandomAccessStream imageStream, bool disposeOfImageStream)
        {
            var bitmap = await GetSoftwareBitmapFromStreamAsync(imageStream);
            using var inMemoryRandomAccessStream = new InMemoryRandomAccessStream();
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, inMemoryRandomAccessStream);
            encoder.SetSoftwareBitmap(bitmap);
            encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Fant;
            await encoder.FlushAsync();
            var source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(await GetSoftwareBitmapFromStreamAsync(inMemoryRandomAccessStream));
            if (disposeOfImageStream)
            {
                imageStream.Dispose();
            }

            return source;
        }

        public static async Task<SoftwareBitmapSource> GetSoftwareBitmapSourceAsync(this IRandomAccessStream imageStream, bool disposeOfImageStream)
        {
            var bitmap = await GetSoftwareBitmapFromStreamAsync(imageStream);
            var source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(bitmap);
            if (disposeOfImageStream)
            {
                imageStream.Dispose();
            }

            return source;
        }

        /// <summary>
        /// Writes the frames that are contained in <paramref name="frames"/> into <paramref name="target"/>
        /// and encodes to a GIF format
        /// </summary>
        /// <returns></returns>
        public static async Task WriteGifBitmapAsync(IRandomAccessStream target, IEnumerable<IRandomAccessStream> frames, int delayInMilliseconds)
        {

            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.GifEncoderId, target);
            await encoder.BitmapProperties.SetPropertiesAsync(new Dictionary<string, BitmapTypedValue> // wtf?
            {
                ["/appext/Application"] = new("NETSCAPE2.0".GetBytes(), PropertyType.UInt8Array),
                ["/appext/Data"] = new(new byte[] {3, 1, 0, 0}, PropertyType.UInt8Array),
                ["/grctlext/Delay"] = new(delayInMilliseconds / 10, PropertyType.UInt16)
            });
            var randomAccessStreams = frames as IRandomAccessStream[] ?? frames.ToArray();
            var frameSoftwareBitmaps = (await Task.WhenAll(randomAccessStreams.Traverse(f => f.Seek(0)).Select(async stream =>
            {
                try
                {
                    // Remarks: the await keyword here is vital, the exception inside a task can be caught only if you await it
                    // otherwise it will be caught by the TaskScheduler.UnobservedTaskException
                    return await GetSoftwareBitmapFromStreamAsync(stream);
                }
                catch (Exception e)
                {
                    return e.HResult switch
                    {
                        // Remarks: the GIF images are consist of multiple frames, some of them may be corrupted or having
                        // a illegal format and thus are incapable of being encoded in to the GIF file, such frames will raise
                        // a COMException indicating an unsuccessful HResult WIN_CODEC_ERR_COMPONENT_NOT_FOUND(0x88982F50),
                        // there is no way to fix this, so instead of try to repair the image, we just simply drop that frame
                        unchecked((int) 0x88982F50) or unchecked((int) 0x88982F81) => null!, // WIN_CODEC_ERR_COMPONENT_NOT_FOUND and WIN_CODEC_ERR_UNSUPPORTED_OPERATION 
                        _ => throw e
                    };
                }
            }))).WhereNotNull().ToArray();
            for (var i = 0; i < frameSoftwareBitmaps.Length; i++)
            {
                var frame = frameSoftwareBitmaps[i];
                encoder.SetSoftwareBitmap(frame);
                if (i < frameSoftwareBitmaps.Length - 1)
                {
                    await encoder.GoToNextFrameAsync();
                }
            }

            encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Fant;
            foreach (var randomAccessStream in randomAccessStreams)
            {
                randomAccessStream.Dispose();
            }
            await encoder.FlushAsync();
        }

        public static async Task<BitmapImage> GetBitmapImageAsync(this IRandomAccessStream imageStream, bool disposeOfImageStream)
        {
            var bitmapImage = new BitmapImage();
            await bitmapImage.SetSourceAsync(imageStream);
            if (disposeOfImageStream)
            {
                imageStream.Dispose();
            }
            return bitmapImage;
        }

        /// <summary>
        /// Decodes the <paramref name="imageStream"/> to a <see cref="SoftwareBitmap"/>
        /// </summary>
        /// <returns></returns>
        public static async Task<SoftwareBitmap?> GetSoftwareBitmapFromStreamAsync(IRandomAccessStream imageStream)
        {
            var decoder = await BitmapDecoder.CreateAsync(imageStream);
            return await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
        }

        public static async Task<IRandomAccessStream> GetGifStreamFromZipStreamAsync(Stream zipStream, UgoiraMetadataResponse ugoiraMetadataResponse)
        {
            var entryStreams = await ReadZipArchiveEntries(zipStream);
            var inMemoryRandomAccessStream = new InMemoryRandomAccessStream();
            await WriteGifBitmapAsync(inMemoryRandomAccessStream, entryStreams.Select(s => s.content.AsRandomAccessStream()), (int) (ugoiraMetadataResponse.UgoiraMetadataInfo?.Frames?.FirstOrDefault()?.Delay ?? 0));
            return inMemoryRandomAccessStream;
        }
    }
}
