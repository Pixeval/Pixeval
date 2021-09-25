using Microsoft.UI.Xaml.Media;

namespace Pixeval.ViewModel
{
    public class PixivReplyStickerViewModel
    {
        public int StickerId { get; }

        public ImageSource ImageSource { get; }

        public PixivReplyStickerViewModel(int stickerId, ImageSource imageSource)
        {
            StickerId = stickerId;
            ImageSource = imageSource;
        }
    }
}