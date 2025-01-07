// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Microsoft.UI.Xaml.Controls;
using Pixeval.CoreApi.Model;
using Pixeval.Database;
using Pixeval.Util.UI;
using Symbol = FluentIcons.Common.Symbol;

namespace Pixeval.Pages;

public record SuggestionModel(string? Name, string? TranslatedName, SuggestionType SuggestionType)
{
    public static readonly SuggestionModel IllustrationTrendingTagHeader =
        new(null, null, SuggestionType.IllustrationTrendingTagHeader);

    public static readonly SuggestionModel IllustrationAutoCompleteTagHeader =
        new(null, null, SuggestionType.IllustrationAutoCompleteTagHeader);

    public static readonly SuggestionModel SettingEntryHeader =
        new(null, null, SuggestionType.SettingEntryHeader);

    public static readonly SuggestionModel NovelTrendingTagHeader =
        new(null, null, SuggestionType.NovelTrendingTagHeader);

    public Symbol SettingsSymbol { get; init; }

    public FontIcon? FontIcon => SuggestionType switch
    {
        SuggestionType.IllustId or SuggestionType.NovelId or SuggestionType.UserId => Symbol.Open.GetSymbolIcon(true),
        SuggestionType.Tag or SuggestionType.IllustrationTag or SuggestionType.NovelTag => Symbol.Tag.GetSymbolIcon(true),
        SuggestionType.UserSearch => Symbol.Person.GetSymbolIcon(true),
        SuggestionType.Settings => SettingsSymbol.GetSymbolIcon(true),
        SuggestionType.History => Symbol.History.GetSymbolIcon(true),
        _ => null
    };

    public static SuggestionModel[] FromId()
    {
        return
        [
            new SuggestionModel(MiscResources.OpenIllustId, null, SuggestionType.IllustId),
            new SuggestionModel(MiscResources.OpenNovelId, null, SuggestionType.NovelId),
            new SuggestionModel(MiscResources.OpenUserId, null, SuggestionType.UserId)
        ];
    }

    public static SuggestionModel FromUserSearch()
    {
        return new SuggestionModel(MiscResources.SearchUser, null, SuggestionType.UserSearch);
    }

    public static SuggestionModel FromTag(Tag tag)
    {
        return new SuggestionModel(tag.Name, tag.TranslatedName, SuggestionType.Tag);
    }

    public static SuggestionModel FromIllustrationTag(Tag tag)
    {
        return new SuggestionModel(tag.Name, tag.TranslatedName, SuggestionType.IllustrationTag);
    }

    public static SuggestionModel FromNovelTag(Tag tag)
    {
        return new SuggestionModel(tag.Name, tag.TranslatedName, SuggestionType.NovelTag);
    }

    public static SuggestionModel FromHistory(SearchHistoryEntry history)
    {
        return new SuggestionModel(history.Value, history.TranslatedName, SuggestionType.History);
    }

    /// <summary>
    /// prevent the default behavior when user choose the suggestion
    /// </summary>
    public override string ToString() => "";
}

public enum SuggestionType
{
    Tag,
    IllustrationTag,
    NovelTag,
    Settings,
    History,
    UserId,
    IllustId,
    NovelId,
    UserSearch,
    IllustrationAutoCompleteTagHeader,
    IllustrationTrendingTagHeader,
    NovelTrendingTagHeader,
    SettingEntryHeader
}
