using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.I18N;
using Pixeval.Models.Database;
using Pixeval.Utilities;
using Pixeval.ViewModels;
using Pixeval.Views.Capability;

namespace Pixeval.Views.Search;

public partial class SearchPage : ContentPage
{
    public SearchPage()
    {
        InitializeComponent();
    }

    private void DeleteButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control { DataContext: SearchHistoryEntry entry })
            return;
        App.AppViewModel.HistoryPersistHelper.SearchHistoryEntries.Remove(entry);
    }

    private void TrendingTagButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control { DataContext: TrendingTag tag })
            return;
        if (TopLevel.GetTopLevel(this)?.ViewContainer is not { } viewContainer)
            return;
        if (DataContext is not SearchPageViewModel viewModel)
            return;

        viewContainer.NavigateTo(new SearchWorksPage(tag.Tag, viewModel.SelectedTrendingTagsType));
    }

    private void SearchButton_OnClick(object? sender, RoutedEventArgs e)
    {
        ExecuteSearch(SearchAutoCompleteBox.Text?.Trim(), AdvancedExpander.IsExpanded);
    }

    private void SearchAutoCompleteBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is not Key.Enter)
            return;

        ExecuteSearch(SearchAutoCompleteBox.Text?.Trim(), AdvancedExpander.IsExpanded);
        e.Handled = true;
    }

    private void SearchHistoryButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control { DataContext: SearchHistoryEntry entry })
            return;

        if (DataContext is SearchPageViewModel viewModel)
            viewModel.SearchText = entry.Value;

        ExecuteSearch(entry.Value, false);
    }

    private void ExecuteSearch(string? searchText, bool advanced)
    {
        if (TopLevel.GetTopLevel(this)?.ViewContainer is not { } viewContainer)
            return;
        if (DataContext is not SearchPageViewModel viewModel)
            return;

        if (string.IsNullOrWhiteSpace(searchText))
        {
            viewContainer.ShowWarning(
                I18NManager.GetResource(MainPageResources.SearchKeywordCannotBeBlankTitle),
                I18NManager.GetResource(MainPageResources.SearchKeywordCannotBeBlankContent));
            return;
        }

        try
        {
            if (!advanced)
            {
                viewContainer.NavigateTo(new SearchWorksPage(searchText, viewModel.SelectedAdvancedOptionsType));
                return;
            }

            if (viewModel.SelectedAdvancedOptionsType is SimpleWorkType.Novel)
            {
                if (!viewModel.NovelForm.TryValidate(out var title, out var content))
                {
                    viewContainer.ShowWarning(title, content);
                    return;
                }

                var arguments = viewModel.NovelForm.BuildArguments(searchText);
                App.AppViewModel.HistoryPersistHelper.AddSearchHistory(searchText);
                viewContainer.NavigateTo(new SearchWorksPage(arguments));
            }
            else
            {
                if (!viewModel.IllustrationForm.TryValidate(out var title, out var content))
                {
                    viewContainer.ShowWarning(title, content);
                    return;
                }

                var arguments = viewModel.IllustrationForm.BuildArguments(searchText);
                App.AppViewModel.HistoryPersistHelper.AddSearchHistory(searchText);
                viewContainer.NavigateTo(new SearchWorksPage(arguments));
            }
        }
        catch (Exception ex)
        {
            viewContainer.ShowError(I18NManager.GetResource(SearchResources.ValidationSearchFailedTitle), ex.Message);
        }
    }
}
