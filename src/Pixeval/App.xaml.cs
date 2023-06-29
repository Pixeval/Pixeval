#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/App.xaml.cs
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
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.Windows.AppLifecycle;
using Pixeval.Activation;
using Pixeval.AppManagement;
using Pixeval.Messages;
using Windows.Graphics;
using Pixeval.Options;
using WinUI3Utilities;
using AppContext = Pixeval.AppManagement.AppContext;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Pages.Login;
using Pixeval.Util.UI.Windowing;

namespace Pixeval;

public partial class App
{
    private const string ApplicationWideFontKey = "ContentControlThemeFontFamily";

    public App()
    {
        // The theme can only be changed in ctor
        AppViewModel = new(this) { AppSetting = AppContext.LoadConfiguration() ?? AppSetting.CreateDefault() };
        AppInstance.GetCurrent().Activated += (_, arguments) => ActivationRegistrar.Dispatch(arguments);
        InitializeComponent();
    }

    public static AppViewModel AppViewModel { get; private set; } = null!;

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        if (AppInstance.GetCurrent().GetActivatedEventArgs().Kind == ExtendedActivationKind.ToastNotification)
        {
            return;
        }

        var isProtocolActivated = AppInstance.GetCurrent().GetActivatedEventArgs() is { Kind: ExtendedActivationKind.Protocol };
        if (isProtocolActivated && AppInstance.GetInstances().Count > 1)
        {
            var notCurrent = AppInstance.GetInstances().First(ins => !ins.IsCurrent);
            await notCurrent.RedirectActivationToAsync(AppInstance.GetCurrent().GetActivatedEventArgs());
            return;
        }

        Current.Resources[ApplicationWideFontKey] = new FontFamily(AppViewModel.AppSetting.AppFontFamilyName);
        await AppKnownFolders.InitializeAsync();

        CurrentContext.Title = AppContext.AppIdentifier;
        WindowFactory.SetTheme(AppViewModel.AppSetting.Theme);

        WindowFactory.Create(out _)
            .WithLoaded((s, _) => s.To<Frame>().Navigate(typeof(LoginPage)))
            .WithClosed((_, _) => AppContext.SaveContext())
            .Init(new(AppViewModel.AppSetting.WindowWidth, AppViewModel.AppSetting.WindowHeight))
            .Activate();

        await AppViewModel.InitializeAsync(isProtocolActivated);
    }

    public static void ExitWithPushNotification()
    {
        WeakReferenceMessenger.Default.Send(new ApplicationExitingMessage());
        CurrentContext.App.Exit();
    }
}
