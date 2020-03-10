using Pixeval.Data.ViewModel;

namespace Pixeval.Core
{
    public interface IIllustrationFileNameFormatter
    {
        string Format(Illustration illustration);

        string FormatManga(Illustration illustration, int idx);

        string FormatGif(Illustration illustration);
    }

    public interface IDownloadPathProvider
    {
        string GetSpotlightPath(string title);

        string GetIllustrationPath();

        string GetMangaPath(string id);
    }
}