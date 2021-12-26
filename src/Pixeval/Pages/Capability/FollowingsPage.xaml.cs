#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/FollowingsPage.xaml.cs
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
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Controls.IllustratorView;
using Pixeval.Pages.IllustratorViewer;

namespace Pixeval.Pages.Capability;

public sealed partial class FollowingsPage
{
    private readonly FollowingsPageViewModel _viewModel = new();


    public FollowingsPage()
    {
        InitializeComponent();
    }


    private async void FollowingsPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.LoadFollowings();
    }

    private void IllustratorView_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is IllustratorView view)
        {
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation", view.Avatar);
            App.AppViewModel.RootFrameNavigate(typeof(IllustratorPage), Tuple.Create<UIElement, IllustratorViewModel>(view.Avatar!, view.ViewModel), new SlideNavigationTransitionInfo
            {
                Effect = SlideNavigationTransitionEffect.FromRight
            });
        }
    }
}