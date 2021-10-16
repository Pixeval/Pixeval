using System;
using System.Linq;
using Windows.ApplicationModel.Activation;
using Microsoft.UI.Xaml.Media;
using Microsoft.Windows.AppLifecycle;
using Pixeval.Activation;
using Pixeval.Util.UI;
using Pixeval.ViewModel;
using ApplicationTheme = Pixeval.Options.ApplicationTheme;
using LaunchActivatedEventArgs = Microsoft.UI.Xaml.LaunchActivatedEventArgs;

namespace Pixeval
{
    public partial class App
    {
        private const string ApplicationWideFontKey = "ContentControlThemeFontFamily";

        public static AppViewModel AppViewModel { get; private set; } = null!;
        
        public App()
        {
            // The theme can only be changed in ctor
            AppViewModel = new AppViewModel(this) {AppSetting = AppContext.LoadSetting() ?? AppSetting.CreateDefault()};
            RequestedTheme = AppViewModel.AppSetting.Theme switch
            {
                ApplicationTheme.Dark => Microsoft.UI.Xaml.ApplicationTheme.Dark,
                ApplicationTheme.Light => Microsoft.UI.Xaml.ApplicationTheme.Light,
                _ => RequestedTheme
            };
            AppInstance.GetCurrent().Activated += (_, arguments) => ActivationRegistrar.Dispatch(arguments);
            InitializeComponent();
        }



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
        /// Calculate the window size by current resolution
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