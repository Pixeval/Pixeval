using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pixeval.CoreApi.Model;
using Pixeval.Util.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;

namespace Pixeval.Util;

public static class TagsManager
{
    public static async Task SetTagsAsync(string imagePath, Illustration illustration, CancellationToken token = default)
    {
        using var image = await Image.LoadAsync(imagePath, token);
        image.SetIdTags(illustration);
        await image.SaveAsync(imagePath, IoHelper.GetIllustrationEncoder(), token);
    }

    public static void SetIdTags(this Image image, Illustration illustration)
    {
        image.SetIdTags(illustration.Id, illustration.Tags.Select(t => t.Name));
    }

    public static async Task SetIdTagsAsync(string imagePath, long id, IEnumerable<string> tags, CancellationToken token = default)
    {
        using var image = await Image.LoadAsync(imagePath, token);
        image.SetIdTags(id, tags);
        await image.SaveAsync(imagePath, IoHelper.GetIllustrationEncoder(), token);
    }

    public static void SetIdTags(this Image image, long id, IEnumerable<string> tags)
    {
        var profile = image.Metadata.ExifProfile ??= new();
        profile.SetValue(ExifTag.ImageNumber, (uint)id);
        profile.SetValue(ExifTag.UserComment, string.Join(';', tags));
    }

    public static long GetIllustrationId(this ImageInfo image)
    {
        if (image.Metadata.ExifProfile?.TryGetValue(ExifTag.ImageNumber, out var id) is true)
            try
            {
                return id.Value;
            }
            catch
            {
                // ignored
            }
        return 0;
    }

    public static string[] GetTags(this ImageInfo image)
    {
        return image.Metadata.ExifProfile?.TryGetValue(ExifTag.UserComment, out var tags) is true
            ? tags.Value.Text.Split(';')
            : [];
    }
}
