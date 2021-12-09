using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Threading.Tasks;
using Windows.Foundation;
using CommunityToolkit.WinUI.UI;
using Microsoft.Toolkit.Mvvm.Messaging;
using Pixeval.Messages;
using Pixeval.Pages.Misc;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pixeval.Dialogs
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ReverseSearchApiKeyNotPresentDialog
    {
        public ContentDialog? Owner;

        public ReverseSearchApiKeyNotPresentDialog()
        {
            InitializeComponent();
        }

        private async void SetApiKeyHyperlinkButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            Owner?.Hide();
            var mainPageRootNavigationView = (NavigationView) App.AppViewModel.AppWindowRootFrame.FindDescendant("MainPageRootNavigationView")!;
            mainPageRootNavigationView.SelectedItem = mainPageRootNavigationView.FindDescendant("SettingsTab")!;
            WeakReferenceMessenger.Default.Send(new OpenSearchSettingMessage());
            await Task.Delay(500);
            var settingsPage = App.AppViewModel.AppWindowRootFrame.FindDescendant("MainPageRootFrame")!.FindDescendant<SettingsPage>()!;
            var position = settingsPage.SearchSettingsGroup
                .TransformToVisual((UIElement) settingsPage.SettingsPageScrollViewer.Content)
                .TransformPoint(new Point(0, 0));
            settingsPage.SettingsPageScrollViewer.ChangeView(null, position.Y, null, false);
        }
    }
}
