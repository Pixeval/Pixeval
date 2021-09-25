using Microsoft.UI.Xaml.Media;
using Pixeval.Misc;

namespace Pixeval.ViewModel
{
    public class PixivReplyEmojiViewModel
    {
        public PixivReplyEmoji EmojiEnumValue { get; }

        public ImageSource ImageSource { get; }

        public PixivReplyEmojiViewModel(PixivReplyEmoji emojiEnumValue, ImageSource imageSource)
        {
            EmojiEnumValue = emojiEnumValue;
            ImageSource = imageSource;
        }
    }
}