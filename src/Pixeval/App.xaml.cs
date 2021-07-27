using System;
using System.Threading.Tasks;
using Mako;
using Microsoft.UI.Xaml;
using Pixeval.Events;
using Pixeval.Util;

namespace Pixeval
{
    public partial class App
    {
        public static MainWindow Window = null!;

        public static MakoClient MakoClient { get; set; } = null!; // The null-state of MakoClient is transient

        public static AppSetting AppSetting { get; set; } = null!;

        public App()
        {
            InitializeComponent();
            RegisterUnhandledExceptionHandler();
            AppSetting = AppContext.LoadConfiguration() ?? AppSetting.CreateDefault();
            RequestedTheme = AppSetting.Theme switch
            {
                ApplicationTheme.Dark  => Microsoft.UI.Xaml.ApplicationTheme.Dark,
                ApplicationTheme.Light => Microsoft.UI.Xaml.ApplicationTheme.Dark,
                _                      => RequestedTheme
            };
        }

        private void RegisterUnhandledExceptionHandler()
        {
            UnhandledException += (_, args) =>
            {
                args.Handled = true; 
                UncaughtExceptionHandler(args.Exception);
            };
            TaskScheduler.UnobservedTaskException += (_, args) =>
            { 
                UncaughtExceptionHandler(args.Exception);
                args.SetObserved();
            };
            AppDomain.CurrentDomain.UnhandledException += async (_, args) =>
            {
                if (args.ExceptionObject is Exception e)
                {
                    UncaughtExceptionHandler(e);
                }

                if (args.IsTerminating)
                {
                    await ExitWithPushedNotification();
                }
            };

            static void UncaughtExceptionHandler(Exception e)
            {
                Window.DispatcherQueue.TryEnqueue(async () =>
                {
                    await MessageDialogBuilder.CreateAcknowledgement(Window, MiscResources.ExceptionEncountered, e.ToString()).ShowAsync();
                    await ExitWithPushedNotification();
                });
            }
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        { 
            Window = new MainWindow();
            Window.SetWindowSize(800, 600);
            Window.Activate();
        }

        /// <summary>
        /// Exit the notification after pushing an <see cref="ApplicationExitingEvent"/>
        /// to the <see cref="EventChannel"/>
        /// </summary>
        /// <returns></returns>
        public static async Task ExitWithPushedNotification()
        {
            await EventChannel.Default.PublishAsync(new ApplicationExitingEvent());
            await Task.Delay(200); // well...just wait a second to let those subscribers handle the event
        }
    }
}
