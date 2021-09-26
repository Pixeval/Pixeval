using Windows.Storage.Streams;
using Microsoft.UI.Xaml.Media;

namespace Pixeval.ViewModel
{
    public class PixivReplyStickerViewModel
    {
        public int StickerId { get; }

        public IRandomAccessStream ImageStream { get; }

        public ImageSource? ImageSource { get; set; }

        public PixivReplyStickerViewModel(int stickerId, IRandomAccessStream imageStream)
        {
            StickerId = stickerId;
            ImageStream = imageStream;
        }
    }
}