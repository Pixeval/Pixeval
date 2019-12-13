using PropertyChanged;

namespace Pixeval.Data.Model.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class UserPreview
    {
        public string[] Thumbnails { get; set; } = new string[3];

        public string UserId { get; set; }

        public string UserName { get; set; }

        public string Avatar { get; set; }
    }
}