using PropertyChanged;

namespace Pixeval.Data.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class User
    {
        public string Name { get; set; }

        public string Id { get; set; }

        public bool IsFollowed { get; set; }

        public string Avatar { get; set; }

        public string Introduction { get; set; }

        public string Background { get; set; }

        public string[] Thumbnails { get; set; } = new string[3];
    }
}