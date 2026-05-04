using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Mako.Engine;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Controls;

namespace Pixeval.Views.Capability;

public partial class SearchWorksPage : ContentPage
{
    private readonly string? _searchText;

    public SearchWorksPage() : this(default, null)
    {
    }

    public SearchWorksPage(SimpleWorkType type, string? s)
    {
        InitializeComponent();
        _searchText = s;
        SimpleWorkTypeComboBox.SelectedIndex = (int) type;
        ChangeSource();
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
            engine = SimpleWorkTypeComboBox.GetSelectedValue<SimpleWorkType>() is SimpleWorkType.IllustrationAndManga
                    ? App.AppViewModel.MakoClient.IllustrationSearch(
                        _searchText,
                        settings.SearchIllustrationTagMatchOption,
                        default,
                        settings.UseSearchStartDate ? settings.SearchStartDate : null,
                        settings.UseSearchEndDate ? settings.SearchEndDate : null)
                    : App.AppViewModel.MakoClient.NovelSearch(
                        _searchText,
                        settings.SearchNovelTagMatchOption,
                        default,
                        settings.UseSearchStartDate ? settings.SearchStartDate : null,
                        settings.UseSearchEndDate ? settings.SearchEndDate : null);
        }

        WorkContainer.ResetEngine(engine);
    }
}
