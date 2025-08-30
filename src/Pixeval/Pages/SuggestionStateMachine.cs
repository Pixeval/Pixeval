// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PininSharp;
using PininSharp.Searchers;
using Pixeval.Attributes;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Database.Managers;
using Pixeval.Utilities;

namespace Pixeval.Pages;

public class SuggestionStateMachine
{
    private static readonly TreeSearcher<SettingsEntryAttribute> _SettingEntriesTreeSearcher =
        new(SearcherLogic.Contain, PinIn.CreateDefault());

    private readonly Task<IEnumerable<SuggestionModel>> _illustrationTrendingTagCache =
        Functions.Block(async () => (await App.AppViewModel.MakoClient.GetTrendingTagsAsync(App.AppViewModel.AppSettings.TargetFilter))
            .Select(t => new Tag { Name = t.Tag, TranslatedName = t.TranslatedName })
            .Select(SuggestionModel.FromIllustrationTag));

    private readonly Task<IEnumerable<SuggestionModel>> _novelTrendingTagCache =
        Functions.Block(async () => (await App.AppViewModel.MakoClient.GetTrendingTagsForNovelAsync(App.AppViewModel.AppSettings.TargetFilter))
            .Select(t => new Tag { Name = t.Tag, TranslatedName = t.TranslatedName })
            .Select(SuggestionModel.FromNovelTag));

    static SuggestionStateMachine()
    {
        foreach (var settingsEntry in SettingsEntryAttribute.LazyValues.Value)
        {
            _SettingEntriesTreeSearcher.Put(settingsEntry.LocalizedResourceHeader, settingsEntry);
        }
    }

    public ObservableCollection<SuggestionModel> Suggestions { get; } = [];

    public Task UpdateAsync(string keyword)
    {
        if (!string.IsNullOrEmpty(keyword))
            return FillSuggestions(keyword);
        Suggestions.Clear();
        return FillHistoryAndRecommendTags();
    }

    private async Task FillSuggestions(string keyword)
    {
        var tagSuggestions = (await App.AppViewModel.MakoClient.GetAutoCompletionForKeyword(keyword)).Select(SuggestionModel.FromTag).ToArray();
        var settingSuggestions = MatchSettings(keyword).Select(SuggestionModel.FromSettings).ToArray();
        var suggestions = new List<SuggestionModel>();

        if (long.TryParse(keyword, out _))
        {
            suggestions.AddRange(SuggestionModel.FromId());
        }

        suggestions.Add(SuggestionModel.FromUserSearch());

        if (settingSuggestions.Length is not 0)
        {
            suggestions.Add(SuggestionModel.SettingEntryHeader);
            suggestions.AddRange(settingSuggestions);
        }

        if (tagSuggestions.Length is not 0)
        {
            suggestions.Add(SuggestionModel.IllustrationAutoCompleteTagHeader);
            suggestions.AddRange(tagSuggestions);
        }
        Suggestions.ReplaceByUpdate(suggestions);
    }

    private async Task FillHistoryAndRecommendTags()
    {
        var newItems = new List<SuggestionModel>();
        var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<SearchHistoryPersistentManager>();
        var histories = manager.TakeLast(count: App.AppViewModel.AppSettings.MaximumSuggestionBoxSearchHistory).SelectNotNull(SuggestionModel.FromHistory);
        newItems.AddRange(histories);
        var prior = App.AppViewModel.AppSettings.SimpleWorkType is SimpleWorkType.IllustAndManga;
        if (prior)
        {
            newItems.Add(SuggestionModel.IllustrationTrendingTagHeader);
            newItems.AddRange(await _illustrationTrendingTagCache);
        }
        newItems.Add(SuggestionModel.NovelTrendingTagHeader);
        newItems.AddRange(await _novelTrendingTagCache);
        if (!prior)
        {
            newItems.Add(SuggestionModel.IllustrationTrendingTagHeader);
            newItems.AddRange(await _illustrationTrendingTagCache);
        }
        Suggestions.AddRange(newItems);
    }

    private static HashSet<SettingsEntryAttribute> MatchSettings(string keyword)
    {
        var pinInResult = _SettingEntriesTreeSearcher.Search(keyword).ToHashSet();
        var nonPinInResult = SettingsEntryAttribute.LazyValues.Value.Where(it => it.LocalizedResourceHeader.Contains(keyword));
        pinInResult.AddRange(nonPinInResult);
        return pinInResult;
    }
}
