using System;
using System.Diagnostics;
using System.Windows;
using Pzxlane.Caching.Persisting;
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
            Debug.WriteLine("auth");
            await Authentication.Authenticate("2653221698@qq.com", "ly20020730");
            Debug.WriteLine(Identity.Global.ToString());
            foreach (var (key, value) in DnsResolver.DnsCache.Value)
            {
                foreach (var ipAddress in value)
                {
                    Debug.WriteLine(key + " " + ipAddress);
                }
            }
        }
    }
}
