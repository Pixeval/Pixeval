#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/SuggestionModel.cs
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
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.CoreApi.Model;
using Pixeval.Database;
using Pixeval.Util.UI;

namespace Pixeval.Pages;

public class SuggestionModel : IEquatable<SuggestionModel?>
{
    public FontIcon? Icon => SuggestionType switch
    {
        SuggestionType.Tag => FontIconSymbols.TagE8EC.GetFontIcon(12),
        SuggestionType.Settings => FontIconSymbols.SettingE713.GetFontIcon(12),
        SuggestionType.History => FontIconSymbols.HistoryE81C.GetFontIcon(12),
        _ => null
    };

    public Visibility TranslatedNameVisibility => TranslatedName == null ? Visibility.Collapsed : Visibility.Visible;

    public SuggestionType SuggestionType { get; init; }

    public string? Name { get; init; }

    public string? TranslatedName { get; init; }

    public bool Equals(SuggestionModel? other)
    {
        return other != null &&
               SuggestionType == other.SuggestionType &&
               Name == other.Name;
    }

    public static SuggestionModel FromTag(Tag tag)
    {
        return new SuggestionModel
        {
            Name = tag.Name,
            TranslatedName = tag.TranslatedName,
            SuggestionType = SuggestionType.Tag
        };
    }

    public static SuggestionModel FromHistory(SearchHistoryEntry history)
    {
        return new SuggestionModel
        {
            Name = history.Value,
            SuggestionType = SuggestionType.History
        };
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as SuggestionModel);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(SuggestionType, Name);
    }
}

public enum SuggestionType
{
    Tag,
    Settings,
    History
}