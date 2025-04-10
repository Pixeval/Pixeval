// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.Extensions;
using Pixeval.Extensions.Common.Commands.Transformers;
using Pixeval.Utilities;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.System;
using WinRT;
using WinUI3Utilities;
using SymbolIcon = FluentIcons.WinUI.SymbolIcon;

namespace Pixeval.Pages.IllustrationViewer;

public sealed partial class IllustrationViewerPage
{
    private IllustrationViewerPageViewModel _viewModel = null!;

    public IllustrationViewerPage() => InitializeComponent();

    public override void OnPageActivated(NavigationEventArgs e, object? parameter) => SetViewModel(parameter);

    public void SetViewModel(object? parameter)
    {
        // 此处this.XamlRoot为null
        _viewModel = this.GetIllustrationViewerPageViewModelFromHandle(parameter);

        _viewModel.DetailedPropertyChanged += (sender, args) =>
        {
            var vm = sender.To<IllustrationViewerPageViewModel>();

            if (args.PropertyName is not nameof(vm.CurrentIllustrationIndex) and not nameof(vm.CurrentPageIndex))
                return;

            var oldIndex = args.OldValue.To<int>();
            var newIndex = args.NewValue.To<int>(); // vm.CurrentIllustrationIndex

            NavigationTransitionInfo? info = null;
            if (oldIndex < newIndex && oldIndex is not -1)
                info = new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight };
            else if (oldIndex > newIndex)
                info = new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromLeft };

            if (args.PropertyName is nameof(vm.CurrentIllustrationIndex))
            {
                var oldTag = args.OldTag.To<long>();
                var newTag = args.NewTag.To<long>(); // vm.CurrentPage.Id
                if (oldTag == newTag)
                    return;
                // TODO: https://github.com/microsoft/microsoft-ui-xaml/issues/9952
                // ThumbnailItemsView.StartBringItemIntoView(vm.CurrentIllustrationIndex, new BringIntoViewOptions { AnimationDesired = true });
                EntryViewerSplitView.NavigationViewSelectRefresh();
            }

