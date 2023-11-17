#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/FollowingsPage.xaml.cs
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
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
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

    public ScrollViewer ScrollViewer => IllustratorListView.FindDescendant<ScrollViewer>()!;

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

    private void IllustratorListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IllustratorListView.IsLoaded)
        {
            return;
        }

        if (IllustratorListView.SelectedIndex is > 0 and var i && i < ViewModel.DataProvider.Source.Count && IllustratorContentViewerFrame is { } frame)
        {
            _ = frame.Navigate(typeof(IllustratorContentViewerPage), ViewModel.DataProvider.Source[i]);
        }
    }

    private async void IllustratorListView_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_illustratorListViewLoaded)
        {
            return;
        }

        _illustratorListViewLoaded = true;
        await ThreadingHelper.SpinWaitAsync(() => !ViewModel.DataProvider.Source.Any() && !ViewModel.HasNoItems);
        IllustratorListView.SelectedIndex = 0;
        _ = IllustratorContentViewerFrame.Navigate(typeof(IllustratorContentViewerPage), ViewModel.DataProvider.Source[0]);
    }

    private TeachingTip IllustratorProfileOnRequestTeachingTip() => FollowingPageTeachingTip;
}
