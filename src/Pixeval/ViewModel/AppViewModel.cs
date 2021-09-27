using System;
using System.Diagnostics.Tracing;
using System.Threading.Tasks;
using Windows.Foundation;
using CommunityToolkit.WinUI;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.CoreApi;
using Pixeval.Messages;
using Pixeval.Misc;
using Pixeval.Util.UI;

#if DEBUG
using System.Diagnostics;
#endif

namespace Pixeval.ViewModel
{
    public class AppViewModel : AutoActivateObservableRecipient, IRecipient<ApplicationExitingMessage>
    {
        public AppViewModel(App app)
        {
            App = app;
        }

        public App App { get; }

        public MainWindow Window = null!;

        public Frame AppWindowRootFrame => Window.PixevalAppRootFrame;

        public MakoClient MakoClient { get; set; } = null!; // The null-state of MakoClient is transient

        public AppSetting AppSetting { get; set; } = null!;

        public FileCache Cache { get; private set; } = null!;

        public ElementTheme AppRootFrameTheme => AppWindowRootFrame.RequestedTheme;

        public string? PixivUid => MakoClient.Session.Id;

        public IntPtr GetMainWindowHandle()
        {
            return Window.GetWindowHandle();
        }

        public void SwitchTheme(ApplicationTheme theme)
        {
            Window.PixevalAppRootFrame.RequestedTheme = theme switch
            {
                ApplicationTheme.Dark => ElementTheme.Dark,
                ApplicationTheme.Light => ElementTheme.Light,
                ApplicationTheme.SystemDefault => ElementTheme.Default,
                _ => throw new ArgumentOutOfRangeException(nameof(theme), theme, null)
            };
        }

        public void RootFrameNavigate(Type type, object parameter, NavigationTransitionInfo infoOverride)
        {
            AppWindowRootFrame.Navigate(type, parameter, infoOverride);
        }

        public void RootFrameNavigate(Type type, object parameter)
        {
            AppWindowRootFrame.Navigate(type, parameter);
        }

        public void RootFrameNavigate(Type type)
        {
            AppWindowRootFrame.Navigate(type);
        }

        private void RegisterUnhandledExceptionHandler()
        {
            App.UnhandledException += async (_, args) =>
            {
                args.Handled = true;
                await Window.DispatcherQueue.EnqueueAsync(async () => await UncaughtExceptionHandler(args.Exception));
            };

            TaskScheduler.UnobservedTaskException += async (_, args) =>
            {
                args.SetObserved();
                await Window.DispatcherQueue.EnqueueAsync(async () => await UncaughtExceptionHandler(args.Exception));
            };

            AppDomain.CurrentDomain.UnhandledException += async (_, args) =>
            {
                if (args.ExceptionObject is Exception e)
                {
                    await Window.DispatcherQueue.EnqueueAsync(async () => await UncaughtExceptionHandler(e));
                }
                else
                {
                    ExitWithPushedNotification();
                }
            };

#if DEBUG
            static Task UncaughtExceptionHandler(Exception e)
            {
                Debugger.Break();
                return Task.CompletedTask;
            }
#elif RELEASE
            static async Task UncaughtExceptionHandler(Exception e)
            {
                await MessageDialogBuilder.CreateAcknowledgement(Window, MiscResources.ExceptionEncountered, e.ToString()).ShowAsync();
                ExitWithPushedNotification();

            }
#endif
        }


        /// <summary>
        /// Exit the notification after pushing an <see cref="ApplicationExitingMessage"/>
        /// to the <see cref="EventChannel"/>
        /// </summary>
        /// <returns></returns>
        public void ExitWithPushedNotification()
        {
            WeakReferenceMessenger.Default.Send(new ApplicationExitingMessage());
            Application.Current.Exit();
        }

        public async Task InitializeAsync()
        {
            RegisterUnhandledExceptionHandler();
            await AppContext.WriteLogoIcoIfNotExist();
            Window = new MainWindow();
            Window.SetWindowSize(AppSetting.WindowWidth, AppSetting.WindowHeight);
            Window.Activate();
            await AppContext.ClearTemporaryDirectory();
            Cache = await FileCache.CreateDefaultAsync();
        }

        /// <summary>
        /// Calculate the window size by current resolution
        /// </summary>
        public (int, int) PredetermineEstimatedWindowSize()
        {
            return UIHelper.GetScreenSize() switch
            {
                // 这 就 是 C #
                ( >= 2560, >= 1440) => (1600, 900),
                ( > 1600, > 900) => (1280, 720),
                _ => (800, 600)
            };
        }

        public (int, int) GetAppWindowSizeTuple()
        {
            return App.AppViewModel.Window.SizeTuple();
        }

        public Size GetAppWindowSize()
        {
            return App.AppViewModel.Window.Size();
        }

        public void Receive(ApplicationExitingMessage message)
        {
            AppContext.SaveContext();
        }
    }
}