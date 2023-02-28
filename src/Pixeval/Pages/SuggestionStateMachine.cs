#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/SuggestionStateMachine.cs
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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PininSharp;
using PininSharp.Searchers;
using Pixeval.Attributes;
using Pixeval.CoreApi.Model;
using Pixeval.Database.Managers;
using Pixeval.Utilities;

namespace Pixeval.Pages;

public class SuggestionStateMachine
{
    private readonly Lazy<Task<IEnumerable<SuggestionModel>>> _illustrationTrendingTagCache = new(
        () => App.AppViewModel.MakoClient.GetTrendingTagsAsync(App.AppViewModel.AppSetting.TargetFilter)
            .WhereAsync(t => t.Tag is not null && t.Translation is not null)
            .SelectAsync(t => new Tag(t.Tag!, t.Translation))
            .SelectAsync(SuggestionModel.FromTag), LazyThreadSafetyMode.ExecutionAndPublication);

    private readonly Lazy<Task<IEnumerable<SuggestionModel>>> _novelTrendingTagCache = new(
        () => App.AppViewModel.MakoClient.GetTrendingTagsForNovelAsync(App.AppViewModel.AppSetting.TargetFilter)
            .WhereAsync(t => t.Tag is not null && t.Translation is not null)
            .SelectAsync(t => new Tag(t.Tag!, t.Translation))
            .SelectAsync(SuggestionModel.FromTag), LazyThreadSafetyMode.ExecutionAndPublication);

    private static readonly TreeSearcher<SettingEntry> SettingEntriesTreeSearcher = new(SearcherLogic.Contain, PinIn.CreateDefault());

    public ObservableCollection<SuggestionModel> Suggestions { get; }

    public SuggestionStateMachine()
    {
        Suggestions = new ObservableCollection<SuggestionModel>();
    }

    static SuggestionStateMachine()
    {
        foreach (var settingsEntry in SettingEntry.LazyValues.Value)
        {
            SettingEntriesTreeSearcher.Put(settingsEntry.GetLocalizedResourceContent()!, settingsEntry);
        }
    }

    public Task UpdateAsync(string keyword)
    {
        if (keyword.IsNotNullOrEmpty())
        {
            return FillSuggestions(keyword);
        }
        Suggestions.Clear();
        return FillHistoryAndRecommendTags();
    }

    private async Task FillSuggestions(string keyword)
    {
        var tagSuggestions = (await App.AppViewModel.MakoClient.GetAutoCompletionForKeyword(keyword)).Select(SuggestionModel.FromTag).ToList();
        var settingSuggestions = MatchSettings(keyword);
        var suggestions = new List<SuggestionModel>();
        if (settingSuggestions.IsNotNullOrEmpty())
        {
            suggestions.Add(SuggestionModel.SettingEntryHeader);
            suggestions.AddRange(settingSuggestions.Select(settingSuggestion => new SuggestionModel(settingSuggestion.GetLocalizedResourceContent()!, settingSuggestion.Category.GetLocalizedResourceContent(), SuggestionType.Settings)));
        }

        if (settingSuggestions.IsNotNullOrEmpty() && tagSuggestions.IsNotNullOrEmpty())
        {
            suggestions.Add(SuggestionModel.IllustrationAutoCompleteTagHeader);
            suggestions.AddRange(tagSuggestions);
        }
        Suggestions.ReplaceByUpdate(suggestions);
    }

    private async Task FillHistoryAndRecommendTags()
    {
        var newItems = new List<SuggestionModel>();
        using var scope = App.AppViewModel.AppServicesScope;
        var manager = scope.ServiceProvider.GetRequiredService<SearchHistoryPersistentManager>();
        var histories = manager.Select(count: App.AppViewModel.AppSetting.MaximumSuggestionBoxSearchHistory).OrderByDescending(e => e.Time).SelectNotNull(SuggestionModel.FromHistory);
        newItems.AddRange(histories);
        newItems.Add(SuggestionModel.IllustrationTrendingTagHeader);
        newItems.AddRange(await _illustrationTrendingTagCache.Value);
        newItems.Add(SuggestionModel.NovelTrendingTagHeader);
        newItems.AddRange(await _novelTrendingTagCache.Value);
        Suggestions.AddRange(newItems);
    }

    private static IReadOnlySet<SettingEntry> MatchSettings(string keyword)
    {
        var pinInResult = SettingEntriesTreeSearcher.Search(keyword).ToHashSet();
        var nonPinInResult = SettingEntry.LazyValues.Value.Where(it => it.GetLocalizedResourceContent()!.Contains(keyword));
        pinInResult.AddRange(nonPinInResult);
        return pinInResult;
    }
}