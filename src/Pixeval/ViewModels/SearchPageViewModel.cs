// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Mako.Model;

namespace Pixeval.ViewModels;

public class SearchPageViewModel : ViewModelBase
{
    public static SearchPageViewModel Instance { get; } = new();

    private SearchPageViewModel()
    {
        _ = LoadResourcesAsync();
    }

    private async Task LoadResourcesAsync()
    {
        var makoClient = App.AppViewModel.MakoClient;
        var trendingTags = await makoClient.GetTrendingTagsAsync();
        TrendingTags.AddRange(trendingTags);
    }

    public List<TrendingTag> TrendingTags { get; } = [];
}
