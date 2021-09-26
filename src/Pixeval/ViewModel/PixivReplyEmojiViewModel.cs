using Windows.Storage.Streams;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.Misc;
using Pixeval.Util.IO;

namespace Pixeval.ViewModel
{
    public class PixivReplyEmojiViewModel
    {
        public PixivReplyEmoji EmojiEnumValue { get; }

        public IRandomAccessStream ImageStream { get; }

        public ImageSource? ImageSource { get; set; }

        public PixivReplyEmojiViewModel(PixivReplyEmoji emojiEnumValue, IRandomAccessStream imageStream)
        {
            EmojiEnumValue = emojiEnumValue;
            ImageStream = imageStream;
        }
    }
}