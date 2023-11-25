#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/FollowingsPage.xaml.cs
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
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.Messages;
using Pixeval.Pages.Misc;
using Pixeval.Controls.IllustratorView;
using Pixeval.Util;
using Pixeval.Util.Threading;

namespace Pixeval.Pages.Capability;

public sealed partial class FollowingsPage
{
    private bool _illustratorListViewLoaded;

    public FollowingsPage()
    {
        InitializeComponent();
        ViewModel = new();
    }

    public IllustratorViewViewModel ViewModel { get; }

    public FrameworkElement SelfIllustratorView => this;

    public ScrollViewer ScrollViewer => IllustratorItemsView.FindDescendant<ScrollViewer>()!;

    public override void OnPageDeactivated(NavigatingCancelEventArgs e)
    {
        ViewModel.Dispose();
        if (IllustratorContentViewerFrame.Content is IDisposable disposable)
        {
            disposable.Dispose();
        }
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }

    public override void OnPageActivated(NavigationEventArgs e)
    {
        _ = WeakReferenceMessenger.Default.TryRegister<FollowingsPage, MainPageFrameNavigatingEvent>(this, static (recipient, _) => recipient.ViewModel.DataProvider.FetchEngine?.Cancel());
        _ = ViewModel.ResetEngineAndFillAsync(App.AppViewModel.MakoClient.Following(App.AppViewModel.PixivUid!, PrivacyPolicy.Public));
    }

    private async void IllustratorListView_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_illustratorListViewLoaded)
        {
            return;
        }

        _illustratorListViewLoaded = true;
        await ThreadingHelper.SpinWaitAsync(() => !ViewModel.DataProvider.Source.Any() && !ViewModel.HasNoItems);
        // TODO: 暂时不加载页面，以后把ListView改成GridView，并占用右侧空白区域
        // _ = IllustratorContentViewerFrame.Navigate(typeof(IllustratorContentViewerPage), ViewModel.DataProvider.Source[0]);
    }

    private TeachingTip IllustratorProfileOnRequestTeachingTip() => FollowingPageTeachingTip;

    private void IllustratorListView_OnSelectionChanged(ItemsView sender, ItemsViewSelectionChangedEventArgs e)
    {
        if (IllustratorItemsView.SelectedItem is IllustratorViewModel viewModel)
            _ = IllustratorContentViewerFrame.Navigate(typeof(IllustratorContentViewerPage), viewModel);
    }

    private async Task IllustratorListView_OnLoadMoreRequested(AdvancedItemsView sender, EventArgs e)
    {
        await ViewModel.DataProvider.View.LoadMoreItemsAsync(20);
    }

    private void FollowingsPage_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var newWidth = Math.Floor(Math.Max(ActualWidth * 2 / 3, MainSplitView.CompactPaneLength));
        MainSplitView.OpenPaneLength = newWidth;
        if (MainSplitView.IsPaneOpen)
            ContentGrid.Width = newWidth;

        var count = Math.Round(newWidth / 305);
        // 10：Padding，5：Spacing
        var newItemWidth = (newWidth - 10 - 5 * (count - 1)) / count;
        var newItemHeight = newItemWidth * 8 / 15;
        // 3：空出余量
        IllustratorItemsView.MinItemWidth = newItemWidth - 3;
        IllustratorItemsView.MinItemHeight = newItemHeight;
    }

    private void MainSplitView_OnPaneAction(SplitView sender, object? args)
    {
        // not loaded
        if (IllustratorItemsView is null)
            return;

        // 10： Margin
        if (args is null)
        {
            IllustratorItemsView.LayoutType = ItemsViewLayoutType.Grid;
            ContentGrid.Width = MainSplitView.OpenPaneLength;
        }
        else // args is SplitViewPaneClosingEventArgs
        {
            IllustratorItemsView.LayoutType = ItemsViewLayoutType.VerticalStack;
            ContentGrid.Width = MainSplitView.CompactPaneLength;
        }
    }
}
