// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Runtime;
using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;
using Pixeval.CoreApi.Global.Enum;
using WinUI3Utilities;

namespace Pixeval.Pages.Capability;

public sealed partial class SearchWorksPage : IScrollViewHost, IStructuralDisposalCompleter
{
    public SearchWorksPage() => InitializeComponent();

    private string _searchText = "";

    public override void OnPageActivated(NavigationEventArgs e, object? parameter)
    {
        (SimpleWorkTypeComboBox.SelectedEnum, _searchText) = e.Parameter.To<(SimpleWorkType, string)>();
        ChangeSource();
    }

    private void ComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e) => ChangeSource();

    private void ChangeSource()
    {
        var settings = App.AppViewModel.AppSettings;
        WorkContainer.WorkView.ResetEngine(
            SimpleWorkTypeComboBox.GetSelectedItem<SimpleWorkType>() is SimpleWorkType.IllustAndManga
                ? App.AppViewModel.MakoClient.SearchIllustrations(
                    _searchText,
                    settings.SearchIllustrationTagMatchOption,
                    settings.WorkSortOption,
                    settings.TargetFilter,
                    settings.UseSearchStartDate ? settings.SearchStartDate : null,
                    settings.UseSearchEndDate ? settings.SearchEndDate : null)
                : App.AppViewModel.MakoClient.SearchNovels(
                    _searchText,
                    settings.SearchNovelTagMatchOption,
                    settings.WorkSortOption,
                    settings.TargetFilter,
                    settings.UseSearchStartDate ? settings.SearchStartDate : null,
                    settings.UseSearchEndDate ? settings.SearchEndDate : null));
    }

    public ScrollView ScrollView => WorkContainer.ScrollView;

    public void CompleteDisposal()
    {
        Content = null;
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.Collect();
    }
}
