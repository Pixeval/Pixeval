using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Messages;
using Pixeval.Options;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.UserControls.JustifiedLayout;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.Threading;
using Pixeval.Util.UI;
using Pixeval.Utilities;

namespace Pixeval.UserControls.IllustrationView;

public sealed partial class JustifiedLayoutIllustrationView : IIllustrationView
{
    private static readonly ExponentialEase ImageSourceSetEasingFunction = new()
    {
        EasingMode = EasingMode.EaseOut,
        Exponent = 12
    };

    public JustifiedLayoutIllustrationView()
    {
        InitializeComponent();
        ViewModel = new JustifiedLayoutIllustrationViewViewModel();
        ((JustifiedLayoutIllustrationViewDataProvider) ViewModel.DataProvider).OnDeferLoadingCompleted += (_, loaded) =>
        {
            IllustrationJustifiedListView.Append(loaded);
        };
        ((JustifiedLayoutIllustrationViewDataProvider) ViewModel.DataProvider).OnIllustrationsSourceCollectionChanged += (_, args) =>
        {
            switch (args)
            {
                case { Action: NotifyCollectionChangedAction.Remove }:
                    IllustrationJustifiedListView.Remove(args.OldItems?.OfType<IllustrationViewModel>() ?? Enumerable.Empty<IllustrationViewModel>());
                    break;
                case { Action: NotifyCollectionChangedAction.Reset }:
                    IllustrationJustifiedListView.Clear();
                    IllustrationJustifiedListView.Append(args.NewItems?.OfType<IllustrationViewModel>() ?? Enumerable.Empty<IllustrationViewModel>());
                    break;
            }
        };
        ViewModel.DataProvider.FilterChanged += (sender, _) =>
        {
            IllustrationJustifiedListView.Filter = sender as Predicate<object>;
            IllustrationJustifiedListView.RecomputeLayout();
            IllustrationJustifiedListView.FillListViewAsync().Discard();
        };
    }
    
    public JustifiedLayoutIllustrationViewViewModel ViewModel { get; }

    public FrameworkElement SelfIllustrationView => this;

    IllustrationViewViewModel IIllustrationView.ViewModel => ViewModel;

    public ScrollViewer ScrollViewer => IllustrationJustifiedListView.FindDescendant<ScrollViewer>()!;

    public int CalculateDesiresRows(double actualHeight)
    {
        return (int) (actualHeight / 250) is var rows and not 0 ? rows : 4;
    }

