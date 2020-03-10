using Pixeval.Data.ViewModel;
using Pixeval.Objects;

namespace Pixeval.Core
{
    public class DefaultIllustrationFileNameFormatter : IIllustrationFileNameFormatter
    {
        public string Format(Illustration illustration)
        {
            return $"[{Texts.FormatPath(illustration.UserName)}]{illustration.Id}{Texts.GetExtension(illustration.Origin.IsNullOrEmpty() ? illustration.Large : illustration.Origin)}";
        }

        public string FormatManga(Illustration illustration, int idx)
        {
            return $"[{Texts.FormatPath(illustration.UserName)}]{illustration.Id}_p{idx}{Texts.GetExtension(illustration.Origin.IsNullOrEmpty() ? illustration.Large : illustration.Origin)}";
        }

        public string FormatGif(Illustration illustration)
        {
            return $"[{Texts.FormatPath(illustration.UserName)}]{illustration.Id}.gif";
        }
    }
}