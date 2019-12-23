using PropertyChanged;

namespace Pixeval.Data.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class AutoCompletion
    {
        public string Tag { get; set; }

        public string TranslatedName { get; set; }
    }
}