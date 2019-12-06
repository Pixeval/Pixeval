using System;
using System.Diagnostics;
using System.Windows;
using Pzxlane.Caching.Persisting;
using Pzxlane.Core;
using Pzxlane.Data.Model.Web.Delegation;

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

            Debug.WriteLine(Identity.Global.ToString());

            await foreach (var illust in PixivClient.Instance.Upload("333556"))
            {
                Debug.WriteLine((await illust).Id);
            }
        }
    }
}
