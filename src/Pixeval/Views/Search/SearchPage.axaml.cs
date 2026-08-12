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
using Avalonia.Input.Platform;
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
using Pixeval.ViewModels.Search;
using Pixeval.Views.Capability;
using Pixeval.Views.Viewers;

namespace Pixeval.Views.Search;

public partial class SearchPage : IconContentPage
{
    private const string SearchTextBoxPart = "PART_TextBox";
    private const string SearchSelectingItemsControlPart = "PART_SelectingItemsControl";
    private static readonly TimeSpan _SearchTagCompletionDelay = TimeSpan.FromSeconds(1);

    private TextBox? _searchTextBox;
    private SelectingItemsControl? _searchSuggestionItemsControl;
    private IReadOnlyList<SearchCompletionItem> _searchCompletionItems = [];
    private SearchCompletionItem? _selectedSearchCompletion;
    private CancellationTokenSource? _searchCompletionCancellationTokenSource;
    private int _searchCompletionUpdateVersion;
    private bool _isUpdatingSearchCompletions;
    private string? _searchCompletionSource;

    public SearchPage()
    {
        InitializeComponent();
    }

    private async void CopyButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control { DataContext: SearchHistoryEntry entry }
            || TopLevel.GetTopLevel(this) is not { ViewContainer: { } viewContainer, Clipboard: { } clipboard })
            return;

        await clipboard.SetTextAsync(entry.Value);
        viewContainer?.ShowSuccess(I18NManager.GetResource(MiscResources.Copied));
    }

    private void DeleteButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control { DataContext: SearchHistoryEntry entry })
            return;
        _ = App.AppViewModel.HistoryPersistHelper.SearchHistoryEntries.Remove(entry);
    }

    private void TrendingTagButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control { DataContext: TrendingTag tag })
            return;
        if (TopLevel.GetTopLevel(this)?.ViewContainer is not { } viewContainer)
            return;
        if (DataContext is not SearchPageViewModel viewModel)
            return;

        viewContainer.NavigateTo(new WorkSearchResultPage(tag.Tag, viewModel.SelectedTrendingTagsType));
    }

    private void SearchButton_OnClick(object? sender, RoutedEventArgs e)
    {
        ExecuteSearch(SearchAutoCompleteBox.Text?.Trim(), AdvancedExpander.IsExpanded);
    }

    private void AdvancedOptionsTabControl_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (sender is not TabbedPage
            {
                CurrentPage: SauceNaoSearchPage
                {
                    DataContext: SauceNaoSearchPageViewModel viewModel
                } page
            }
            || !SauceNaoSearchPageViewModel.IsApiKeyExisted)
            return;

        _ = KeyboardShortcut.TryExecutePaste(e, viewModel.PasteCommand, page);
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

        _searchTextBox =
            e.NameScope.Find<TextBox>(SearchTextBoxPart)
            ?? SearchAutoCompleteBox.GetVisualDescendants().OfType<TextBox>().FirstOrDefault();
        _searchSuggestionItemsControl = e.NameScope.Find<SelectingItemsControl>(SearchSelectingItemsControlPart);
        if (_searchSuggestionItemsControl is not null)
        {
            _searchSuggestionItemsControl.SelectionChanged += SearchSuggestionItemsControl_OnSelectionChanged;
            _searchSuggestionItemsControl.AddHandler(
                InputElement.PointerPressedEvent,
                SearchSuggestionItemsControl_OnPointerPressed,
                RoutingStrategies.Bubble,
                handledEventsToo: true);
        }
    }

    private void DetachSearchSuggestionItemsControl()
    {
        if (_searchSuggestionItemsControl is not null)
        {
            _searchSuggestionItemsControl.SelectionChanged -= SearchSuggestionItemsControl_OnSelectionChanged;
            _searchSuggestionItemsControl.RemoveHandler(
                InputElement.PointerPressedEvent,
                SearchSuggestionItemsControl_OnPointerPressed);
        }

        _searchSuggestionItemsControl = null;
    }

    private void SearchSuggestionItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.OfType<SearchCompletionItem>().LastOrDefault() is { } completion)
            _selectedSearchCompletion = completion;
    }

    private void SearchSuggestionItemsControl_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.Properties.IsLeftButtonPressed || e.Source is not Control source)
            return;

        var completion = source.GetSelfAndVisualAncestors()
            .OfType<Control>()
            .Select(control => control.DataContext)
            .OfType<SearchCompletionItem>()
            .FirstOrDefault();
        if (completion is not null)
            Dispatcher.UIThread.Post(() => CommitSearchCompletion(completion));
    }

    private void SearchAutoCompleteBox_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (!_isUpdatingSearchCompletions)
            UpdateSearchCompletions(SearchAutoCompleteBox.Text);
    }

    private void SearchAutoCompleteBox_OnTapped(object? sender, TappedEventArgs e)
    {
        if (sender is AutoCompleteBox box)
            box.IsDropDownOpen = true;
    }

    private void UpdateSearchCompletions(string? text)
    {
        var source = text ?? "";
        if (string.Equals(source, _searchCompletionSource, StringComparison.Ordinal))
            return;

        _searchCompletionSource = source;
        ++_searchCompletionUpdateVersion;
        _isUpdatingSearchCompletions = true;
        try
        {
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
        finally
        {
            _isUpdatingSearchCompletions = false;
        }
    }

    private IReadOnlyList<SearchCompletionItem> CreateImmediateSearchCompletions(string normalized)
    {
        var suggestions = new List<SearchCompletionItem>();
        if (normalized.All(char.IsDigit))
        {
            suggestions.Add(new(
                SearchCompletionKind.OpenIllustration,
                I18NManager.GetResource(SearchResources.OpenId.Illustration),
                normalized));
            suggestions.Add(new(
                SearchCompletionKind.OpenNovel,
                I18NManager.GetResource(SearchResources.OpenId.Novel),
                normalized));
            suggestions.Add(new(
                SearchCompletionKind.OpenUser,
                I18NManager.GetResource(SearchResources.OpenId.User),
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

            await Task.Delay(_SearchTagCompletionDelay, token).ConfigureAwait(false);
            var tags = await App.AppViewModel.MakoClient
                .GetAutoCompletionForKeyword(keyword, true, token)
                .ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            if (version != _searchCompletionUpdateVersion)
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
        {
            ClearSearchCompletionItems();
            return;
        }

        if (!_searchCompletionItems.SequenceEqual(suggestions))
        {
            _searchCompletionItems = suggestions;
            _selectedSearchCompletion = null;
            SearchAutoCompleteBox.SelectedItem = null;
            SearchAutoCompleteBox.ItemsSource = suggestions;
        }
    }

    private void ClearSearchCompletionItems()
    {
        _searchCompletionCancellationTokenSource?.Cancel();
        _searchCompletionCancellationTokenSource = null;
        _searchCompletionItems = [];
        _selectedSearchCompletion = null;
        SearchAutoCompleteBox.SelectedItem = null;
        SearchAutoCompleteBox.ItemsSource = Array.Empty<SearchCompletionItem>();
        SearchAutoCompleteBox.IsDropDownOpen = false;
    }

    private string SearchAutoCompleteBox_OnSelectItem(string? text, object item)
        => SearchAutoCompleteBox.Text ?? "";

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
                I18NManager.GetResource(MainPageResources.SearchKeywordCannotBeBlank.Title),
                I18NManager.GetResource(MainPageResources.SearchKeywordCannotBeBlank.Content));
            return;
        }

        try
        {
            if (!advanced)
            {
                viewContainer.NavigateTo(new WorkSearchResultPage(searchText, viewModel.SelectedAdvancedOptionsType));
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
                viewContainer.NavigateTo(new WorkSearchResultPage(arguments));
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
                viewContainer.NavigateTo(new WorkSearchResultPage(arguments));
            }
        }
        catch (Exception ex)
        {
            viewContainer.ShowError(I18NManager.GetResource(SearchResources.Validation.SearchFailed.Title), ex.Message);
        }
    }

    private bool CommitSearchCompletion(SearchCompletionItem? completion)
    {
        if (completion is null)
            return false;

        ++_searchCompletionUpdateVersion;
        ClearSearchCompletionItems();
        _searchCompletionSource = null;

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

        viewContainer.CreateIllustrationPage(id.ToString(), IPlatformInfo.Pixiv);
        return true;
    }

    private bool TryOpenNovelPage()
    {
        if (!TryGetSearchId(out var id) || TopLevel.GetTopLevel(this)?.ViewContainer is not { } viewContainer)
            return false;

        viewContainer.CreateNovelPage(id);
        return true;
    }

    private bool TryOpenUserPage()
    {
        if (!TryGetSearchId(out var id) || TopLevel.GetTopLevel(this)?.ViewContainer is not { } viewContainer)
            return false;

        viewContainer.CreateUserPage(id);
        return true;
    }

    private void OpenUserSearchPage()
    {
        if (TopLevel.GetTopLevel(this)?.ViewContainer is not { } viewContainer)
            return;

        viewContainer.NavigateTo(new UserSearchResultPage(SearchAutoCompleteBox.Text?.Trim()));
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
