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
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Messages;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WinUIEx;

namespace Pixeval;

[LocalizedStringResources()]
public partial class App
{
    private MainWindow _mainWindow;

    public App(IServiceProvider serviceProvider)
    {
        _mainWindow = serviceProvider.GetRequiredService<MainWindow>();
        InitializeComponent();
        var appWindow = _mainWindow.GetAppWindow();
        appWindow.Title = SR.AppName;
        appWindow.Show();
        appWindow.SetIcon("");

    }

    private void RegisterUnhandledExceptionHandler()
    {
        UnhandledException += (_, args) =>
        {
            args.Handled = true;
            _mainWindow.DispatcherQueue.TryEnqueue(() => UncaughtExceptionHandler(args.Exception));
        };

        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            args.SetObserved();
            _mainWindow.DispatcherQueue.TryEnqueue(() => UncaughtExceptionHandler(args.Exception));
        };

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
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
            void UncaughtExceptionHandler(Exception e)
            {
                //return ShowExceptionDialogAsync(e);
            }
#endif
    }


    public static void ExitWithPushedNotification()
    {
        WeakReferenceMessenger.Default.Send(new ApplicationExitingMessage());
        Current.Exit();
    }
}