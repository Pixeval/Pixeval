// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;
using Mako.Global.Enum;
using Mako.Model;
using Microsoft.UI.Xaml;
using Misaki;
using Pixeval.Database.Managers;
using Pixeval.Utilities;

namespace Pixeval.Pages.Misc;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class BrowsingHistoryPage : IScrollViewHost
{
    public BrowsingHistoryPage()
    {
        InitializeComponent();
        SimpleWorkTypeComboBox.SelectedEnum = App.AppViewModel.AppSettings.SimpleWorkType;
    }

    public override void OnPageActivated(NavigationEventArgs e, object? parameter) => ChangeSource();

    private void ComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e) => ChangeSource();

    private void ChangeSource()
    {
        var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<BrowseHistoryPersistentManager>();
        var type = SimpleWorkTypeComboBox.GetSelectedItem<SimpleWorkType>();
        var isIllustration = type is SimpleWorkType.IllustAndManga;
        var source = manager
            .Reverse()
            .SelectNotNull(t => t.Entry)
            .Where(t => t.ImageType is ImageType.Other ^ isIllustration)
            .ToAsyncEnumerable();

        WorkContainer.WorkView.ResetEngine(type switch
        {
            SimpleWorkType.IllustAndManga => App.AppViewModel.MakoClient.Computed(source),
            _ => App.AppViewModel.MakoClient.Computed(source.OfType<Novel>()),
        });
    }

    public ScrollView ScrollView => WorkContainer.ScrollView;

    private void WorkContainer_OnRefreshRequested(object sender, RoutedEventArgs e) => ChangeSource();
}
