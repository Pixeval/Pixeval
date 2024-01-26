#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/LoginPage.xaml.cs
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
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Controls;
using Pixeval.Options;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using WinUI3Utilities;
using AppContext = Pixeval.AppManagement.AppContext;

namespace Pixeval.Pages.Login;

public sealed partial class LoginPage
{
    private readonly BrowserInfo[] _browserInfos =
    [
        BrowserInfo.Chrome,
        BrowserInfo.Edge,
        BrowserInfo.Firefox,
        BrowserInfo.WebView2
    ];

    private readonly LoginPageViewModel _viewModel;

    public LoginPage()
    {
        _viewModel = new(this);
        InitializeComponent();
    }

    private async void ItemsView_OnItemInvoked(ItemsView sender, ItemsViewItemInvokedEventArgs args)
    {
        try
        {
            var browserInfo = sender.SelectedItem.To<BrowserInfo>();
            if (await (browserInfo.Type switch
            {
                AvailableBrowserType.Chrome => _viewModel.BrowserLoginAsync(BrowserInfo.Chrome, this, Navigated),
                AvailableBrowserType.Edge => _viewModel.BrowserLoginAsync(BrowserInfo.Edge, this, Navigated),
                AvailableBrowserType.Firefox => _viewModel.BrowserLoginAsync(BrowserInfo.Firefox, this, Navigated),
                AvailableBrowserType.WebView2 => _viewModel.WebView2LoginAsync(this, Navigated),
                _ => ThrowHelper.ArgumentOutOfRange<AvailableBrowserType, Task<string?>>(browserInfo.Type)
            }) is { } error)
            {
                _viewModel.AdvancePhase(LoginPageViewModel.LoginPhaseEnum.WaitingForUserInput);
                _ = await this.CreateAcknowledgementAsync(LoginPageResources.ErrorWhileLoggingInTitle, error);
            }
        }
        catch (Exception exception)
        {
            _ = await this.CreateAcknowledgementAsync(LoginPageResources.ErrorWhileLoggingInTitle,
                    LoginPageResources.ErrorWhileLogginInContentFormatted.Format(exception + "\n" + exception.StackTrace));
            Application.Current.Exit();
        }

        return;
        void Navigated()
        {
            if (App.AppViewModel.MakoClient == null!)
                ThrowHelper.Exception();

            _ = DispatcherQueue.TryEnqueue(() =>
            {
                _viewModel.AdvancePhase(LoginPageViewModel.LoginPhaseEnum.SuccessNavigating);
                NavigateParent<MainPage>(null, new DrillInNavigationTransitionInfo());
                AppContext.SaveContext();
            });
        }
    }

    private async void LoginPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_viewModel.CheckRefreshAvailable())
            {
                await _viewModel.RefreshAsync();
                NavigateParent<MainPage>(null, new DrillInNavigationTransitionInfo());
            }
            else
            {
                _viewModel.AdvancePhase(LoginPageViewModel.LoginPhaseEnum.WaitingForUserInput);
                _viewModel.IsFinished = true;
            }
        }
        catch (Exception exception)
        {
            _ = await this.CreateAcknowledgementAsync(LoginPageResources.ErrorWhileLoggingInTitle,
                    LoginPageResources.ErrorWhileLogginInContentFormatted.Format(exception.StackTrace));
            Application.Current.Exit();
        }
    }

    // ReSharper disable once MemberCanBeMadeStatic.Local
    private Type LoginProxyOption => typeof(LoginProxyOption);
}
