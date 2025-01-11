// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.Database.Managers;
using Pixeval.Utilities;

namespace Pixeval.Pages.Misc;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class BrowsingHistoryPage : IScrollViewHost
{
    public BrowsingHistoryPage() => InitializeComponent();

    public override void OnPageActivated(NavigationEventArgs e, object? parameter) => ChangeSource();

    private void ComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e) => ChangeSource();

    private void ChangeSource()
    {
        var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<BrowseHistoryPersistentManager>();
        var type = SimpleWorkTypeComboBox.GetSelectedItem<SimpleWorkType>();
        var source = manager.Enumerate()
            .SelectNotNull(t => t.TryGetEntry(type))
            .ToAsyncEnumerable();

        WorkContainer.WorkView.ResetEngine(type switch
        {
            SimpleWorkType.IllustAndManga => App.AppViewModel.MakoClient.Computed(source.Cast<Illustration>()),
            _ => App.AppViewModel.MakoClient.Computed(source.Cast<Novel>()),
        });
    }

    public ScrollView ScrollView => WorkContainer.ScrollView;
}
