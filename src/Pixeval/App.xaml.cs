using Mako;
using Microsoft.UI.Xaml;
using Pixeval.Interop;
using Pixeval.Util;
using WinRT;

namespace Pixeval
{
    public partial class App
    {
        public static MakoClient? PixevalAppClient { get; set; }

        public static MainWindow Window = null!;

        public App()
        {
            InitializeComponent();
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        { 
            Window = new MainWindow();
            var windowHandle = Window.As<IWindowNative>(); // see https://github.com/microsoft/WinUI-3-Demos/blob/master/src/Build2020Demo/DemoBuildCs/DemoBuildCs/DemoBuildCs/App.xaml.cs
            UIHelper.SetWindowSize(windowHandle.WindowHandle, 800, 600);
            Window.Activate();
            await AppContext.CopyLoginProxyIfRequired();
        }
    }
}
