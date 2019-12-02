using PropertyChanged;

namespace Pzxlane.Data.Model.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class User
    {
        public string Name { get; set; }

        public string Id { get; set; }

        public bool IsFollowed { get; set; }

        public string Twitter { get; set; }

        public string WebPage { get; set; }

        public int Illustrations { get; set; }

        public string Introduction { get; set; }

        public string[] Thumbnails { get; set; }
    }
}