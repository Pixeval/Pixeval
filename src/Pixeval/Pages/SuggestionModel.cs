#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/SuggestionModel.cs
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

using Microsoft.UI.Xaml.Controls;
using Pixeval.Controls.MarkupExtensions;
using Pixeval.CoreApi.Model;
using Pixeval.Database;
using Pixeval.Util.UI;

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

    public FontIcon? FontIcon => SuggestionType switch
    {
        SuggestionType.IllustId or SuggestionType.NovelId or SuggestionType.UserId => FontIconSymbol.OpenInNewWindowE8A7.GetFontIcon(12),
        SuggestionType.Tag => FontIconSymbol.TagE8EC.GetFontIcon(12),
        SuggestionType.Settings => FontIconSymbol.SettingsE713.GetFontIcon(12),
        SuggestionType.History => FontIconSymbol.HistoryE81C.GetFontIcon(12),
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

    public static SuggestionModel FromTag(Tag tag)
    {
        return new SuggestionModel(tag.Name, tag.TranslatedName, SuggestionType.Tag);
    }

    public static SuggestionModel FromHistory(SearchHistoryEntry history)
    {
        return new SuggestionModel(history.Value, null, SuggestionType.History);
    }

    /// <summary>
    /// prevent the default behavior when user choose the suggestion
    /// </summary>
    public override string ToString() => "";
}

public enum SuggestionType
{
    Tag,
    Settings,
    History,
    UserId,
    IllustId,
    NovelId,
    IllustrationAutoCompleteTagHeader,
    IllustrationTrendingTagHeader,
    NovelTrendingTagHeader,
    SettingEntryHeader
}
