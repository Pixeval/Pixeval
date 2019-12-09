using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using Pixeval.Caching.Persisting;
using Pixeval.Objects;

namespace Pixeval
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
            await AddUserNameAndAvatar();
        }

        private async Task AddUserNameAndAvatar()
        {
            if (!Identity.Global.AvatarUrl.IsNullOrEmpty() && !Identity.Global.Name.IsNullOrEmpty())
            {
                UserName.Text = Identity.Global.Name;
                UserAvatar.Source = await PixivImage.FromUrl(Identity.Global.AvatarUrl);
            }
        }

        private void MainWindow_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            ToLoseFocus.Focus();
            DownloadListTab.IsSelected = false;
        }

        private void SettingDialog_OnDialogClosing(object sender, DialogClosingEventArgs e) => SettingsTab.IsSelected = false;
    }
}
