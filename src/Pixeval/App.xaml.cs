using System;
using System.Threading.Tasks;
using Windows.Storage;
using CommunityToolkit.WinUI.Helpers;
using Mako;
using Microsoft.UI.Xaml;

namespace Pixeval
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App
    {
        public static MakoClient? PixevalAppClient { get; set; }

        private MainWindow? _window;

        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            _window.Activate();
        }

        public static async Task<byte[]> GetResourceBytes(string path)
        {
            return await (await StorageFile.GetFileFromApplicationUriAsync(new Uri(path))).ReadBytesAsync();
        }
    }
}
