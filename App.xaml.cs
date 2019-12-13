using System.Windows;
using Pixeval.Persisting;

namespace Pixeval
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
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