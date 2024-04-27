#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IllustrationContainer.xaml.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Pixeval.Controls.FlyoutContent;
using Pixeval.Util;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using WinUI3Utilities;
using Pixeval.Misc;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.Controls.Windowing;

namespace Pixeval.Controls;

/// <summary>
/// 所有插画集合通用的容器
/// </summary>
public partial class WorkContainer : IScrollViewProvider
{
    /// <summary>
    /// The command elements that will appear at the left of the TopCommandBar
    /// </summary>
    public ObservableCollection<UIElement> CommandBarElements { get; } = [];

    public ObservableCollection<ICommandBarElement> PrimaryCommandsSupplements { get; } = [];

    public ObservableCollection<ICommandBarElement> SecondaryCommandsSupplements { get; } = [];

    public ulong HWnd => WindowFactory.GetWindowForElement(this).HWnd;

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

    private void WorkContainer_OnLoaded(object sender, RoutedEventArgs e) => _ = WorkView.Focus(FocusState.Programmatic);

    private FilterSettings _lastFilterSettings = FilterSettings.Default;

    private void SelectAllToggleButton_OnTapped(object sender, TappedRoutedEventArgs e)
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

    private void AddAllToBookmarkButton_OnTapped(object sender, TappedRoutedEventArgs e) => AddToBookmarkTeachingTip.IsOpen = true;

    private async void SaveAllButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        if (ViewModel.SelectedEntries.Count >= 20 && await this.CreateOkCancelAsync(WorkContainerResources.SelectedTooManyItemsTitle,
                WorkContainerResources.SelectedTooManyItemsForSaveContent) is not ContentDialogResult.Primary)
            return;

        foreach (var i in ViewModel.SelectedEntries)
            i.SaveCommand.Execute(null);

        HWnd.InfoGrowl(WorkContainerResources.DownloadItemsQueuedFormatted.Format(ViewModel.SelectedEntries.Count));
    }

    private async void OpenAllInBrowserButton_OnTapped(object sender, TappedRoutedEventArgs e)
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
            HWnd.SuccessGrowl(WorkContainerResources.AddedAllToBookmarkContentFormatted.Format(c));
    }

    private void CancelSelectionButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        WorkView.AdvancedItemsView.DeselectAll();
    }

    private void OpenConditionDialogButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        FilterTeachingTip.IsOpen = true;
    }

    private void FilterTeachingTip_OnActionButtonClick(TeachingTip sender, object args)
    {
        FilterContent.Reset();
    }

    private void Content_OnLoading(FrameworkElement sender, object e)
    {
        var teachingTip = sender.GetTag<TeachingTip>();
        var appBarButton = teachingTip.GetTag<AppBarButton>();
        teachingTip.Target = appBarButton.IsInOverflow ? null : appBarButton;
    }

    private void FilterTeachingTip_OnCloseButtonClick(TeachingTip sender, object args)
    {
        if (FilterContent.GetFilterSettings is not (
            var includeTags,
            var excludeTags,
            var leastBookmark,
            var maximumBookmark,
            _, // TODO user group name
            var illustratorName,
            var illustratorId,
            var illustrationName,
            var illustrationId,
            var publishDateStart,
            var publishDateEnd) filterSettings)
            return;

        if (filterSettings == _lastFilterSettings)
        {
            return;
        }

        _lastFilterSettings = filterSettings;

        ViewModel.Filter = o =>
        {
            var stringTags = o.Tags.Select(t => t.Name).ToArray();
            var result =
                ExamineExcludeTags(stringTags, excludeTags)
                && ExamineIncludeTags(stringTags, includeTags)
                && o.TotalBookmarks >= leastBookmark
                && o.TotalBookmarks <= maximumBookmark
                && illustrationName.Match(o.Title)
                && illustratorName.Match(o.User.Name)
                && (illustratorId is -1 || illustratorId == o.User.Id)
                && illustrationId is -1 || illustrationId == o.Id
                && o.PublishDate >= publishDateStart
                && o.PublishDate <= publishDateEnd;
            return result;
        };
        return;

        static bool ExamineExcludeTags(IEnumerable<string> tags, IEnumerable<Token> predicates)
            => predicates.Aggregate(true, (acc, token) => acc && tags.None(token.Match));

        static bool ExamineIncludeTags(ICollection<string> tags, IEnumerable<Token> predicates)
            => tags.Count is 0 || predicates.Aggregate(true, (acc, token) => acc && tags.Any(token.Match));
    }

    private void FastFilterAutoSuggestBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        PerformSearch(sender.Text);
    }

    public void PerformSearch(string text)
    {
        ViewModel.Filter = text.IsNullOrBlank()
            ? null
            : o => o.Id.ToString().Contains(text)
                   || o.Tags.Any(x => x.Name.Contains(text) || (x.TranslatedName?.Contains(text) ?? false))
                   || o.Title.Contains(text);
    }

    public ScrollView ScrollView => WorkView.ScrollView;
}
