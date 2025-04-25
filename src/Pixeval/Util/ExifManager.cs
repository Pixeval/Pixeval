// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Misaki;
using Pixeval.Options;
using Pixeval.Util.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;

namespace Pixeval.Util;

public static class ExifManager
{
    public static async Task SetTagsAsync(string imagePath, IArtworkInfo illustration, IllustrationDownloadFormat? illustrationDownloadFormat = null, CancellationToken token = default)
    {
        using var image = await Image.LoadAsync(imagePath, token);
        image.SetIdTags(illustration);
        await image.SaveAsync(imagePath, IoHelper.GetIllustrationEncoder(illustrationDownloadFormat), token);
    }

    public static async Task SetTagsAsync(string imagePath, IArtworkInfo illustration, UgoiraDownloadFormat? ugoiraDownloadFormat = null, CancellationToken token = default)
    {
        using var image = await Image.LoadAsync(imagePath, token);
        image.SetIdTags(illustration);
        await image.SaveAsync(imagePath, IoHelper.GetUgoiraEncoder(ugoiraDownloadFormat), token);
    }

    public static void SetIdTags(this Image image, IArtworkInfo illustration)
    {
        image.SetIdTags(illustration.Id, illustration.Tags[ITagCategory.Empty].Select(t => t.Name));
    }

    public static async Task SetIdTagsAsync(string imagePath, string id, IEnumerable<string> tags, IllustrationDownloadFormat? illustrationDownloadFormat = null, CancellationToken token = default)
    {
        using var image = await Image.LoadAsync(imagePath, token);
        image.SetIdTags(id, tags);
        await image.SaveAsync(imagePath, IoHelper.GetIllustrationEncoder(illustrationDownloadFormat), token);
    }

    public static async Task SetIdTagsAsync(string imagePath, string id, IEnumerable<string> tags, UgoiraDownloadFormat? ugoiraDownloadFormat = null, CancellationToken token = default)
    {
        using var image = await Image.LoadAsync(imagePath, token);
        image.SetIdTags(id, tags);
        await image.SaveAsync(imagePath, IoHelper.GetUgoiraEncoder(ugoiraDownloadFormat), token);
    }

    public static void SetIdTags(this Image image, string id, IEnumerable<string> tags)
    {
        var profile = image.Metadata.ExifProfile ??= new();
        profile.SetValue(ExifTag.ImageID, id);
        profile.SetValue(ExifTag.UserComment, string.Join(';', tags));
    }

    public static string? GetArtworkId(this ImageInfo image)
    {
        if (image.Metadata.ExifProfile?.TryGetValue(ExifTag.ImageID, out var id) is true)
            try
            {
                return id.Value;
            }
            catch
            {
                // ignored
            }
        return null;
    }

    public static string[] GetTags(this ImageInfo image)
    {
        return image.Metadata.ExifProfile?.TryGetValue(ExifTag.UserComment, out var tags) is true
            ? tags.Value.Text.Split(';')
            : [];
    }
}
