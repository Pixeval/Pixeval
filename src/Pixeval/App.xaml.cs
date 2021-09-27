using Microsoft.UI.Xaml;
using Pixeval.Util.UI;
using Pixeval.ViewModel;
using ApplicationTheme = Pixeval.Options.ApplicationTheme;

namespace Pixeval
{
    public partial class App
    {
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
            InitializeComponent();
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            await AppViewModel.InitializeAsync();
        }

        /// <summary>
        /// Calculate the window size by current resolution
        /// </summary>
        public static (int, int) PredetermineEstimatedWindowSize()
        {
            return UIHelper.GetScreenSize() switch
            {
                // 这 就 是 C #
                ( >= 2560, >= 1440) => (1600, 900),
                ( > 1600, > 900) => (1280, 720),
                _ => (800, 600)
            };
        }
    }
}