            Navigate<ImageViewerPage>(IllustrationImageShowcaseFrame, vm.CurrentImage, info);
        };

        _viewModel.PropertyChanged += (sender, args) =>
        {
            var vm = sender.To<IllustrationViewerPageViewModel>();
            switch (args.PropertyName)
            {
                case nameof(IllustrationViewerPageViewModel.CurrentImage):
                {
                    foreach (var appBarButton in ExtensionsCommandBar.PrimaryCommands.OfType<AppBarButton>())
                    {
                        var extension = appBarButton.GetTag<IImageTransformerCommandExtension>();
                        appBarButton.Command = _viewModel.CurrentImage.GetTransformExtensionCommand(extension);
                    }
                    break;
                }
            }
        };

        // 第一次_viewModel.CurrentIllustrationIndex变化时，还没有订阅事件，所以不会触发DetailedPropertyChanged，需要手动触发
        Navigate<ImageViewerPage>(IllustrationImageShowcaseFrame, _viewModel.CurrentImage);

        CommandBorderDropShadow.Receivers.Add(IllustrationImageShowcaseFrame);

        // TODO: https://github.com/microsoft/microsoft-ui-xaml/issues/9952
        // ThumbnailItemsView.StartBringItemIntoView(_viewModel.CurrentIllustrationIndex, new BringIntoViewOptions { AnimationDesired = true });

        var extensionService = App.AppViewModel.AppServiceProvider.GetRequiredService<ExtensionService>();
        if (!extensionService.ActiveImageTransformerCommands.Any())
            return;
        foreach (var extension in extensionService.ActiveImageTransformerCommands)
        {
            var appBarButton = new AppBarButton
            {
                Tag = extension,
                Label = extension.GetLabel(),
                Icon = new SymbolIcon { Symbol = extension.GetIcon() },
                CommandParameter = extension,
                // 第一次_viewModel.CurrentIllustrationIndex变化时，还没有订阅事件，所以需要手动订阅
                Command = _viewModel.CurrentImage.GetTransformExtensionCommand(extension)
            };
            ToolTipService.SetToolTip(appBarButton, extension.GetDescription());
            ExtensionsCommandBar.PrimaryCommands.Add(appBarButton);
        }

        ExtensionsCommandBar.PrimaryCommands.Add(new AppBarSeparator());
    }

    private async void FrameworkElement_OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs e)
    {
        var viewModel = sender.GetDataContext<IllustrationItemViewModel>();
        _ = await viewModel.TryLoadThumbnailAsync(_viewModel);
    }

    private void AddToBookmarkTeachingTip_OnCloseButtonClick(TeachingTip sender, object args)
    {
        _viewModel.CurrentIllustration.AddToBookmarkCommand.Execute((BookmarkTagSelector.SelectedTags,
            BookmarkTagSelector.IsPrivate, this));

        this.SuccessGrowl(EntryViewerPageResources.AddedToBookmark);
    }

    private void NextButton_OnClicked(object sender, IWinRTObject e)
    {
        switch (_viewModel.NextButtonAction)
        {
            case true: ++PipsPager.SelectedPageIndex; break;
            // 由于先后次序问题，必须操作SelectedIndex，而不是_viewModel.CurrentIllustrationIndex
            case false: ++ThumbnailItemsView.SelectedIndex; break;
            case null: break;
        }
    }

    private void NextButton_OnRightTapped(object sender, IWinRTObject e)
    {
        // 由于先后次序问题，必须操作SelectedIndex，而不是_viewModel.CurrentIllustrationIndex
        ++ThumbnailItemsView.SelectedIndex;
    }

    private void PrevButton_OnClicked(object sender, IWinRTObject e)
    {
        switch (_viewModel.PrevButtonAction)
        {
            case true: --PipsPager.SelectedPageIndex; break;
            // 由于先后次序问题，必须操作SelectedIndex，而不是_viewModel.CurrentIllustrationIndex
            case false: --ThumbnailItemsView.SelectedIndex; break;
            case null: break;
        }
    }

    private void PrevButton_OnRightTapped(object sender, IWinRTObject e)
    {
        // 由于先后次序问题，必须操作SelectedIndex，而不是_viewModel.CurrentIllustrationIndex
        --ThumbnailItemsView.SelectedIndex;
    }

    private void ThumbnailItemsView_OnPreviewKeyDown(object sender, KeyRoutedEventArgs e)
    {
        e.Handled = true;
        switch (e.Key)
        {
            case VirtualKey.Left: PrevButton_OnClicked(null!, null!); break;
            case VirtualKey.Right: NextButton_OnClicked(null!, null!); break;
            case VirtualKey.Up: PrevButton_OnRightTapped(null!, null!); break;
            case VirtualKey.Down: NextButton_OnRightTapped(null!, null!); break;
        }
    }

    private void Content_OnLoading(FrameworkElement sender, object args)
    {
        var teachingTip = sender.GetTag<TeachingTip>();
        var appBarButton = teachingTip.GetTag<AppBarButton>();
        teachingTip.Target = appBarButton.IsInOverflow ? null : appBarButton;
    }

    private void IllustrationViewerPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        var dataTransferManager = DataTransferManagerInterop.GetForWindow((nint) Window.HWnd);
        dataTransferManager.DataRequested += OnDataTransferManagerOnDataRequested;
        return;
        async void OnDataTransferManagerOnDataRequested(DataTransferManager s, DataRequestedEventArgs args)
        {
            var vm = _viewModel.CurrentIllustration;
            if (!_viewModel.CurrentImage.LoadSuccessfully)
                return;

            var request = args.Request;
            var deferral = request.GetDeferral();

            var props = request.Data.Properties;

            props.Title = EntryViewerPageResources.ShareTitleFormatted.Format(vm.Id);
            props.Description = vm.Title;

            var file = await _viewModel.CurrentImage.SaveToFolderAsync(AppKnownFolders.Temp);
            request.Data.SetStorageItems([file]);
            request.Data.ShareCanceled += FileDispose;
            request.Data.ShareCompleted += FileDispose;
            deferral.Complete();
            return;

            async void FileDispose(DataPackage dataPackage, object o) => await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
        }
    }

    /// <summary>
    /// ReSharper disable once UnusedMember.Global
    /// </summary>
    public void SetPosition()
    {
        if (InnerTopBarPresenter.ActualWidth > TopBar.ActualWidth + 5)
        {
            if (InnerTopBarPresenter.Content is null)
            {
                OuterTopBarPresenter.Content = null;
                InnerTopBarPresenter.Content = TopBar;
            }
        }
        else if (OuterTopBarPresenter.Content is null)
        {
            InnerTopBarPresenter.Content = null;
            OuterTopBarPresenter.Content = TopBar;
        }
    }

    public override void CompleteDisposal()
    {
        base.CompleteDisposal();
        _viewModel.Dispose();
    }
}
