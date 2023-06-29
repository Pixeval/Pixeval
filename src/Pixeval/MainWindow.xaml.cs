#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/MainWindow.xaml.cs
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

using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Misc;
using Pixeval.Pages.Login;
using AppContext = Pixeval.AppManagement.AppContext;
using Pixeval.Util.UI;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using Pixeval.Pages;
using WinUI3Utilities;

namespace Pixeval;

public sealed partial class MainWindow : INavigationModeInfo
{
    public MainWindow()
    {
        CurrentContext.Window = this;
        InitializeComponent();
    }

    public NavigationTransitionInfo? DefaultNavigationTransitionInfo { get; internal set; } = new SuppressNavigationTransitionInfo();

    /// <summary>
    /// The parameter of OnNavigatedTo is always <see cref="NavigationMode.New"/>
    /// </summary>
    public static NavigationMode? NavigationMode { get; private set; }

    public static NavigationMode? GetNavigationModeAndReset()
    {
        var mode = NavigationMode;
        NavigationMode = null;
        return mode;
    }

    private void PixevalAppRootFrame_OnLoaded(object sender, RoutedEventArgs e)
    {
        PixevalAppRootFrame.Navigate(typeof(LoginPage));
    }

    private void MainWindow_OnClosed(object sender, WindowEventArgs args)
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
        AppContext.SaveContext();
    }

    private void PixevalAppRootFrame_OnNavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        e.Handled = true;
        throw e.Exception;
    }

    private void PixevalAppRootFrame_OnNavigating(object sender, NavigatingCancelEventArgs e)
    {
        NavigationMode = e.NavigationMode;
    }

    public void ShowProgressRing()
    {
        Processing.Visibility = Visibility.Visible;
    }

    public void HideProgressRing()
    {
        Processing.Visibility = Visibility.Collapsed;
    }


    private void PixevalAppRootFrame_OnNavigated(object sender, NavigationEventArgs e)
    {
        //SetTitleBarDragRegion();

        //switch (sender)
        //{
        //    case Frame { SourcePageType: var page }:
        //        TitleBar.Visibility = page == typeof(MainPage) ? Visibility.Visible : Visibility.Collapsed;

        //        break;
        //}
    }

    private static Task GoBackToMainPageAsync()
    {
        if (App.AppViewModel.AppWindowRootFrame.Content is MainPage)
        {
            return Task.CompletedTask;
        }
        var stack = App.AppViewModel.AppWindowRootFrame.BackStack;
        while (stack.Count >= 1 && stack.Last().SourcePageType != typeof(MainPage))
        {
            stack.RemoveAt(stack.Count - 1);
        }
        App.AppViewModel.AppWindowRootFrame.GoBack();
        return App.AppViewModel.AppWindowRootFrame.AwaitPageTransitionAsync<MainPage>();
    }
}
