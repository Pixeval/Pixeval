using PropertyChanged;

namespace Pixeval.Data.Model.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class UserPreview
    {
        public string UserId { get; set; }

        public string UserName { get; set; }

        public string Avatar { get; set; }

        public string[] Thumbnails = new string[3];
    }
}