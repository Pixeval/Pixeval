using System;
using System.Collections.Frozen;
using System.Collections.ObjectModel;
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
public partial class TagsEntryViewModel : ObservableObject, IEntry, IDisposable
{
    private TagsEntryViewModel(string path)
    {
        FullPath = path;

        Name = Path.GetFileNameWithoutExtension(path);
    }

    public string Name { get; }

    public string FullPath { get; }

    public long Id { get; private set; }

    /// <remarks>
    /// Should be private set
    /// </remarks>
    [ObservableProperty] private SoftwareBitmapSource? _thumbnail;

    private FrozenSet<string>? _tagsSet;

    public FrozenSet<string>? TagsSet
    {
        get => _tagsSet;
        set
        {
            if (value == _tagsSet)
                return;
            _tagsSet = value;
            Tags.Clear();
            if (value is not null)
            {
                foreach (var tag in value)
                    Tags.Add(tag);
            }
            // 用来提醒AdvancedObservableCollection的Filter更新
            OnPropertyChanged(nameof(Tags));
        }
    }

    public ObservableCollection<string> Tags { get; } = [];

    /// <remarks>
    /// Should be private set
    /// </remarks>
    [ObservableProperty] private Illustration? _illustration;

    public async Task<string?> SaveTagsAsync()
    {
        return TagsSet is null
            ? TagsEntryResources.TagsIsUnloaded
            : await Task.Run(async () =>
            {
                try
                {
                    var image = await Image.LoadAsync(FullPath);
                    image.SetTags(TagsSet);
                    await using var stream = File.OpenWrite(FullPath);
                    await image.SaveAsync(stream, image.Metadata.DecodedImageFormat!);
                    return null;
                }
                catch (Exception e)
                {
                    return e.Message;
                }
            });
    }

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
        var id = 0L;
        await Task.Run(async () =>
        {
            try
            {
                // 理论上只有此句可能throw
                var info = await Image.IdentifyAsync(path);
                id = info.GetIllustrationId();
                tags = info.GetTags().ToFrozenSet();
            }
            catch
            {
                // ignored
            }
        });
        entry.Id = id;
        entry.TagsSet = tags;
    }

    private static async void LoadThumbnail(TagsEntryViewModel entry, string path)
    {
        entry.Thumbnail = await (await IoHelper.GetFileThumbnailAsync(path)).GetSoftwareBitmapSourceAsync(true);
    }

    public void Dispose() => Thumbnail?.Dispose();
}
