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
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.Messages;
using Pixeval.Pages.Misc;
using Pixeval.Util;
using Pixeval.Util.UI;
using WinUI3Utilities;

namespace Pixeval.Pages.Capability;

public sealed partial class FollowingsPage
{
    private readonly SolidColorBrush _backgroundBrush;
    private readonly IllustratorViewViewModel _viewModel = new();
    private const int CompactPaneLength = 300;

    public FollowingsPage()
    {
        InitializeComponent();
        _backgroundBrush = Resources["SystemControlBackgroundChromeMediumLowBrush"].To<SolidColorBrush>().WithAlpha(64);
    }

    public FrameworkElement SelfIllustratorView => this;

    public override void OnPageDeactivated(NavigatingCancelEventArgs e)
    {
        _viewModel.Dispose();
        if (IllustratorContentViewerFrame.Content is IDisposable disposable)
        {
            disposable.Dispose();
        }
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }

    public override void OnPageActivated(NavigationEventArgs e)
    {
        _ = WeakReferenceMessenger.Default.TryRegister<FollowingsPage, MainPageFrameNavigatingEvent>(this, static (recipient, _) => recipient._viewModel.DataProvider.FetchEngine?.Cancel());
        _viewModel.ResetEngine(App.AppViewModel.MakoClient.Following(App.AppViewModel.PixivUid, PrivacyPolicy.Public));
        // 此时没有Navigate，右边没有内容
        // ClipGeometry.Rect = new(MainSplitView.OpenPaneLength - CompactPaneLength, 0, int.MaxValue, int.MaxValue);
    }

    private TeachingTip IllustratorItemOnRequestTeachingTip() => FollowingsPageTeachingTip;

    private void IllustratorItemsView_OnSelectionChanged(ItemsView sender, ItemsViewSelectionChangedEventArgs e)
    {
        MainSplitView.IsPaneOpen = false;
        if (IllustratorItemsView.SelectedItem is IllustratorItemViewModel viewModel)
            _ = IllustratorContentViewerFrame.Navigate(typeof(IllustratorContentViewerPage), viewModel);
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

        var openPaneLength = MainSplitView.OpenPaneLength;
        // 10： Margin
        if (args is null)
        {
            IllustratorItemsView.LayoutType = ItemsViewLayoutType.Grid;
            ContentGrid.Width = openPaneLength;
            ClipGeometry.Rect = new(openPaneLength - CompactPaneLength, 0, int.MaxValue, int.MaxValue);
        }
        else // args is SplitViewPaneClosingEventArgs
        {
            IllustratorItemsView.LayoutType = ItemsViewLayoutType.VerticalStack;
            ContentGrid.Width = CompactPaneLength;
            ClipGeometry.Rect = new(0, 0, int.MaxValue, int.MaxValue);
        }
    }
}
