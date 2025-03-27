// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Frozen;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Pixeval.Controls;
using Mako.Model;
using Pixeval.Util;
using Pixeval.Util.IO;
using SixLabors.ImageSharp;

namespace Pixeval.Pages.Tags;

/// <summary>
/// 由于<see cref="Illustration"/>不一定存在，所以这个类不直接继承 <see cref="IllustrationItemViewModel"/>
/// </summary>
public partial class TagsEntryViewModel : ObservableObject, IEntry
{
    private TagsEntryViewModel(string path)
    {
        FullPath = path;

        Name = Path.GetFileNameWithoutExtension(path);
    }

    public string Name { get; }

    public string FullPath { get; }

    public long Id { get; private set; }

    [ObservableProperty]
    public partial ImageSource? Thumbnail { get; private set; }

    public FrozenSet<string>? TagsSet
    {
        get;
        set
        {
            if (value == field)
                return;
            field = value;
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

    [ObservableProperty]
    public partial Illustration? Illustration { get; set; }

    public async Task<string?> SaveTagsAsync()
    {
        return TagsSet is null
            ? TagsEntryResources.TagsIsUnloaded
            : await Task.Run(async () =>
            {
                try
                {
                    var image = await Image.LoadAsync(FullPath);
                    image.SetIdTags(Id, TagsSet);
                    await using var stream = IoHelper.OpenAsyncWrite(FullPath);
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
        entry.Thumbnail = await IoHelper.GetFileThumbnailAsync(path);
    }
}
