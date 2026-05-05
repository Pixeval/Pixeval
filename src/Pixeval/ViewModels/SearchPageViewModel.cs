// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Mako;
using Mako.Model;
using Pixeval.Models.Database;

namespace Pixeval.ViewModels;

public partial class SearchPageViewModel : ViewModelBase
{
    public static SearchPageViewModel Instance { get; } = new();

    private static MakoClient MakoClient => App.AppViewModel.MakoClient;

    private SearchPageViewModel()
    {
        _ = LoadResourcesAsync();
    }

    private async Task LoadResourcesAsync()
    {
        await Task.WhenAll(RefreshIllustrationTagsAsync(), RefreshNovelTagsAsync());
    }

    [MemberNotNull(nameof(IllustrationTrendingTags))]
    public async Task RefreshIllustrationTagsAsync()
    {
        IllustrationTrendingTags = await MakoClient.GetIllustrationTrendingTagsAsync();
    }

    [MemberNotNull(nameof(NovelTrendingTags))]
    public async Task RefreshNovelTagsAsync()
    {
        NovelTrendingTags = await MakoClient.GetNovelTrendingTagsAsync();
    }

    [ObservableProperty]
    public partial IReadOnlyList<TrendingTag> IllustrationTrendingTags { get; private set; }

    [ObservableProperty]
    public partial IReadOnlyList<TrendingTag> NovelTrendingTags { get; private set; }

    public ObservableCollection<SearchHistoryEntry> SearchHistories => App.AppViewModel.HistoryPersistHelper.SearchHistoryEntries;
}
