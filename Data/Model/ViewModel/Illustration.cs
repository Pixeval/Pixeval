using Pixeval.Data.Model.Web.Response;
using PropertyChanged;

#pragma warning disable 8509

namespace Pixeval.Data.Model.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class Illustration
    {
        public string Id { get; set; }

        public bool IsUgoira { get; set; }

        public string Origin { get; set; }

        public string Thumbnail { get; set; }

        public int Bookmark { get; set; }

        public bool IsLiked { get; set; }

        public bool IsManga { get; set; }

        public string Title { get; set; }

        public IllustType Type { get; set; }

        public string UserName { get; set; }

        public string UserId { get; set; }

        public string[] Tags { get; set; }

        public Illustration[] MangaMetadata { get; set; }

        public class IllustType
        {
            public static readonly IllustType Illust = new IllustType();

            public static readonly IllustType Ugoira = new IllustType();

            public static readonly IllustType Manga = new IllustType();

            public static IllustType Parse(IllustResponse.Response illustration)
            {
                return illustration switch
                {
                    { Type: "illustration", IsManga: true } => Manga,
                    { Type: "ugoira" }                => Ugoira,
                    { Type: "illustration", IsManga: false} => Illust
                };
            }
        }

    }
}