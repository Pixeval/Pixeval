using Pixeval.Data;
using Pixeval.Data.ViewModel;

#pragma warning disable 8509
namespace Pixeval.Core
{
    public enum IllustType
    {
        Illust, Ugoira, Manga
    }

    public static class IllustTypeParser
    {
        public static IllustType ParseType(Illustration illustration)
        {
            return illustration switch
            {
                { Type: "illustration", IsManga: true }  => IllustType.Manga,
                { Type: "manga" }                        => IllustType.Manga,
                { Type: "ugoira" }                       => IllustType.Ugoira,
                { Type: "illustration", IsManga: false } => IllustType.Illust,
            };
        }
    }
}