    private void Thumbnail_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        if (InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down))
        {
            // User is doing the range selection
            return;
        }

        e.Handled = true;
        WeakReferenceMessenger.Default.Send(new MainPageFrameSetConnectedAnimationTargetMessage(sender as UIElement));

        var viewModels = sender.GetDataContext<JustifiedListViewRowItemWrapper>().Item
            .GetMangaIllustrationViewModels()
            .ToArray();

        ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation", (UIElement) sender);
        UIHelper.RootFrameNavigate(typeof(IllustrationViewerPage), new IllustrationViewerPageViewModel(this, viewModels), new SuppressNavigationTransitionInfo());
    }

    public UIElement? GetItemContainer(IllustrationViewModel viewModel)
    {
        return IllustrationJustifiedListView.Children.Select(c => c.ContainerFromItem(viewModel)).WhereNotNull().FirstOrDefault();
    }

    private async void PostBookmarkButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
        var viewModel = sender.GetDataContext<JustifiedListViewRowItemWrapper>().Item;
        await viewModel.PostPublicBookmarkAsync();
    }

    private async void RemoveBookmarkButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
        var viewModel = sender.GetDataContext<JustifiedListViewRowItemWrapper>().Item;
        await viewModel.RemoveBookmarkAsync();
    }

    private void BookmarkContextItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        sender.GetDataContext<JustifiedListViewRowItemWrapper>().Item.SwitchBookmarkStateAsync();
    }

    private async void SaveContextItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        await sender.GetDataContext<JustifiedListViewRowItemWrapper>().Item.SaveAsync();
    }

    private async void SaveAsContextItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        await sender.GetDataContext<JustifiedListViewRowItemWrapper>().Item.SaveAsAsync();
    }

    private async void OpenInBrowserContextItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        await Launcher.LaunchUriAsync(MakoHelper.GenerateIllustrationWebUri(sender.GetDataContext<JustifiedListViewRowItemWrapper>().Item.Id));
    }

    private void AddToBookmarkContextItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void CopyWebLinkContextItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        UIHelper.SetClipboardContent(package => package.SetText(MakoHelper.GenerateIllustrationWebUri(sender.GetDataContext<JustifiedListViewRowItemWrapper>().Item.Id).ToString()));
    }

    private void CopyAppLinkContextItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        UIHelper.SetClipboardContent(package => package.SetText(MakoHelper.GenerateIllustrationAppUri(sender.GetDataContext<JustifiedListViewRowItemWrapper>().Item.Id).ToString()));
    }

    private async void ShowQrCodeContextItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        await ViewModel.ShowQrCodeForIllustrationAsync(sender.GetDataContext<JustifiedListViewRowItemWrapper>().Item);
    }

    private async void ShowPixEzQrCodeContextItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        await ViewModel.ShowPixEzQrCodeForIllustrationAsync(sender.GetDataContext<JustifiedListViewRowItemWrapper>().Item);
    }

    private async void IllustrationThumbnailContainerItem_OnLoaded(object sender, RoutedEventArgs e)
    {
        var (context, layoutWidth, _) = sender.GetDataContext<JustifiedListViewRowItemWrapper>();
        var element = (FrameworkElement) sender;

        if (await context.LoadThumbnailIfRequired(layoutWidth > 350 ? ThumbnailUrlOption.Large : ThumbnailUrlOption.SquareMedium))
        {
            var transform = (ScaleTransform) element.RenderTransform;
            if (element.IsFullyOrPartiallyVisible(this))
            {
                var scaleXAnimation = transform.CreateDoubleAnimation(nameof(transform.ScaleX), from: 1.1, to: 1, easingFunction: ImageSourceSetEasingFunction, duration: TimeSpan.FromSeconds(2));
                var scaleYAnimation = transform.CreateDoubleAnimation(nameof(transform.ScaleY), from: 1.1, to: 1, easingFunction: ImageSourceSetEasingFunction, duration: TimeSpan.FromSeconds(2));
                var opacityAnimation = element.CreateDoubleAnimation(nameof(element.Opacity), from: 0, to: 1, easingFunction: ImageSourceSetEasingFunction, duration: TimeSpan.FromSeconds(2));
                UIHelper.CreateStoryboard(scaleXAnimation, scaleYAnimation, opacityAnimation).Begin();
            }
            else
            {
                transform.ScaleX = 1;
                transform.ScaleY = 1;
                element.Opacity = 1;
            }
        }
    }

    private async void IllustrationJustifiedListView_OnLoadMoreRequested(object? sender, JustifiedListViewLoadMoreRequestEventArgs e)
    {
        var result = await ViewModel.DataProvider.LoadMore();
        e.Deferral.SetResult(result);
    }

    private void IllustrationJustifiedListView_OnRowBringingIntoView(object sender, EffectiveViewportChangedEventArgs args)
    {
        var wrapper = sender.GetDataContext<JustifiedListViewRowBinding?>();
        var preLoadRows = Math.Clamp(App.AppViewModel.AppSetting.PreLoadRows, 1, 15);
        var gridViewItems = ((FrameworkElement) sender).FindDescendants().OfType<Grid>().Where(i => i.Name == "IllustrationThumbnailContainerItem").ToList();

        if (wrapper == null || !gridViewItems.Any())
        {
            return;
        }

        async Task LoadThumbnailAsync(IllustrationViewModel context, FrameworkElement container)
        {
            if (args.BringIntoViewDistanceY <= container.ActualHeight * preLoadRows)
            {
                if (await context.LoadThumbnailIfRequired()!)
                {
                    var transform = (ScaleTransform) container.RenderTransform;
                    if (container.IsFullyOrPartiallyVisible(this))
                    {
                        var scaleXAnimation = transform.CreateDoubleAnimation(nameof(transform.ScaleX), from: 1.1, to: 1, easingFunction: ImageSourceSetEasingFunction, duration: TimeSpan.FromSeconds(2));
                        var scaleYAnimation = transform.CreateDoubleAnimation(nameof(transform.ScaleY), from: 1.1, to: 1, easingFunction: ImageSourceSetEasingFunction, duration: TimeSpan.FromSeconds(2));
                        var opacityAnimation = container.CreateDoubleAnimation(nameof(container.Opacity), from: 0, to: 1, easingFunction: ImageSourceSetEasingFunction, duration: TimeSpan.FromSeconds(2));
                        UIHelper.CreateStoryboard(scaleXAnimation, scaleYAnimation, opacityAnimation).Begin();
                    }
                    else
                    {
                        transform.ScaleX = 1;
                        transform.ScaleY = 1;
                        container.Opacity = 1;
                    }
                }

                return;
            }

            // small tricks to reduce memory consumption
            switch (context)
            {
                case { LoadingThumbnail: true }:
                    context.LoadingThumbnailCancellationHandle.Cancel();
                    break;
                case { ThumbnailSource: not null }:
                    var source = context.ThumbnailSource;
                    context.ThumbnailSource = null;
                    source.Dispose();
                    break;
            }
        }

        foreach (var ((illustrationViewModel, _, _), gridViewItem) in wrapper.Wrappers.Zip(gridViewItems))
        {
            LoadThumbnailAsync(illustrationViewModel, gridViewItem).Discard();
        }
    }
}