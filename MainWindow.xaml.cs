using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using Pzxlane.Caching.Persisting;
using Pzxlane.Core;
using Pzxlane.Data.Model.ViewModel;
using Pzxlane.Data.Model.Web.Delegation;
using Pzxlane.Objects;

namespace Pzxlane
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        private async void MainWindow_OnInitialized(object sender, EventArgs e)
        {
            await Authentication.Authenticate("2653221698@qq.com", "ly20020730");

            var url = Identity.Global.AvatarUrl;
            UserAvatar.Source = (ImageSource) new ImageSourceConverter().ConvertFrom(await HttpClientFactory.PixivImage(url).GetByteArrayAsync(""));
            UserName.Content = Identity.Global.Name;
        }
    }
}
