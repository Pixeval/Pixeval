using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.CoreApi.Model;
using Pixeval.Util;
using Pixeval.Util.IO;
using SixLabors.ImageSharp;

namespace Pixeval.Pages.Tags;

public class TagsEntryViewModel : IIllustrate, IDisposable
{
    private TagsEntryViewModel(string path)
    {
        FullPath = path;

        Name = Path.GetFileNameWithoutExtension(path);
    }

    public string Name { get; }

    public string FullPath { get; }

    public SoftwareBitmapSource Thumbnail { get; private init; } = null!;

    public string[] Tags { get; private init; } = null!;

    public Illustration? Illustration { get; private init; } 

    public static async Task<TagsEntryViewModel?> IdentifyAsync(string path)
    {
        try
        {
            var imageInfo = await Image.IdentifyAsync(path);
            var entry = new TagsEntryViewModel(path)
            {
                Thumbnail = await (await IoHelper.GetFileThumbnailAsync(path)).GetSoftwareBitmapSourceAsync(true),
                Tags = imageInfo.GetTags(),
                Illustration = await imageInfo.GetIllustrationAsync()
            };
            return entry;
        }
        catch
        {
            return null;
        }
    }

    public void Dispose() => Thumbnail.Dispose();
}
