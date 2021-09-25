using Microsoft.UI.Xaml;
using Pixeval.ViewModel;

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
    }
}