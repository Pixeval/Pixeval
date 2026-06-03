// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Mako.Global.Enum;
using Mako.Model;
using Misaki;
using Pixeval.I18N;
using Pixeval.Models.Database;
using Pixeval.Utilities;
using Pixeval.ViewModels;
using Pixeval.Views.Capability;
using Pixeval.Views.Viewers;

namespace Pixeval.Views.Search;

public enum SearchCompletionKind
{
    OpenIllustration,
    OpenNovel,
    OpenUser,
    SearchUser,
    Tag
}

public sealed record SearchCompletionItem(SearchCompletionKind Kind, string Text, string? Description = null);

public partial class SearchPage : ContentPage
{
    private const string SearchTextBoxPart = "PART_TextBox";
    private const string SearchSelectingItemsControlPart = "PART_SelectingItemsControl";

    private TextBox? _searchTextBox;
    private SelectingItemsControl? _searchSuggestionItemsControl;
    private SearchCompletionItem? _selectedSearchCompletion;
    private CancellationTokenSource? _searchCompletionCancellationTokenSource;
    private int _searchCompletionUpdateVersion;

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

        viewContainer.NavigateTo(new WorkSearchPage(tag.Tag, viewModel.SelectedTrendingTagsType));
    }

    private void SearchButton_OnClick(object? sender, RoutedEventArgs e)
    {
        ExecuteSearch(SearchAutoCompleteBox.Text?.Trim(), AdvancedExpander.IsExpanded);
    }

    private void SearchAutoCompleteBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is not Key.Enter)
            return;

        if (SearchAutoCompleteBox.IsDropDownOpen && CommitSearchCompletion(GetSelectedSearchCompletion()))
        {
            e.Handled = true;
            return;
        }

        ExecuteSearch(SearchAutoCompleteBox.Text?.Trim(), AdvancedExpander.IsExpanded);
        e.Handled = true;
    }

    private void SearchAutoCompleteBox_OnTemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
        DetachSearchSuggestionItemsControl();

        _searchTextBox = e.NameScope.Find<TextBox>(SearchTextBoxPart)
                         ?? SearchAutoCompleteBox.GetVisualDescendants().OfType<TextBox>().FirstOrDefault();
        _searchSuggestionItemsControl = e.NameScope.Find<SelectingItemsControl>(SearchSelectingItemsControlPart);
        if (_searchSuggestionItemsControl is not null)
        {
            _searchSuggestionItemsControl.SelectionChanged += SearchSuggestionItemsControl_OnSelectionChanged;
            _searchSuggestionItemsControl.PointerReleased += SearchSuggestionItemsControl_OnPointerReleased;
        }
    }

    private void DetachSearchSuggestionItemsControl()
    {
        if (_searchSuggestionItemsControl is not null)
        {
            _searchSuggestionItemsControl.SelectionChanged -= SearchSuggestionItemsControl_OnSelectionChanged;
            _searchSuggestionItemsControl.PointerReleased -= SearchSuggestionItemsControl_OnPointerReleased;
        }
        _searchSuggestionItemsControl = null;
    }

    private void SearchSuggestionItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.OfType<SearchCompletionItem>().LastOrDefault() is { } completion)
            _selectedSearchCompletion = completion;
    }

    private void SearchSuggestionItemsControl_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton is not MouseButton.Left)
            return;

        var selectedItem = GetSelectedSearchCompletion();
        Dispatcher.UIThread.Post(() =>
        {
            _ = CommitSearchCompletion(selectedItem ?? GetSelectedSearchCompletion());
        });
    }

    private void SearchAutoCompleteBox_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        QueueUpdateSearchCompletions();
    }

    private void SearchAutoCompleteBox_OnGotFocus(object? sender, RoutedEventArgs e)
    {
        QueueUpdateSearchCompletions();
    }

    private void QueueUpdateSearchCompletions()
    {
        var version = ++_searchCompletionUpdateVersion;
        Dispatcher.UIThread.Post(() =>
        {
            if (version == _searchCompletionUpdateVersion)
                UpdateSearchCompletions(SearchAutoCompleteBox.Text);
        });
    }

    private void UpdateSearchCompletions(string? text)
    {
        var source = text ?? "";
        var normalized = source.Trim();
        if (normalized.Length is 0)
        {
            ClearSearchCompletionItems();
            return;
        }

        _searchCompletionCancellationTokenSource?.Cancel();
        _searchCompletionCancellationTokenSource?.Dispose();
        _searchCompletionCancellationTokenSource = new();

        var version = _searchCompletionUpdateVersion;
        var immediateSuggestions = CreateImmediateSearchCompletions(normalized);
        SetSearchCompletionItems(immediateSuggestions);
        _ = LoadTagCompletionsAsync(source, immediateSuggestions, version, _searchCompletionCancellationTokenSource.Token);
    }

    private IReadOnlyList<SearchCompletionItem> CreateImmediateSearchCompletions(string normalized)
    {
        var suggestions = new List<SearchCompletionItem>();
        if (normalized.All(char.IsDigit))
        {
            suggestions.Add(new(
                SearchCompletionKind.OpenIllustration,
                I18NManager.GetResource(SearchResources.OpenIdIllustration),
                normalized));
            suggestions.Add(new(
                SearchCompletionKind.OpenNovel,
                I18NManager.GetResource(SearchResources.OpenIdNovel),
                normalized));
            suggestions.Add(new(
                SearchCompletionKind.OpenUser,
                I18NManager.GetResource(SearchResources.OpenIdUser),
                normalized));
        }

        suggestions.Add(new(
            SearchCompletionKind.SearchUser,
            I18NManager.GetResource(SearchResources.SearchUser),
            normalized));

        return suggestions;
    }

    private async Task LoadTagCompletionsAsync(
        string source,
        IReadOnlyList<SearchCompletionItem> immediateSuggestions,
        int version,
        CancellationToken token)
    {
        try
        {
            var keyword = GetLastKeyword(source);
            if (keyword.Length is 0)
                return;

            var tags = await App.AppViewModel.MakoClient.GetAutoCompletionForKeyword(keyword);
            token.ThrowIfCancellationRequested();
            if (version != _searchCompletionUpdateVersion || !string.Equals(source, SearchAutoCompleteBox.Text ?? "", StringComparison.Ordinal))
                return;

            var suggestions = immediateSuggestions
                .Concat(tags.Select(tag => new SearchCompletionItem(
                    SearchCompletionKind.Tag,
                    tag.Name,
                    tag.TranslatedName)))
                .ToArray();
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (version == _searchCompletionUpdateVersion && string.Equals(source, SearchAutoCompleteBox.Text ?? "", StringComparison.Ordinal))
                    SetSearchCompletionItems(suggestions);
            });
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void SetSearchCompletionItems(IReadOnlyList<SearchCompletionItem> suggestions)
    {
        if (suggestions.Count is 0)
            ClearSearchCompletionItems();
        else
            SearchAutoCompleteBox.ItemsSource = suggestions;
        SearchAutoCompleteBox.IsDropDownOpen = suggestions.Count > 0 && HasSearchAutoCompleteBoxFocus();
    }

    private void ClearSearchCompletionItems()
    {
        _searchCompletionCancellationTokenSource?.Cancel();
        _selectedSearchCompletion = null;
        SearchAutoCompleteBox.SelectedItem = null;
        SearchAutoCompleteBox.ItemsSource = null;
        SearchAutoCompleteBox.IsDropDownOpen = false;
    }

    private string SearchAutoCompleteBox_OnSelectItem(string? text, object item)
        => item is SearchCompletionItem { Kind: SearchCompletionKind.Tag } completion
            ? ApplyKeywordCompletion(text ?? "", completion.Text)
            : text ?? "";

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
                viewContainer.NavigateTo(new WorkSearchPage(searchText, viewModel.SelectedAdvancedOptionsType));
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
                viewContainer.NavigateTo(new WorkSearchPage(arguments));
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
                viewContainer.NavigateTo(new WorkSearchPage(arguments));
            }
        }
        catch (Exception ex)
        {
            viewContainer.ShowError(I18NManager.GetResource(SearchResources.ValidationSearchFailedTitle), ex.Message);
        }
    }

    private bool CommitSearchCompletion(SearchCompletionItem? completion)
    {
        if (completion is null)
            return false;

        ++_searchCompletionUpdateVersion;
        SearchAutoCompleteBox.SelectedItem = null;
        SearchAutoCompleteBox.IsDropDownOpen = false;

        switch (completion.Kind)
        {
            case SearchCompletionKind.OpenIllustration:
                return TryOpenIllustrationPage();

            case SearchCompletionKind.OpenNovel:
                return TryOpenNovelPage();

            case SearchCompletionKind.OpenUser:
                return TryOpenUserPage();

            case SearchCompletionKind.SearchUser:
                OpenUserSearchPage();
                return true;

            case SearchCompletionKind.Tag:
                CommitTagCompletion(completion.Text);
                return true;

            default:
                throw new ArgumentOutOfRangeException(nameof(completion));
        }
    }

    private bool TryOpenIllustrationPage()
    {
        if (!TryGetSearchId(out var id) || TopLevel.GetTopLevel(this)?.ViewContainer is not { } viewContainer)
            return false;

        _ = viewContainer.CreateIllustrationPageAsync(id.ToString(), IPlatformInfo.Pixiv);
        return true;
    }

    private bool TryOpenNovelPage()
    {
        if (!TryGetSearchId(out var id) || TopLevel.GetTopLevel(this)?.ViewContainer is not { } viewContainer)
            return false;

        _ = viewContainer.CreateNovelPageAsync(id);
        return true;
    }

    private bool TryOpenUserPage()
    {
        if (!TryGetSearchId(out var id) || TopLevel.GetTopLevel(this)?.ViewContainer is not { } viewContainer)
            return false;

        _ = viewContainer.CreateUserPageAsync(id);
        return true;
    }

    private void OpenUserSearchPage()
    {
        if (TopLevel.GetTopLevel(this)?.ViewContainer is not { } viewContainer)
            return;

        viewContainer.NavigateTo(new UserSearchPage(SearchAutoCompleteBox.Text?.Trim()));
    }

    private void CommitTagCompletion(string tag)
    {
        var text = SearchAutoCompleteBox.Text ?? "";
        SearchAutoCompleteBox.Text = ApplyKeywordCompletion(text, tag);
        if (GetSearchTextBox() is { } textBox)
        {
            _ = textBox.Focus();
            textBox.CaretIndex = SearchAutoCompleteBox.Text?.Length ?? 0;
            textBox.SelectionStart = textBox.CaretIndex;
            textBox.SelectionEnd = textBox.CaretIndex;
        }
    }

    private bool TryGetSearchId(out long id)
        => long.TryParse(SearchAutoCompleteBox.Text?.Trim(), out id);

    private SearchCompletionItem? GetSelectedSearchCompletion()
        => SearchAutoCompleteBox.SelectedItem as SearchCompletionItem
           ?? _searchSuggestionItemsControl?.SelectedItem as SearchCompletionItem
           ?? _selectedSearchCompletion;

    private TextBox? GetSearchTextBox()
        => _searchTextBox ??= SearchAutoCompleteBox.GetVisualDescendants().OfType<TextBox>().FirstOrDefault();

    private bool HasSearchAutoCompleteBoxFocus()
        => SearchAutoCompleteBox.IsKeyboardFocusWithin
           || SearchAutoCompleteBox.IsFocused
           || GetSearchTextBox()?.IsFocused is true;

    private static string GetLastKeyword(string text)
    {
        var lastSpaceIndex = text.LastIndexOf(' ');
        return lastSpaceIndex < 0 ? text : text[(lastSpaceIndex + 1)..];
    }

    private static string ApplyKeywordCompletion(string text, string keyword)
    {
        var lastSpaceIndex = text.LastIndexOf(' ');
        return lastSpaceIndex < 0
            ? keyword
            : string.Concat(text.AsSpan(0, lastSpaceIndex + 1), keyword);
    }
}
