using System;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Misc;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.Util.UI;
using Pixeval.ViewModel;

namespace Pixeval.UserControls
{
    public sealed partial class PixivReplyBar
    {
        private EventHandler<SendButtonTappedEventArgs>? _sendButtonTapped;

        public event EventHandler<SendButtonTappedEventArgs> SendButtonTapped
        {
            add => _sendButtonTapped += value;
            remove => _sendButtonTapped -= value;
        }

        private EventHandler<StickerTappedEventArgs>? _stickerTapped;

        public event EventHandler<StickerTappedEventArgs> StickerTapped
        {
            add => _stickerTapped += value;
            remove => _stickerTapped -= value;
        }

        public PixivReplyBar()
        {
            InitializeComponent();
        }

        private void PixivReplyBar_OnLoaded(object sender, RoutedEventArgs e)
        {
            EmojiButtonFlyoutEmojiSectionNavigationViewItem.Tag = new NavigationViewTag(typeof(PixivReplyEmojiListPage), this); 
            EmojiButtonFlyoutStickersSectionNavigationViewItem.Tag = new NavigationViewTag(typeof(PixivReplyStickerListPage), (Guid.NewGuid(), _stickerTapped) /* just to support the serialization, see https://docs.microsoft.com/en-us/windows/winui/api/microsoft.ui.xaml.controls.frame.navigate?view=winui-3.0#Microsoft_UI_Xaml_Controls_Frame_Navigate_Windows_UI_Xaml_Interop_TypeName_System_Object_Microsoft_UI_Xaml_Media_Animation_NavigationTransitionInfo_ */);
        }

        private void EmojiButtonFlyoutNavigationView_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            EmojiButtonFlyoutFrame.NavigateByNavigationViewTag(sender, new SlideNavigationTransitionInfo
            {
                Effect = args.SelectedItemContainer == EmojiButtonFlyoutEmojiSectionNavigationViewItem
                    ? SlideNavigationTransitionEffect.FromLeft
                    : SlideNavigationTransitionEffect.FromRight
            });
        }

        private void SendButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            ReplyContentRichEditBox.Document.GetText(TextGetOptions.UseObjectText, out var content);
            if (content.Length > 140)
            {
                PixivReplyBarInAppNotification.Show(MiscResources.ReplyContentTooLong, 2000);
                return;
            }
            _sendButtonTapped?.Invoke(this, new SendButtonTappedEventArgs(e, content));
            ReplyContentRichEditBox.ClearContent();
        }
    }

    public class SendButtonTappedEventArgs : EventArgs
    {
        public TappedRoutedEventArgs TappedEventArgs { get; }

        public string ReplyContentRichEditBoxStringContent { get; }

        public SendButtonTappedEventArgs(TappedRoutedEventArgs tappedEventArgs, string replyContentRichEditBoxStringContent)
        {
            TappedEventArgs = tappedEventArgs;
            ReplyContentRichEditBoxStringContent = replyContentRichEditBoxStringContent;
        }
    }

    public class StickerTappedEventArgs : EventArgs
    {
        public TappedRoutedEventArgs TappedEventArgs { get; }

        public PixivReplyStickerViewModel StickerViewModel { get; }

        public StickerTappedEventArgs(TappedRoutedEventArgs tappedEventArgs, PixivReplyStickerViewModel stickerViewModel)
        {
            TappedEventArgs = tappedEventArgs;
            StickerViewModel = stickerViewModel;
        }
    }
}