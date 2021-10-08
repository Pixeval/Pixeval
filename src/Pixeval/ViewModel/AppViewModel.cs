using System;
using System.Diagnostics.Tracing;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using PInvoke;
using Pixeval.CommunityToolkit;
using Pixeval.CoreApi;
using Pixeval.Messages;
using Pixeval.Misc;
using Pixeval.Options;
using Pixeval.Util.UI;
using ApplicationTheme = Pixeval.Options.ApplicationTheme;
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

        public AppWindow AppWindow = null!;

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

        public Size DesiredThumbnailSize()
        {
            return App.AppViewModel.AppSetting.ThumbnailDirection switch
            {
                ThumbnailDirection.Landscape => new Size(250, 180),
                ThumbnailDirection.Portrait => new Size(180, 250),
                _ => throw new ArgumentOutOfRangeException()
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

            AppWindow = AppWindow.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(GetMainWindowHandle()));
            AppWindow.Title = AppContext.AppIdentifier;
            AppWindow.Resize(new SizeInt32(AppSetting.WindowWidth, AppSetting.WindowHeight));
            AppWindow.Show();
            AppWindow.SetIcon(await AppContext.GetIconAbsolutePath());

            await AppContext.ClearTemporaryDirectory();
            Cache = await FileCache.CreateDefaultAsync();
        }

        public (int, int) GetAppWindowSizeTuple()
        {
            var windowSize = AppWindow.Size;
            return (windowSize.Width, windowSize.Height);
        }

        public Size GetAppWindowSize()
        {
            return AppWindow.Size.ToWinRTSize();
        }

        public Size GetDpiAwareAppWindowSize()
        {
            var dpi = User32.GetDpiForWindow(GetMainWindowHandle());
            var size = GetAppWindowSize();
            var scalingFactor = (float) dpi / 96;
            return new Size(size.Width / scalingFactor, size.Height / scalingFactor);
        }

        public (int, int) GetDpiAwareAppWindowSizeTuple()
        {
            var size = GetDpiAwareAppWindowSize();
            return ((int, int)) (size.Width, size.Height);
        }

        public void Receive(ApplicationExitingMessage message)
        {
            AppContext.SaveContext();
        }
    }
}