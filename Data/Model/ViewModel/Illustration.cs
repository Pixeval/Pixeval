using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using Pixeval.Annotations;
using Pixeval.Core;
using Pixeval.Data.Model.Web.Response;

#pragma warning disable 8509

namespace Pixeval.Data.Model.ViewModel
{
    public class Illustration : INotifyPropertyChanged
    {
        private string id;
        public string Id
        {
            get => id;
            set => UpdateProperty(ref id, value);
        }

        private bool isUgoira;
        public bool IsUgoira
        {
            get => isUgoira;
            set => UpdateProperty(ref isUgoira, value);
        }

        private string origin;
        public string Origin
        {
            get => origin;
            set => UpdateProperty(ref origin, value);
        }

        private string thumbnail;
        public string Thumbnail
        {
            get => thumbnail;
            set => UpdateProperty(ref thumbnail, value);
        }

        private int bookmark;
        public int Bookmark
        {
            get => bookmark;
            set => UpdateProperty(ref bookmark, value);
        }

        private bool isLiked;
        public bool IsLiked
        {
            get => isLiked;
            set => UpdateProperty(ref isLiked, value);
        }

        private bool isManga;
        public bool IsManga
        {
            get => isManga;
            set => UpdateProperty(ref isManga, value);
        }

        private string title;
        public string Title
        {
            get => title;
            set => UpdateProperty(ref title, value);
        }

        private IllustType type;
        public IllustType Type
        {
            get => type;
            set => UpdateProperty(ref type, value);
        }

        private string userName;
        public string UserName
        {
            get => userName;
            set => UpdateProperty(ref userName, value);
        }

        private string userId;
        public string UserId
        {
            get => userId;
            set => UpdateProperty(ref userId, value);
        }

        private string[] tags;
        public string[] Tags
        {
            get => tags;
            set => UpdateProperty(ref tags, value);
        }

        private Illustration[] mangaMetadata;
        public Illustration[] MangaMetadata
        {
            get => mangaMetadata;
            set => UpdateProperty(ref mangaMetadata, value);
        }

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

        private void UpdateProperty<T>(ref T obj, T newVal, [CallerMemberName] string propertyName = "")
        {
            if (!newVal.Equals(obj))
            {
                obj = newVal;
                OnPropertyChanged(propertyName);
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            if (propertyName == nameof(IsLiked))
            {
                if (isLiked)
                {
                    PixivClient.Instance.PostFavoriteAsync(this);
                }
                else
                {
                    PixivClient.Instance.RemoveFavoriteAsync(this);
                }
            }
        }
    }
}