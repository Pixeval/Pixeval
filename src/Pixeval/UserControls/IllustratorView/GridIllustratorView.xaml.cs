using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media;
using Pixeval.Util.Threading;
using Pixeval.Util.UI;
using Pixeval.Utilities;

namespace Pixeval.UserControls.IllustratorView;

public sealed partial class GridIllustratorView : IIllustratorView
{
    private readonly SemaphoreSlim _semaphoreSlim; 

    // do not close the teaching tip if it turns off and on too fast.
    private bool _teachingTipCloseRequest;

    private CancellationTokenSource? _lastTeachingTipConfigureToken;

    public GridIllustratorView()
    {
        InitializeComponent();
        ViewModel = new GridIllustratorViewViewModel();
        _semaphoreSlim = new SemaphoreSlim(1, 1);
    }

    private EventHandler<TappedRoutedEventArgs>? _userTapped;

    public event EventHandler<TappedRoutedEventArgs> UserTapped
    {
        add => _userTapped += value;
        remove => _userTapped -= value;
    }

    public GridIllustratorViewViewModel ViewModel { get; }

    public FrameworkElement SelfIllustratorView => this;

    IllustratorViewViewModel IIllustratorView.ViewModel => ViewModel;

    public ScrollViewer ScrollViewer => IllustratorGridView.FindDescendant<ScrollViewer>()!;

    public UIElement? GetItemContainer(IllustratorViewModel viewModel)
    {
        return IllustratorGridView.ContainerFromItem(viewModel) as UIElement;
    }

    private void IllustratorProfile_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        _userTapped?.Invoke(sender, e);
    }

    private void IllustratorProfile_OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        _teachingTipCloseRequest = false;
        var viewModel = sender.GetDataContext<IllustratorViewModel>();
        IllustratorInformationTeachingTip.Target = (FrameworkElement) sender;
        IllustratorInformationTeachingTip.IsOpen = true;
        ConfigureTeachingTipAsync(viewModel.UserId, viewModel.Username, viewModel.UserDetail?.UserEntity?.Comment is { Length: > 0 } comment ? comment : IllustratorProfileResources.UserHasNoComment).Discard();
    }

    private async void IllustratorProfile_OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        _teachingTipCloseRequest = true;
        await Task.Delay(500);
        if (_teachingTipCloseRequest)
        {
            IllustratorInformationTeachingTip.IsOpen = false;
        }
    }

    private async Task ConfigureTeachingTipAsync(string uid, string title, string subtitle)
    {
        await _semaphoreSlim.WaitAsync();
        _lastTeachingTipConfigureToken?.Cancel();
        _lastTeachingTipConfigureToken = new CancellationTokenSource();
        IllustratorInformationImagesContainer.Children.Clear();
        IllustratorInformationTeachingTip.Title = title;
        IllustratorInformationTeachingTip.Subtitle = subtitle;
        if (await ViewModel.GetIllustratorDisplayImagesAsync(uid) is { Length: > 0 } and [..var sources])
        {
            IllustratorInformationTeachingTip.Width = 300 + (sources.Length - 1) * 5;
            var averageSize = 300 / sources.Length;
            var images = sources.Select(s => new Image
            {
                Width = averageSize,
                Height = averageSize,
                Stretch = Stretch.UniformToFill,
                Source = s
            });

            if (_lastTeachingTipConfigureToken.IsCancellationRequested)
            {
                _semaphoreSlim.Release();
                return;
            }

            IllustratorInformationImagesContainer.Children.AddRange(images);
        }

        _semaphoreSlim.Release();
    }
}