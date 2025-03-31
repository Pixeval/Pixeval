// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Pixeval.Controls.Windowing;
using WinUI3Utilities;

namespace Pixeval.Pages;

public partial class SuggestionModelDataTemplateSelector : DataTemplateSelector
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
                             <TextBlock x:Uid="{{header}}" Style="{StaticResource BodyStrongTextBlockStyle}" />
                         </DataTemplate>
                         """;
            return XamlReader.Load(xaml).To<DataTemplate>();
        }

        return CommonSuggestion;
    }
}

public partial class NavigationViewItemDataTemplateSelector : DataTemplateSelector
{
    public DataTemplate? NavigationViewTagDataTemplate { get; set; }

    public DataTemplate? NavigationViewSeparatorDataTemplate { get; set; }

    protected override DataTemplate? SelectTemplateCore(object item)
    {
        return item switch
        {
            NavigationViewTag _ => NavigationViewTagDataTemplate,
            NavigationViewSeparator _ => NavigationViewSeparatorDataTemplate,
            _ => null
        };
    }
}
