using Pixeval.Models;

namespace Pixeval.Interfaces
{
    public interface IIllustrationFileNameFormatter
    {
        string Format(Illustration illustration);

        string FormatManga(Illustration illustration, int idx);

        string FormatGif(Illustration illustration);
    }
}