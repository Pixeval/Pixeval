using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Misc;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.Util.UI;

namespace Pixeval.UserControls
{
    public sealed partial class PixivReplyBar
    {
        private readonly NavigationViewTag _replyEmojiPage = new(typeof(PixivReplyEmojiListPage), null);

        private readonly NavigationViewTag _replyStickerPage = new(typeof(PixivReplyStickerListPage), null);

        public PixivReplyBar()
        {
            InitializeComponent();
        }

        private void EmojiButtonFlyoutNavigationView_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            EmojiButtonFlyoutFrame.NavigateByNavigationViewTag(sender, new SlideNavigationTransitionInfo
            {
                Effect = args.SelectedItemContainer.Tag == _replyEmojiPage
                    ? SlideNavigationTransitionEffect.FromLeft
                    : SlideNavigationTransitionEffect.FromRight
            });
        }
    }
}
