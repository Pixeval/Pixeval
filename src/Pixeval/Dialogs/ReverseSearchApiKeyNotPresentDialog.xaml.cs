#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/ReverseSearchApiKeyNotPresentDialog.xaml.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System.Threading.Tasks;
using Windows.Foundation;
using CommunityToolkit.WinUI.UI;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Pixeval.Messages;
using Pixeval.Pages.Misc;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pixeval.Dialogs;

/// <summary>
///     An empty page that can be used on its own or navigated to within a Frame.
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