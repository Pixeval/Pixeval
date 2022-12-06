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

using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using LiteDB.Async;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.CoreApi;
using Pixeval.CoreApi.Models;
using Pixeval.Misc;
using Pixeval.Pages.Login;
using Pixeval.Storage;

namespace Pixeval;

internal sealed partial class MainWindow
{
    private readonly ISessionRefresher _sessionRefresher;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    public MainWindow(ISessionRefresher sessionRefresher,
        IHostApplicationLifetime hostApplicationLifetime)
    {
        _sessionRefresher = sessionRefresher;
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppTitleBarText.Text = AppConstants.AppIdentifier;
        _hostApplicationLifetime = hostApplicationLifetime;
    }

    private void MainWindow_OnClosed(object sender, WindowEventArgs args)
    {
        _hostApplicationLifetime.StopApplication();
        Environment.Exit(0);
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        await _sessionRefresher.GetAccessTokenAsync();
    }
}