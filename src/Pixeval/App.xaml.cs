using System;
using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.CoreApi;
using Pixeval.Events;
using Pixeval.Interop;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using WinRT;

#if DEBUG
using System.Diagnostics;
#endif

namespace Pixeval
{
    public partial class App
    {
        public static MainWindow Window = null!;

        public static Frame AppWindowRootFrame => Window.PixevalAppRootFrame;

        public static MakoClient MakoClient { get; set; } = null!; // The null-state of MakoClient is transient

        public static AppSetting AppSetting { get; set; } = null!;

        public static FileCache Cache { get; private set; } = null!;

        public static string? Uid => MakoClient.Session.Id;

        public App()
        {
            InitializeComponent();
            RegisterUnhandledExceptionHandler();
            AppSetting = AppContext.LoadConfiguration() ?? AppSetting.CreateDefault();
            RequestedTheme = AppSetting.Theme switch
            {
                ApplicationTheme.Dark  => Microsoft.UI.Xaml.ApplicationTheme.Dark,
                ApplicationTheme.Light => Microsoft.UI.Xaml.ApplicationTheme.Light,
                _                      => RequestedTheme
            };
        }

        public static IntPtr GetMainWindowHandle()
        {
            return Window.As<IWindowNative>().WindowHandle;
        }

        public static void RootFrameNavigate(Type type, object parameter, NavigationTransitionInfo infoOverride)
        {
            AppWindowRootFrame.Navigate(type, parameter, infoOverride);
        }

        public static void RootFrameNavigate(Type type, object parameter)
        {
            AppWindowRootFrame.Navigate(type, parameter);
        }

        public static void RootFrameNavigate(Type type)
        {
            AppWindowRootFrame.Navigate(type);
        }

        private void RegisterUnhandledExceptionHandler()
        {
            UnhandledException += async (_, args) =>
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

                Debugger.Break();

                await MessageDialogBuilder.CreateAcknowledgement(Window, MiscResources.ExceptionEncountered, e.ToString()).ShowAsync();
                ExitWithPushedNotification();

            }
#endif
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            await AppContext.WriteLogoIcoIfNotExist();
            Window = new MainWindow();
            Window.SetWindowSize(800, 600);
            Window.Activate();
            await AppContext.ClearTemporaryDirectory();
            Cache = await FileCache.CreateDefaultAsync();
        }

        /// <summary>
        /// Exit the notification after pushing an <see cref="ApplicationExitingEvent"/>
        /// to the <see cref="EventChannel"/>
        /// </summary>
        /// <returns></returns>
        public static void ExitWithPushedNotification()
        {
            EventChannel.Default.Publish(new ApplicationExitingEvent());
            Current.Exit();
        }
    }
}