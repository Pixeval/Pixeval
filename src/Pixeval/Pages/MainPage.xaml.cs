#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/MainPage.xaml.cs
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

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Data;
using Pixeval.Startup.WinUI.Navigation;
using Pixeval.ViewModels;

namespace Pixeval.Pages;

internal sealed partial class MainPage : INavigationRoot
{
    private static UIElement? _connectedAnimationTarget;

    private readonly SearchHistoryRepository _searchHistoryRepository;

    public MainPageViewModel ViewModel { get; set; }

    public Frame NavigationFrame => MainPageFrame;

    public void OnPageActivated(NavigationEventArgs e)
    {
        //// dirty trick, the order of the menu items is the same as the order of the fields in MainPageTabItem
        //// since enums are basically integers, we just need a cast to transform it to the correct offset.
        //((NavigationViewItem)MainPageNavigationView.MenuItems[(int)_configurationManager.DefaultSelectedTabItem])
        //    .IsSelected = true;

        //// The application is invoked by a protocol, call the corresponding protocol handler.
        //if (_appViewModel.ConsumeProtocolActivation())
        //{
        //    ActivationRegistrar.Dispatch(AppInstance.GetCurrent().GetActivatedEventArgs());
        //}

        //WeakReferenceMessenger.Default.TryRegister<MainPage, MainPageFrameSetConnectedAnimationTargetMessage>(this,
        //    (_, message) => _connectedAnimationTarget = message.Sender);
        //WeakReferenceMessenger.Default.TryRegister<MainPage, NavigatingBackToMainPageMessage>(this,
        //    (_, message) => _illustrationViewerContent = message.IllustrationViewModel);
        //WeakReferenceMessenger.Default.TryRegister<MainPage, IllustrationTagClickedMessage>(this,
        //    async (_, message) => await PerformSearchAsync(message.Tag));

        //// Connected animation to the element located in MainPage
        //if (ConnectedAnimationService.GetForCurrentView().GetAnimation("ForwardConnectedAnimation") is { } animation)
        //{
        //    animation.Options = new DirectConnectedAnimationConfiguration();
        //    animation.TryStart(_connectedAnimationTarget ?? this);
        //    _connectedAnimationTarget = null;
        //}

        //// Scroll the content to the item that were being browsed just now
        //if (_illustrationViewerContent is not null && MainPageFrame.FindDescendant<AdaptiveGridView>() is { } gridView)
        //{
        //    gridView.ScrollIntoView(_illustrationViewerContent);
        //    _illustrationViewerContent = null;
        //}
    }
}