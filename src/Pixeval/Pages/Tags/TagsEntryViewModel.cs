using System;
using System.Collections.Frozen;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.Controls;
using Pixeval.CoreApi.Model;
using Pixeval.Util;
using Pixeval.Util.IO;
using SixLabors.ImageSharp;

namespace Pixeval.Pages.Tags;

/// <summary>
/// 由于<see cref="Illustration"/>不一定存在，所以这个类不直接继承 <see cref="IllustrationItemViewModel"/>
/// </summary>
public partial class TagsEntryViewModel : ObservableObject, IIllustrate, IDisposable
{
    private TagsEntryViewModel(string path)
    {
        FullPath = path;

        Name = Path.GetFileNameWithoutExtension(path);
    }

    public string Name { get; }

    public string FullPath { get; }

    /// <remarks>
    /// Should be private set
    /// </remarks>
    [ObservableProperty] private SoftwareBitmapSource _thumbnail = null!;

    /// <remarks>
    /// Should be private set
    /// </remarks>
    [ObservableProperty] private FrozenSet<string>? _tags;

    public Illustration? Illustration { get; private set; }

    public static async Task<TagsEntryViewModel?> IdentifyAsync(string path)
    {
        try
        {
            _ = await Image.DetectFormatAsync(path);
            var entry = new TagsEntryViewModel(path);
            LoadInfo(entry, path);
            LoadThumbnail(entry, path);
            return entry;
        }
        catch
        {
            return null;
        }
    }

    private static async void LoadInfo(TagsEntryViewModel entry, string path)
    {
        var illustration = null as Illustration;
        var tags = null as FrozenSet<string>;
        await Task.Run(async () =>
        {
            try
            {
                var info = await Image.IdentifyAsync(path);
                illustration = await info.GetIllustrationAsync();
                tags = info.GetTags().ToFrozenSet();
            }
            catch
            {
                // ignored
            }
        });
        entry.Illustration = illustration;
        entry.Tags = tags;
    }

    private static async void LoadThumbnail(TagsEntryViewModel entry, string path)
    {
        entry.Thumbnail = await (await IoHelper.GetFileThumbnailAsync(path)).GetSoftwareBitmapSourceAsync(true);
    }

    public void Dispose() => Thumbnail.Dispose();
}
