// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Mako;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Models.Database;
using Pixeval.ViewModels.Search;

namespace Pixeval.ViewModels;

public partial class SearchPageViewModel : ViewModelBase
{
    public static SearchPageViewModel Instance { get; } = new();

    private static MakoClient MakoClient => App.AppViewModel.MakoClient;

    private SearchPageViewModel()
    {
        IllustrationForm = new IllustrationSearchFormViewModel();
        NovelForm = new NovelSearchFormViewModel();
        _ = LoadResourcesAsync();
    }

    private async Task LoadResourcesAsync()
    {
        await Task.WhenAll(RefreshIllustrationTagsAsync(), RefreshNovelTagsAsync(), RefreshSearchOptionsAsync());
    }

    private async Task RefreshIllustrationTagsAsync()
    {
        IllustrationTrendingTags = await MakoClient.GetIllustrationTrendingTagsAsync();
    }

    private async Task RefreshNovelTagsAsync()
    {
        NovelTrendingTags = await MakoClient.GetNovelTrendingTagsAsync();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TrendingTags))]
    public partial SimpleWorkType SelectedTrendingTagsType { get; set; } = App.AppViewModel.AppSettings.DefaultSimpleWorkType;

    [ObservableProperty]
    public partial string SearchText { get; set; } = "";

    [ObservableProperty]
    public partial SimpleWorkType SelectedAdvancedOptionsType { get; set; } = App.AppViewModel.AppSettings.DefaultSimpleWorkType;

    public IReadOnlyList<TrendingTag> TrendingTags => SelectedTrendingTagsType is SimpleWorkType.Novel ? NovelTrendingTags : IllustrationTrendingTags;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TrendingTags))]
    public partial IReadOnlyList<TrendingTag> IllustrationTrendingTags { get; private set; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TrendingTags))]
    public partial IReadOnlyList<TrendingTag> NovelTrendingTags { get; private set; } = [];

    public IllustrationSearchFormViewModel IllustrationForm { get; }

    public NovelSearchFormViewModel NovelForm { get; }

    public ObservableCollection<SearchHistoryEntry> SearchHistories => App.AppViewModel.HistoryPersistHelper.SearchHistoryEntries;

    private async Task RefreshSearchOptionsAsync()
    {
        var options = await MakoClient.GetSearchOptionsAsync();
        IllustrationForm.ToolItems =
        [
            .. IllustrationForm.ToolItems,
            .. options.IllustrationOptions.Tools.Options
        ];
        NovelForm.LanguageItems =
        [
            .. NovelForm.LanguageItems,
            .. options.NovelOptions.Languages.Options
        ];
        NovelForm.GenreItems =
        [
            .. NovelForm.GenreItems,
            .. options.NovelOptions.Genres.Options
        ];
    }
}
