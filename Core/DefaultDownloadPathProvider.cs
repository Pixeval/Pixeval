using System.IO;
using Pixeval.Data.ViewModel;
using Pixeval.Objects;
using Pixeval.Persisting;

namespace Pixeval.Core
{
    public class DefaultDownloadPathProvider : IDownloadPathProvider
    {
        public string GetSpotlightPath(string title)
        {
            return Path.Combine(Settings.Global.DownloadLocation, "Spotlight", Texts.FormatPath(title));
        }

        public string GetIllustrationPath()
        {
            return Settings.Global.DownloadLocation;
        }

        public string GetMangaPath(string id)
        {
            return Path.Combine(Settings.Global.DownloadLocation, id);
        }
    }
}