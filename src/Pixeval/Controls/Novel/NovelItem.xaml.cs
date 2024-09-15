using System;
using System.Threading.Tasks;
using Windows.Foundation;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Controls.Windowing;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.Messages;
using Pixeval.Util.UI;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;
using Symbol = FluentIcons.Common.Symbol;

namespace Pixeval.Controls;

[DependencyProperty<NovelItemViewModel>("ViewModel", propertyChanged: nameof(OnViewModelChanged))]
public sealed partial class NovelItem
{
    public event TypedEventHandler<NovelItem, NovelItemViewModel>? ViewModelChanged;

    public event TypedEventHandler<NovelItem, NovelItemViewModel>? OpenNovelRequested;

    public event TypedEventHandler<NovelItem, NovelItemViewModel>? RequestOpenUserInfoPage;

    public event TypedEventHandler<NovelItem, NovelItemViewModel>? RequestAddToBookmark;

    public ulong HWnd => WindowFactory.GetWindowForElement(this).HWnd;

#pragma warning disable CS0067, CS0414 // Event is never used
    public event Func<TeachingTip> RequestTeachingTip = null!;
#pragma warning restore CS0067, CS0414 // Event is never used

    public NovelItem() => InitializeComponent();

    private int _isPointerOver;

    public int IsPointerOver
    {
        get => _isPointerOver;
        private set
        {
            var old = _isPointerOver;
            _isPointerOver = value;
            var currentView = ConnectedAnimationService.GetForCurrentView();
            switch (IsPointerOver)
            {
                case > 0 when old <= 0:
                {
                    var anim1 = currentView.PrepareToAnimate("ForwardConnectedAnimation1", this);
                    var anim2 = currentView.PrepareToAnimate("ForwardConnectedAnimation2", Image);
                    var anim3 = currentView.PrepareToAnimate("ForwardConnectedAnimation3", HeartButton);
                    var anim4 = currentView.PrepareToAnimate("ForwardConnectedAnimation4", TitleTextBlock);
                    var anim5 = currentView.PrepareToAnimate("ForwardConnectedAnimation5", AuthorTextBlock);
                    var anim6 = currentView.PrepareToAnimate("ForwardConnectedAnimation6", TagsList);
                    anim1.Configuration = anim2.Configuration = anim3.Configuration =
                        anim4.Configuration = anim5.Configuration = anim6.Configuration =
                            new BasicConnectedAnimationConfiguration();
                    _ = anim1.TryStart(NovelItemPopup);
                    _ = anim2.TryStart(PopupImage);
                    _ = anim3.TryStart(PopupHeartButton);
                    _ = anim4.TryStart(PopupTitleTextBlock);
                    _ = anim5.TryStart(PopupAuthorTextBlock);
                    _ = anim6.TryStart(PopupTagsList);
                    NovelItemPopup.Child.To<FrameworkElement>().Width = ActualWidth + 10;
                    NovelItemPopup.IsOpen = true;
                    break;
                }
                case <= 0 when old > 0:
                {
                    var anim1 = currentView.PrepareToAnimate("BackwardConnectedAnimation1", NovelItemPopup);
                    var anim2 = currentView.PrepareToAnimate("BackwardConnectedAnimation2", PopupImage);
                    var anim3 = currentView.PrepareToAnimate("BackwardConnectedAnimation3", PopupHeartButton);
                    var anim4 = currentView.PrepareToAnimate("BackwardConnectedAnimation4", PopupTitleTextBlock);
                    var anim5 = currentView.PrepareToAnimate("BackwardConnectedAnimation5", PopupAuthorTextBlock);
                    var anim6 = currentView.PrepareToAnimate("BackwardConnectedAnimation6", PopupTagsList);
                    anim1.Configuration = anim2.Configuration = anim3.Configuration =
                        anim4.Configuration = anim5.Configuration = anim6.Configuration =
                            new BasicConnectedAnimationConfiguration();
                    anim1.Completed += (_, _) =>
                    {
                        NovelItemPopup.IsOpen = false;
                        NovelItemPopup.Visibility = Visibility.Visible;
                    };
                    _ = anim1.TryStart(this);
                    _ = anim2.TryStart(Image);
                    _ = anim3.TryStart(HeartButton);
                    _ = anim4.TryStart(TitleTextBlock);
                    _ = anim5.TryStart(AuthorTextBlock);
                    _ = anim6.TryStart(TagsList);
                    _ = Task.Delay(100).ContinueWith(_ => NovelItemPopup.Visibility = Visibility.Collapsed, TaskScheduler.FromCurrentSynchronizationContext());
                    break;
                }
            }
        }
    }

    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NovelItem item)
        {
            item.ViewModelChanged?.Invoke(item, item.ViewModel);
        }
    }

    private void TagButton_OnClicked(object sender, RoutedEventArgs e)
    {
        _ = WeakReferenceMessenger.Default.Send(new WorkTagClickedMessage(SimpleWorkType.Novel, ((TextBlock)((Button)sender).Content).Text));
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

    private void OpenNovel_OnClicked(object sender, RoutedEventArgs e)
    {
        OpenNovelRequested?.Invoke(this, ViewModel);
    }

    private XamlUICommand OpenNovelCommand { get; } = EntryItemResources.OpenNovel.GetCommand(Symbol.BookOpen);

    private void AddToBookmark_OnClicked(object sender, RoutedEventArgs e)
    {
        RequestAddToBookmark?.Invoke(this, ViewModel);
    }

    private void OpenUserInfoPage_OnClicked(object sender, RoutedEventArgs e)
    {
        RequestOpenUserInfoPage?.Invoke(this, ViewModel);
    }
}
