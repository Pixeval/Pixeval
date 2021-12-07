using System.Text;
using Microsoft.UI.Xaml;
using Pixeval.AppManagement;

namespace Pixeval.Pages.Misc;

public sealed partial class AboutPage
{
    public AboutPage()
    {
        InitializeComponent();
    }

    private async void AboutPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        var licenseText = Encoding.UTF8.GetString(await AppContext.GetAssetBytesAsync("GPLv3.md"));
        OpenSourceLicenseMarkdownTextBlock.Text = licenseText;
    }
}