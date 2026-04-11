using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Mako.Engine;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Controls;
using Frame = FluentAvalonia.UI.Controls.Frame;

namespace Pixeval.Views.Capability;

public partial class SearchWorksPage : UserControl
{
    private string? _searchText;

    public SearchWorksPage()
    {
        InitializeComponent();
        AddHandler(Frame.NavigatedToEvent, (sender, e) =>
        {
            if (e.Parameter is not (SimpleWorkType type, string s))
                return;
            _searchText = s;
            SimpleWorkTypeComboBox.SelectedIndex = (int) type;
            ChangeSource();
        });
    }

    private void WorkTypeComboBox_OnSelectionChanged(SymbolComboBox sender, EventArgs e)
    {
        ChangeSource();
    }

    private void WorkContainer_OnRefreshRequested(object? sender, RoutedEventArgs e)
    {
        ChangeSource();
    }

    private void ChangeSource()
    {
        IFetchEngine<IWorkEntry> engine;
        if (_searchText is null)
            engine = App.AppViewModel.MakoClient.Computed(AsyncEnumerable.Empty<IWorkEntry>());
        else
        {
            var settings = App.AppViewModel.AppSettings;
            engine = App.AppViewModel.MakoClient.SearchWorks(
                _searchText,
                SimpleWorkTypeComboBox.GetSelectedValue<SimpleWorkType>(),
                settings.SearchIllustrationTagMatchOption,
                settings.SearchNovelTagMatchOption,
                settings.WorkSortOption,
                settings.UseSearchStartDate ? settings.SearchStartDate : null,
                settings.UseSearchEndDate ? settings.SearchEndDate : null,
                null,
                settings.TargetFilter);
        }

        WorkContainer.ResetEngine(engine);
    }
}
