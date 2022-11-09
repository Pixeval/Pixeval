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

using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.Windows.AppLifecycle;
using Pixeval.Messages;
using Pixeval.Util.UI;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WinUIEx;

namespace Pixeval;

public partial class App
{

    private readonly MainWindow _mainWindow;

    public App(IServiceProvider serviceProvider)
    {
        _mainWindow = serviceProvider.GetRequiredService<MainWindow>();
        // The theme can only be changed in ctor
        
        InitializeComponent();
        RegisterUnhandledExceptionHandler();

        var appWindow = _mainWindow.GetAppWindow();
        appWindow.Title = AppConstants.AppIdentifier;
        appWindow.Show();
        appWindow.SetIcon("");
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        
    }



    private void RegisterUnhandledExceptionHandler()
    {
        UnhandledException +=  (_, args) =>
        {
            args.Handled = true;
            _mainWindow.DispatcherQueue.TryEnqueue(() => UncaughtExceptionHandler(args.Exception));
        };

        TaskScheduler.UnobservedTaskException +=  (_, args) =>
        {
            args.SetObserved();
             _mainWindow.DispatcherQueue.TryEnqueue( () => UncaughtExceptionHandler(args.Exception));
        };

        AppDomain.CurrentDomain.UnhandledException +=  (_, args) =>
        {
            if (args.ExceptionObject is Exception e)
            {
                _mainWindow.DispatcherQueue.TryEnqueue(() => UncaughtExceptionHandler(e));
            }
            else
            {
                ExitWithPushedNotification();
            }
        };

#if DEBUG
        // ReSharper disable once UnusedParameter.Local
        static void UncaughtExceptionHandler(Exception e)
        {
            Debugger.Break();
        }
#elif RELEASE
            Task UncaughtExceptionHandler(Exception e)
            {
                return ShowExceptionDialogAsync(e);
            }
#endif
    }


    public static void ExitWithPushedNotification()
    {
        WeakReferenceMessenger.Default.Send(new ApplicationExitingMessage());
        Current.Exit();
    }
}