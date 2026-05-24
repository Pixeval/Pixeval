// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Mako.Engine;
using Mako.Global.Enum;
using Misaki;
using Pixeval.Collections;
using Pixeval.Controls;
using Pixeval.Filters;
using Pixeval.I18N;
using Pixeval.Models.Filters;
using Pixeval.Models.Options;
using Pixeval.Utilities;
using Pixeval.ViewModels;

namespace Pixeval.Views.Work;

/// <summary>
/// 所有插画集合通用的容器
/// </summary>
public partial class WorkContainer : UserControl
{
    private const string FilterDiagnosticResourcePrefix = "Filter.Diagnostics.";

    private static AutoCompleteFilterPredicate<object?> FilterCompletionFilter { get; } = static (_, item) => item is FilterCompletionItem;

    private static AutoCompleteSelector<object> FilterCompletionSelector { get; } = static (_, item)
        => item is FilterCompletionItem completion ? completion.InsertText : item?.ToString() ?? "";

    private TextBox? _filterTextBox;
    private IReadOnlyCollection<IWorkViewModel>? _filterCompletionSource;
    private int _filterCompletionSourceCount = -1;
    private IReadOnlyList<FilterCompletionDefinition> _tagValueCompletions = [];
    private IReadOnlyList<FilterCompletionDefinition> _authorValueCompletions = [];

    public static readonly DirectProperty<WorkContainer, bool> IsRefreshEnabledProperty = AvaloniaProperty.RegisterDirect<WorkContainer, bool>(
        nameof(IsRefreshEnabled),
        o => o.IsRefreshEnabled,
        (o, v) => o.IsRefreshEnabled = v);

    public bool IsRefreshEnabled
    {
        get;
        set => SetAndRaise(IsRefreshEnabledProperty, ref field, value);
    } = true;

    public event EventHandler<RoutedEventArgs>? RefreshRequested;

    /// <summary>
    /// The command elements that will appear at the left of the TopCommandBar
    /// </summary>
    public AvaloniaList<Control> CommandBarElements { get; } = [];

    public WorkContainer()
    {
        InitializeComponent();
        FilterAutoSuggestBox.ItemFilter = FilterCompletionFilter;
        FilterAutoSuggestBox.ItemSelector = FilterCompletionSelector;

        CommandBarElements.CollectionChanged += (_, e) =>
        {
            if (e is { Action: NotifyCollectionChangedAction.Add, NewItems: { } newItems })
                foreach (Control argsNewItem in newItems)
                    ExtraCommandsBar.Children.Insert(0, argsNewItem);
            else
                throw new ArgumentException("This collection does not support operations except the Add");
        };
    }

