using System;
using System.Threading.Tasks;
using System.Windows;
using Pixeval.Persisting;

namespace Pixeval
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            if (Dispatcher != null)
                Dispatcher.UnhandledException += (sender, args) => DispatcherOnUnhandledException(args.Exception);
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => DispatcherOnUnhandledException((Exception) args.ExceptionObject);
            TaskScheduler.UnobservedTaskException += (sender, args) => DispatcherOnUnhandledException(args.Exception);
        }

        private static void DispatcherOnUnhandledException(Exception e)
        {
#if DEBUG
            Trace.WriteLine(e.Message);
#endif
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await Settings.Global.Restore();
            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await Settings.Global.Store();
            if (!PixevalEnvironment.LogoutExit && Identity.Global.AccessToken != null) await Identity.Global.Store();
            base.OnExit(e);
        }
    }
}