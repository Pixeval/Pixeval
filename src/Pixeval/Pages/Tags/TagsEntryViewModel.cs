using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.CoreApi.Model;
using Pixeval.Util.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;

namespace Pixeval.Pages.Tags;

public class TagsEntryViewModel : IIllustrate, IDisposable
{
    private TagsEntryViewModel(string path, SoftwareBitmapSource thumbnail)
    {
        FullPath = path;
        Thumbnail = thumbnail;

        Name = Path.GetFileNameWithoutExtension(path);
    }

    public string Name { get; }

    public string FullPath { get; }

    public SoftwareBitmapSource Thumbnail { get; }

    public ObservableCollection<string> Tags { get; } = [];

    public static async Task<TagsEntryViewModel?> IdentifyAsync(string path)
    {
        try
        {
            var imageInfo = await Image.IdentifyAsync(path);
            var file = await StorageFile.GetFileFromPathAsync(path);
            var thumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, 64);
            var entry = new TagsEntryViewModel(path, await thumbnail.AsStreamForRead().GetSoftwareBitmapSourceAsync(true));
            if (imageInfo.Metadata.ExifProfile is { } profile && profile.TryGetValue(ExifTag.UserComment, out var value))
                foreach (var s in value.Value.Text.Split(';'))
                    entry.Tags.Add(s);
            return entry;
        }
        catch
        {
            return null;
        }
    }

    public void Dispose() => Thumbnail.Dispose();
}