    private void SelectAllToggleButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        WorkView.WorkListBox.SelectAll();
    }

    private void SortOptionComboBox_OnSelectionChanged(SymbolComboBox sender, EventArgs e) => SetSortOption();

    private void WorkView_OnDataContextChanged(object? sender, EventArgs args)
    {
        DataContext = (sender as Control)?.DataContext;
        ResetFilterValueCompletions();
        SetSortOption();
    }

    public void SetSortOption()
    {
        if (DataContext is IOperableViewViewModel vm && SortOptionComboBox.GetSelectedValue<LocalSortOption>() is var sortOption)
        {
            vm.SetSortDescriptions(MakoHelper.GetSortDescription(sortOption));

            ScrollToTop();
        }
    }

    private void ScrollToTop()
    {
         if (WorkView.WorkListBox.Scroll is { } scrollView)
             scrollView.Offset = new(0, 0);
    }

    private async void AddAllToBookmarkButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        await ShowBookmarkTagSelectorAsync(AddAllToBookmarkButton, null).ConfigureAwait(false);
    }

    private async void WorkView_OnRequestAddToBookmark(Control sender, IWorkViewModel e)
    {
        await ShowBookmarkTagSelectorAsync(sender, e).ConfigureAwait(false);
    }

    private async Task ShowBookmarkTagSelectorAsync(Control placementTarget, IWorkViewModel? target)
    {
        if (target is null && DataContext is not IOperableViewViewModel { SelectedEntries.Count: > 0 })
            return;

        await BookmarkTagSelectorFlyoutHelper.ShowAsync(
            placementTarget,
            GetTagSelectorWorkType(target),
            async e => await AddToBookmarkAsync(target, e),
            PlacementMode.Bottom);
    }

    private async Task AddToBookmarkAsync(IWorkViewModel? target, (bool isPrivate, IReadOnlyList<string> tags) e)
    {
        if (target is { } current)
        {
            await current.AddToBookmarkCommand.ExecuteAsync((e.tags, e.isPrivate, this));
            TopLevel.GetTopLevel(this)?.ViewContainer?.ShowSuccess(I18NManager.GetResource(EntryViewResources.AddedToBookmark));
            return;
        }

        if (DataContext is not IOperableViewViewModel viewModel)
            return;

        if (TopLevel.GetTopLevel(this)?.ViewContainer is { } viewContainer
            && viewModel.SelectedEntries.Count >= 20
            && await viewContainer.CreateOkCancelAsync(
                I18NManager.GetResource(WorkContainerResources.SelectedTooManyItemsForBookmarkTitle),
                I18NManager.GetResource(WorkContainerResources.SelectedTooManyItemsForBookmarkContent)) is not ContentDialogResult.Primary)
            return;

        foreach (var i in viewModel.SelectedEntries)
            await i.AddToBookmarkCommand.ExecuteAsync((e.tags, e.isPrivate, this));
        if (viewModel.SelectedEntries.Count is var c and > 0)
            TopLevel.GetTopLevel(this)?.ViewContainer?.ShowSuccess(I18NManager.GetResource(WorkContainerResources.AddedAllToBookmarkContentFormatted, c));
    }

    private SimpleWorkType GetTagSelectorWorkType(IWorkViewModel? target)
        => target is NovelItemViewModel || DataContext is NovelViewViewModel
            ? SimpleWorkType.Novel
            : SimpleWorkType.IllustrationAndManga;

    private async void SaveAllButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not IOperableViewViewModel viewModel)
            return;

        if (TopLevel.GetTopLevel(this)?.ViewContainer is { } viewContainer
            && viewModel.SelectedEntries.Count >= 20
            && await viewContainer.CreateOkCancelAsync(
                I18NManager.GetResource(WorkContainerResources.SelectedTooManyItemsTitle),
                I18NManager.GetResource(WorkContainerResources.SelectedTooManyItemsForSaveContent)) is not ContentDialogResult.Primary)
            return;

        foreach (var i in viewModel.SelectedEntries)
            i.SaveCommand.Execute(null);

        TopLevel.GetTopLevel(this)?.ViewContainer?.ShowInformation(
            I18NManager.GetResource(WorkContainerResources.DownloadItemsQueuedFormatted, viewModel.SelectedEntries.Count));
    }

    private async void OpenAllInBrowserButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not IOperableViewViewModel viewModel)
            return;

        if (TopLevel.GetTopLevel(this)?.ViewContainer is { } viewContainer
            && viewModel.SelectedEntries.Count > 15
            && await viewContainer.CreateOkCancelAsync(
                I18NManager.GetResource(WorkContainerResources.SelectedTooManyItemsTitle),
                I18NManager.GetResource(WorkContainerResources.SelectedTooManyItemsForOpenInBrowserContent)) is not ContentDialogResult.Primary)
            return;

        foreach (var selectedEntry in viewModel.SelectedEntries)
        {
            _ = await TopLevel.GetTopLevel(this)!.Launcher.LaunchUriAsync(selectedEntry.Entry.WebsiteUri);
        }
    }

    private void RefreshButton_OnClick(object? sender, RoutedEventArgs e) => RefreshRequested?.Invoke(sender, e);

    private void CancelSelectionButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        WorkView.WorkListBox.UnselectAll();
    }

    private void FilterAutoSuggestBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is not Key.Enter)
            return;

        if (FilterAutoSuggestBox.IsDropDownOpen && FilterAutoSuggestBox.SelectedItem is FilterCompletionItem completion)
        {
            FilterAutoSuggestBox.Text = completion.InsertText;
            FilterAutoSuggestBox.IsDropDownOpen = false;
            if (GetFilterTextBox() is { } textBox)
            {
                textBox.CaretIndex = completion.InsertText.Length;
                textBox.SelectionStart = textBox.CaretIndex;
                textBox.SelectionEnd = textBox.CaretIndex;
            }

            e.Handled = true;
            return;
        }

        PerformSearch(FilterAutoSuggestBox.Text);
    }

    private void FilterAutoSuggestBox_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        UpdateFilterCompletions(FilterAutoSuggestBox.Text);
    }

    private void FilterAutoSuggestBox_OnGotFocus(object? sender, RoutedEventArgs e)
    {
        UpdateFilterCompletions(FilterAutoSuggestBox.Text);
    }

    public void PerformSearch(string? text)
    {
        if (DataContext is not IOperableViewViewModel viewModel)
            return;

        if (string.IsNullOrWhiteSpace(text))
        {
            viewModel.UserFilter = null;
            viewModel.ViewRange = Range.All;
            ClearFilterSelection();
            UpdateFilterCompletions(text);
            return;
        }

        var analysis = AnalyzeFilter(text);
        UpdateFilterCompletions(text, analysis);
        if (!analysis.IsSuccess || analysis.Query is not { } query)
        {
            if (analysis.Diagnostics.Count > 0)
            {
                HighlightFilterDiagnostic(analysis.Diagnostics[0].Span);
                TopLevel.GetTopLevel(this)?.ViewContainer?.ShowError(
                    I18NManager.GetResource(MacroParserResources.FilterQueryError),
                    FormatDiagnosticMessage(analysis));
            }

            return;
        }

        viewModel.UserFilter = query.HasPredicates
            ? IFilter<IWorkViewModel>.Create(o => o.Filter(query.Root), false)
            : null;
        viewModel.ViewRange = query.ViewRange;
        ClearFilterSelection();
    }

    private FilterAnalysisResult AnalyzeFilter(string? text)
    {
        var caret = GetFilterTextBox()?.CaretIndex ?? text?.Length ?? 0;
        return WorkFilterLanguage.Instance.Analyze(text, caret, GetFilterValueCompletions);
    }

    private IReadOnlyList<FilterCompletionDefinition> GetFilterValueCompletions(FilterValueCompletionContext context)
    {
        if (DataContext is not IOperableViewViewModel { Source.Count: > 0 } viewModel)
            return [];

        EnsureFilterValueCompletions(viewModel.Source);
        return context.Match.Syntax.Key switch
        {
            WorkFilterSyntaxKeys.Tag => _tagValueCompletions,
            WorkFilterSyntaxKeys.Author => _authorValueCompletions,
            _ => []
        };
    }

    private void EnsureFilterValueCompletions(IReadOnlyCollection<IWorkViewModel> source)
    {
        if (ReferenceEquals(_filterCompletionSource, source) && _filterCompletionSourceCount == source.Count)
            return;

        var tags = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        var authors = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var work in source)
        {
            foreach (var author in work.Entry.Authors)
                _ = authors.Add(author.Name);

            foreach (var tagGroup in work.Entry.Tags)
            {
                foreach (var tag in tagGroup)
                {
                    if (string.IsNullOrWhiteSpace(tag.Name))
                        continue;

                    if (tags.TryGetValue(tag.Name, out var description) && !string.IsNullOrWhiteSpace(description))
                        continue;

                    tags[tag.Name] = string.IsNullOrWhiteSpace(tag.TranslatedName) || string.Equals(tag.Name, tag.TranslatedName, StringComparison.OrdinalIgnoreCase)
                        ? null
                        : tag.TranslatedName;
                }
            }
        }

        _tagValueCompletions = [.. tags.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase).Select(pair => new FilterCompletionDefinition($"tag:{pair.Key}", pair.Key, pair.Key, pair.Value))];
        _authorValueCompletions = [.. authors.OrderBy(name => name, StringComparer.OrdinalIgnoreCase).Select(name => new FilterCompletionDefinition($"author:{name}", name, name))];
        _filterCompletionSource = source;
        _filterCompletionSourceCount = source.Count;
    }

    private void ResetFilterValueCompletions()
    {
        _filterCompletionSource = null;
        _filterCompletionSourceCount = -1;
        _tagValueCompletions = [];
        _authorValueCompletions = [];
    }

    private void UpdateFilterCompletions(string? text, FilterAnalysisResult? analysis = null)
    {
        Debug.WriteLine(nameof(UpdateFilterCompletions));
        var normalized = text ?? "";
        analysis ??= AnalyzeFilter(normalized);
        var suggestions = analysis.Completions
            .Select(completion => completion with { InsertText = ApplyCompletion(normalized, completion) })
            .Where(completion => !string.Equals(completion.InsertText, normalized, StringComparison.Ordinal))
            .ToArray();

        FilterAutoSuggestBox.ItemsSource = suggestions.Length > 0 ? suggestions : null;
        if (suggestions.Length is 0)
            FilterAutoSuggestBox.SelectedItem = null;
        FilterAutoSuggestBox.IsDropDownOpen = suggestions.Length > 0 && (FilterAutoSuggestBox.IsFocused || GetFilterTextBox()?.IsFocused is true);
    }

    private static string ApplyCompletion(string source, FilterCompletionItem completion)
    {
        var start = Math.Clamp(completion.ReplacementSpan.Start, 0, source.Length);
        var end = Math.Clamp(completion.ReplacementSpan.End, start, source.Length);
        return string.Concat(source.AsSpan(0, start), completion.InsertText, source.AsSpan(end));
    }

    private static string FormatDiagnosticMessage(FilterAnalysisResult analysis)
    {
        var diagnostic = analysis.Diagnostics[0];
        var completionSuffix = analysis.Completions.Count > 0
            ? I18NManager.GetResource(
                FilterResources.DiagnosticsCompletionSuffixFormatted,
                string.Join(", ", analysis.Completions.Select(t => t.DisplayText).Distinct(StringComparer.OrdinalIgnoreCase).Take(6)))
            : "";
        return I18NManager.GetResource(
            FilterResources.DiagnosticsMessageWithPositionFormatted,
            FormatDiagnostic(diagnostic),
            diagnostic.Span.Start + 1,
            completionSuffix);
    }

    private static string FormatDiagnostic(FilterDiagnostic diagnostic)
    {
        var arguments = diagnostic.Arguments;
        return arguments.Count > 0
            ? I18NManager.GetResource(GetFilterDiagnosticResourceKey(diagnostic.Kind), [.. arguments])
            : I18NManager.GetResource(GetFilterDiagnosticResourceKey(diagnostic.Kind));
    }

    private static string GetFilterDiagnosticResourceKey(FilterDiagnosticKind kind) => FilterDiagnosticResourcePrefix + kind;

    private void HighlightFilterDiagnostic(FilterTextSpan span)
    {
        if (GetFilterTextBox() is not { } textBox)
            return;

        var textLength = FilterAutoSuggestBox.Text?.Length ?? 0;
        var start = Math.Clamp(span.Start, 0, textLength);
        var end = Math.Clamp(span.End, start, textLength);
        if (start == end && start < textLength)
            ++end;

        _ = textBox.Focus();
        textBox.SelectionStart = start;
        textBox.SelectionEnd = end;
        textBox.CaretIndex = end;
    }

    private void ClearFilterSelection()
    {
        if (GetFilterTextBox() is not { } textBox)
            return;

        var caret = Math.Clamp(textBox.CaretIndex, 0, FilterAutoSuggestBox.Text?.Length ?? 0);
        textBox.SelectionStart = caret;
        textBox.SelectionEnd = caret;
    }

    private TextBox? GetFilterTextBox()
        => _filterTextBox ??= FilterAutoSuggestBox.GetVisualDescendants().OfType<TextBox>().FirstOrDefault();

    public void ResetEngine(IFetchEngine<IArtworkInfo> newEngine, int itemsPerPage = 20, int itemLimit = -1)
    {
        WorkView.ResetEngine(newEngine, itemsPerPage, itemLimit);
    }

    public void SetSource(IReadOnlyCollection<IArtworkInfo> source)
    {
        WorkView.SetSource(source);
    }

    public static readonly FuncValueConverter<int, string> CancelSelectionButtonConverter = new(i => i > 0
        ? I18NManager.GetResource(WorkContainerResources.CancelSelectionButtonFormatted, i)
        : I18NManager.GetResource(WorkContainerResources.CancelSelectionButtonDefaultLabel));
}
