// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Mako.Global.Enum;
using Pixeval.Filters;
using Pixeval.Util;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using Windows.System;
using WinUI3Utilities;

namespace Pixeval.Controls;

/// <summary>
/// 所有插画集合通用的容器
/// </summary>
public partial class WorkContainer : IScrollViewHost
{
    /// <summary>
    /// The command elements that will appear at the left of the TopCommandBar
    /// </summary>
    public ObservableCollection<UIElement> CommandBarElements { get; } = [];

    public ObservableCollection<ICommandBarElement> PrimaryCommandsSupplements { get; } = [];

    public ObservableCollection<ICommandBarElement> SecondaryCommandsSupplements { get; } = [];

    public WorkContainer()
    {
        InitializeComponent();
        CommandBarElements.CollectionChanged += (_, e) =>
        {
            if (e is { Action: NotifyCollectionChangedAction.Add, NewItems: { } newItems })
                foreach (UIElement argsNewItem in newItems)
                    ExtraCommandsBar.Children.Insert(0, argsNewItem);
            else
                ThrowHelper.Argument(e, "This collection does not support operations except the Add");
        };
        PrimaryCommandsSupplements.CollectionChanged += (_, args) => AddCommandCallback(args, CommandBar.PrimaryCommands);
        SecondaryCommandsSupplements.CollectionChanged += (_, args) => AddCommandCallback(args, CommandBar.SecondaryCommands);

        // 由于经常在Loaded之前ResetEngine，而WorkView里是在ResetEngine中决定View的属性的
        // 而写在XAML中的属性会在Load之后才会被设置，所以我们不在XAML中设置而是手动
        WorkView.LayoutType = App.AppViewModel.AppSettings.ItemsViewLayoutType;
        WorkView.ThumbnailDirection = App.AppViewModel.AppSettings.ThumbnailDirection;
        _ = WorkView.Focus(FocusState.Programmatic);
        return;

        static void AddCommandCallback(NotifyCollectionChangedEventArgs e, ICollection<ICommandBarElement> commands)
        {
            if (e is { Action: NotifyCollectionChangedAction.Add, NewItems: { } newItems })
                foreach (UIElement argsNewItem in newItems)
                    commands.Add(argsNewItem.To<ICommandBarElement>());
            else
                ThrowHelper.Argument(e, "This collection does not support operations except the Add");
        }
    }

    public SimpleWorkType Type => WorkView.Type;

    public ISortableEntryViewViewModel ViewModel => WorkView.ViewModel;

    private void SelectAllToggleButton_OnClicked(object sender, RoutedEventArgs e)
    {
        WorkView.AdvancedItemsView.SelectAll();
    }

    private void SortOptionComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e) => SetSortOption();

    private void WorkView_OnViewModelChanged(WorkView sender, ISortableEntryViewViewModel args) => SetSortOption();

    public void SetSortOption()
    {
        if (WorkView is { ViewModel: { } vm } && SortOptionComboBox.ItemSelected)
        {
            switch (MakoHelper.GetSortDescriptionForIllustration(SortOptionComboBox.GetSelectedItem<WorkSortOption>()))
            {
                case { } desc:
                    vm.SetSortDescription(desc);
                    break;
                default:
                    // reset the view so that it can resort its item to the initial order
                    vm.ClearSortDescription();
                    break;
            }
            ScrollToTop();
        }
    }

    private void ScrollToTop()
    {
        if (WorkView.ScrollView is { } scrollView)
            _ = scrollView.ScrollTo(0, 0);
    }

    private void AddAllToBookmarkButton_OnClicked(object sender, RoutedEventArgs e) => AddToBookmarkTeachingTip.IsOpen = true;

    private async void SaveAllButton_OnClicked(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedEntries.Count >= 20 && await this.CreateOkCancelAsync(WorkContainerResources.SelectedTooManyItemsTitle,
                WorkContainerResources.SelectedTooManyItemsForSaveContent) is not ContentDialogResult.Primary)
            return;

        foreach (var i in ViewModel.SelectedEntries)
            i.SaveCommand.Execute(null);

        this.InfoGrowl(WorkContainerResources.DownloadItemsQueuedFormatted.Format(ViewModel.SelectedEntries.Count));
    }

    private async void OpenAllInBrowserButton_OnClicked(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedEntries.Count > 15 && await this.CreateOkCancelAsync(
                WorkContainerResources.SelectedTooManyItemsTitle,
                WorkContainerResources.SelectedTooManyItemsForOpenInBrowserContent) is not ContentDialogResult.Primary)
            return;
        foreach (var selectedEntry in ViewModel.SelectedEntries)
        {
            _ = await Launcher.LaunchUriAsync(selectedEntry.WebUri);
        }
    }

    private async void AddToBookmarkTeachingTip_OnCloseButtonClick(TeachingTip sender, object args)
    {
        if (ViewModel.SelectedEntries.Count > 5 &&
            await this.CreateOkCancelAsync(WorkContainerResources.SelectedTooManyItemsForBookmarkTitle,
                WorkContainerResources.SelectedTooManyItemsForBookmarkContent) is not ContentDialogResult.Primary)
            return;

        foreach (var i in ViewModel.SelectedEntries)
            i.AddToBookmarkCommand.Execute((BookmarkTagSelector.SelectedTags, BookmarkTagSelector.IsPrivate, null as object));

        if (ViewModel.SelectedEntries.Count is var c and > 0)
            this.SuccessGrowl(WorkContainerResources.AddedAllToBookmarkContentFormatted.Format(c));
    }

    private void CancelSelectionButton_OnClicked(object sender, RoutedEventArgs e)
    {
        WorkView.AdvancedItemsView.DeselectAll();
    }

    private void Content_OnLoading(FrameworkElement sender, object e)
    {
        var teachingTip = sender.GetTag<TeachingTip>();
        var appBarButton = teachingTip.GetTag<AppBarButton>();
        teachingTip.Target = appBarButton.IsInOverflow ? null : appBarButton;
    }

    private void FilterAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(sender.Text))
        {
            ViewModel.Filter = null;
            ViewModel.ViewRange = Range.All;
        }
        else
            PerformSearch(sender.Text);
    }

    public void PerformSearch(string text)
    {
        try
        {
            var sequence = Parser.Parse(text, out var index);

            ViewModel.Filter = o => o.Filter(sequence);
            ViewModel.ViewRange = index?.NarrowRange ?? Range.All;
        }
        catch (Exception e)
        {
            this.ErrorGrowl(MacroParserResources.FilterQueryError, e.Message);
        }
    }

    public ScrollView ScrollView => WorkView.ScrollView;
}
