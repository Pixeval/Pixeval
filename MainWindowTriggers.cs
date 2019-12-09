using System.Windows;

namespace Pixeval
{
    public partial class MainWindow
    {
        private void RetractDownloadListButton_OnClick(object sender, RoutedEventArgs e)
        {
            DownloadListTab.IsSelected = false;
        }
    }
}