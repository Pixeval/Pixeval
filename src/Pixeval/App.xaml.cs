#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/App.xaml.cs
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
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.Windows.AppLifecycle;
using Pixeval.Activation;
using Pixeval.Util.UI;
using Pixeval.ViewModel;
using ApplicationTheme = Pixeval.Options.ApplicationTheme;

namespace Pixeval
{
    public partial class App
    {
        private const string ApplicationWideFontKey = "ContentControlThemeFontFamily";

        public App()
        {
            // The theme can only be changed in ctor
            AppViewModel = new AppViewModel(this) { AppSetting = AppContext.LoadSetting() ?? AppSetting.CreateDefault() };
            RequestedTheme = AppViewModel.AppSetting.Theme switch
            {
                ApplicationTheme.Dark => Microsoft.UI.Xaml.ApplicationTheme.Dark,
                ApplicationTheme.Light => Microsoft.UI.Xaml.ApplicationTheme.Light,
                _ => RequestedTheme
            };
            AppInstance.GetCurrent().Activated += (_, arguments) => ActivationRegistrar.Dispatch(arguments);
            InitializeComponent();
        }

        public static AppViewModel AppViewModel { get; private set; } = null!;

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            var isProtocolActivated = AppInstance.GetCurrent().GetActivatedEventArgs() is { Kind: ExtendedActivationKind.Protocol };
            if (isProtocolActivated && AppInstance.GetInstances().Count > 1)
            {
                var notCurrent = AppInstance.GetInstances().First(ins => !ins.IsCurrent);
                await notCurrent.RedirectActivationToAsync(AppInstance.GetCurrent().GetActivatedEventArgs());
                return;
            }

            Current.Resources[ApplicationWideFontKey] = new FontFamily(AppViewModel.AppSetting.AppFontFamilyName);
            await AppViewModel.InitializeAsync(isProtocolActivated);
        }

        /// <summary>
        ///     Calculate the window size by current resolution
        /// </summary>
        public static (int, int) PredetermineEstimatedWindowSize()
        {
            return UIHelper.GetScreenSize() switch
            {
                // 这 就 是 C #
                (>= 2560, >= 1440) => (1600, 900),
                (> 1600, > 900) => (1280, 720),
                _ => (800, 600)
            };
        }
    }
}