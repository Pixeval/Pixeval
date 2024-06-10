using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pixeval.CoreApi.Model;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;

namespace Pixeval.Util;

public static class TagsManager
{
    public static void SetTags(this Image image, Illustration illustration)
    {
        var profile = image.Metadata.ExifProfile ??= new();
        profile.SetValue(ExifTag.ImageNumber, (uint)illustration.Id);
        profile.SetValue(ExifTag.UserComment, string.Join(';', illustration.Tags.Select(t => t.Name)));
    }

    public static void SetTags(this Image image, IEnumerable<string> tags)
    {
        var profile = image.Metadata.ExifProfile ??= new();
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
