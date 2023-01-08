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

using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls.IllustratorView;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.Messages;
using Pixeval.Pages.IllustratorViewer;
using Pixeval.Util;
using Pixeval.Util.UI;
using IllustratorViewModel = Pixeval.UserControls.IllustratorView.IllustratorViewModel;

namespace Pixeval.Pages.Capability;

public sealed partial class FollowingsPage
{
    public FollowingsPage()
    {
        InitializeComponent();
    }


    private void FollowingsPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        _ = IllustratorView.ViewModel.ResetEngineAndFillAsync(App.AppViewModel.MakoClient.Following(App.AppViewModel.PixivUid!, PrivacyPolicy.Public));
    }

    public override void OnPageDeactivated(NavigatingCancelEventArgs e)
    {
        IllustratorView.ViewModel.Dispose();
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }

    public override void OnPageActivated(NavigationEventArgs e)
    {
        WeakReferenceMessenger.Default.Register<FollowingsPage, MainPageFrameNavigatingEvent>(this, (recipient, _) => recipient.IllustratorView.ViewModel.DataProvider.FetchEngine?.Cancel());
    }

    private void IllustratorView_OnUserTapped(object sender, TappedRoutedEventArgs e)
    {
        var context = sender.GetDataContext<IllustratorViewModel>();
        var item = IllustratorView.GetItemContainer(context);
        ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation", item);
        App.AppViewModel.RootFrameNavigate(typeof(IllustratorPage), context, new SlideNavigationTransitionInfo
        {
            Effect = SlideNavigationTransitionEffect.FromRight
        });
    }
}