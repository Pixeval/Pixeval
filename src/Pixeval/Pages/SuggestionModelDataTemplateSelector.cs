#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/SuggestionModelDataTemplateSelector.cs
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
using Microsoft.UI.Xaml.Markup;
using Pixeval.Utilities;
using WinUI3Utilities;

namespace Pixeval.Pages;

public class SuggestionModelDataTemplateSelector : DataTemplateSelector
{
    public string? IllustrationHeader { get; set; }

    public string? NovelHeader { get; set; }

    public string? AutoCompletionHeader { get; set; }

    public string? SettingEntryHeader { get; set; }

    public DataTemplate? CommonSuggestion { get; set; }

    protected override DataTemplate? SelectTemplateCore(object item)
    {
        if (item is SuggestionModel model && model.SuggestionType switch
            {
                SuggestionType.IllustrationTrendingTagHeader => IllustrationHeader,
                SuggestionType.NovelTrendingTagHeader => NovelHeader,
                SuggestionType.SettingEntryHeader => SettingEntryHeader,
                SuggestionType.IllustrationAutoCompleteTagHeader => AutoCompletionHeader,
                _ => null
            } is { } header)
        {
            var xaml = $$"""
                         <DataTemplate xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                             <TextBlock x:Uid="{{header}}" Style="{StaticResource ContentBoldTextBlockStyle}" />
                         </DataTemplate>
                         """;
            return XamlReader.Load(xaml).To<DataTemplate>();
        }

        return CommonSuggestion;
    }
}
