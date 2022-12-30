#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/SuggestionModelDataTemplateSelector.cs
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

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Pixeval.Pages;

public class SuggestionModelDataTemplateSelector : DataTemplateSelector
{
    public DataTemplate? IllustrationHeader { get; set; }

    public DataTemplate? NovelHeader { get; set; }

    public DataTemplate? AutoCompletionHeader { get; set; }

    public DataTemplate? SettingEntryHeader { get; set; }

    public DataTemplate? CommonSuggestion { get; set; }

    protected override DataTemplate SelectTemplateCore(object item)
    {
        return item switch
        {
            SuggestionModel { SuggestionType: SuggestionType.IllustrationTrendingTagHeader } => IllustrationHeader!,
            SuggestionModel { SuggestionType: SuggestionType.NovelTrendingTagHeader } => NovelHeader!,
            SuggestionModel { SuggestionType: SuggestionType.SettingEntryHeader } => SettingEntryHeader!,
            SuggestionModel { SuggestionType: SuggestionType.IllustrationAutoCompleteTagHeader } => AutoCompletionHeader!,
            _ => CommonSuggestion!
        };
    }
}