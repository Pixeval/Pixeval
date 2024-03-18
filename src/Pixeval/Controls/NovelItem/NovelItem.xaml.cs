using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Foundation;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Controls.MarkupExtensions;
using Pixeval.Messages;
using Pixeval.Options;
using Pixeval.Util.UI;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<NovelItemViewModel>("ViewModel", propertyChanged: nameof(OnViewModelChanged))]
public sealed partial class NovelItem
{
    public event TypedEventHandler<NovelItem, NovelItemViewModel>? ViewModelChanged;

    public event TypedEventHandler<NovelItem, NovelItemViewModel>? OpenNovelRequested;

    public NovelItem() => InitializeComponent();

    public event Func<TeachingTip> RequestTeachingTip = null!;

    private int _isPointerOver;

    public int IsPointerOver
    {
        get => _isPointerOver;
        private set
        {
            var old = _isPointerOver;
            _isPointerOver = value;
            if (IsPointerOver > 0 && old <= 0)
            {
                var anim1 = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation1", this);
                var anim2 = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation2", Image);
                var anim3 = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation3", HeartButton);
                var anim4 = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation4", TitleTextBlock);
                var anim5 = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation5", AuthorTextBlock);
                var anim6 = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation6", TagsList);
                anim1.Configuration = new BasicConnectedAnimationConfiguration();
                anim2.Configuration = new BasicConnectedAnimationConfiguration();
                anim3.Configuration = new BasicConnectedAnimationConfiguration(); 
                anim4.Configuration = new BasicConnectedAnimationConfiguration();
                anim5.Configuration = new BasicConnectedAnimationConfiguration();
                anim6.Configuration = new BasicConnectedAnimationConfiguration();
                _ = anim1.TryStart(NovelItemPopup);
                _ = anim2.TryStart(PopupImage);
                _ = anim3.TryStart(PopupHeartButton);
                _ = anim4.TryStart(PopupTitleTextBlock);
                _ = anim5.TryStart(PopupAuthorTextBlock);
                _ = anim6.TryStart(PopupTagsList);
                NovelItemPopup.Child.To<FrameworkElement>().Width = ActualWidth + 10;
            }
            NovelItemPopup.IsOpen = IsPointerOver > 0;
        }
    }

    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NovelItem item)
        {
            item.ViewModelChanged?.Invoke(item, item.ViewModel);
        }
    }

    private void TagButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
        _ = WeakReferenceMessenger.Default.Send(new WorkTagClickedMessage(SimpleWorkType.Novel, (string)((Button)sender).Content));
    }

    private async void NovelItem_OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        await Task.Delay(1000);
        IsPointerOver += 1;
    }

    private async void NovelItem_OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        await Task.Delay(100);
        IsPointerOver -= 1;
    }

    private void NovelItemPopup_OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        IsPointerOver += 1;
    }

    private void OpenNovel_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        OpenNovelRequested?.Invoke(this, ViewModel);
    }

    private XamlUICommand OpenNovelCommand { get; } = EntryItemResources.OpenNovel.GetCommand(FontIconSymbol.ReadingModeE736);
}
