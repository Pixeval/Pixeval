// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Pixeval.Filters;

namespace Pixeval.Views.Work;

public sealed class WorkFilterSearchRequestedEventArgs(string? text, int caretIndex) : EventArgs
{
    public string? Text { get; } = text;

    public int CaretIndex { get; } = caretIndex;
}

public partial class WorkFilterAutoSuggestBox : UserControl
{
    private const string FilterTextBoxPart = "PART_TextBox";
    private const string FilterSelectingItemsControlPart = "PART_SelectingItemsControl";

    private TextBox? _filterTextBox;
    private SelectingItemsControl? _filterSuggestionItemsControl;
    private IReadOnlyList<FilterCompletionItem> _filterCompletionItems = [];
    private FilterCompletionItem? _selectedFilterCompletion;
    private int _filterCompletionUpdateVersion;
    private bool _isCommittingFilterCompletion;

    public WorkFilterAutoSuggestBox()
    {
        InitializeComponent();
        FilterAutoSuggestBox.ItemSelector = (_, _) => FilterAutoSuggestBox.Text ?? "";
        FilterAutoSuggestBox.AddHandler(KeyDownEvent, FilterAutoSuggestBox_OnKeyDown, RoutingStrategies.Tunnel);
    }

    public string? Text
    {
        get => FilterAutoSuggestBox.Text;
        set => FilterAutoSuggestBox.Text = value;
    }

    public int CaretIndex => GetCaretIndex(Text ?? "");

    public Func<string?, int, FilterAnalysisResult>? Analyze { get; set; }

    public event EventHandler<WorkFilterSearchRequestedEventArgs>? SearchRequested;

    public void RefreshCompletions(FilterAnalysisResult? analysis = null) => UpdateFilterCompletions(Text, analysis);

    public void HighlightDiagnostic(FilterTextSpan span)
    {
        if (GetFilterTextBox() is not { } textBox)
            return;

        var textLength = Text?.Length ?? 0;
        var start = Math.Clamp(span.Start, 0, textLength);
        var end = Math.Clamp(span.End, start, textLength);
        if (start == end && start < textLength)
            ++end;

        _ = textBox.Focus();
        textBox.SelectionStart = start;
        textBox.SelectionEnd = end;
        textBox.CaretIndex = end;
    }

    public void ClearSelection()
    {
        if (GetFilterTextBox() is not { } textBox)
            return;

        var caret = Math.Clamp(textBox.CaretIndex, 0, Text?.Length ?? 0);
        textBox.SelectionStart = caret;
        textBox.SelectionEnd = caret;
    }

    private void FilterAutoSuggestBox_OnTemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
        DetachFilterSuggestionItemsControl();

        _filterTextBox = e.NameScope.Find<TextBox>(FilterTextBoxPart)
                         ?? FilterAutoSuggestBox.GetVisualDescendants().OfType<TextBox>().FirstOrDefault();
        _filterSuggestionItemsControl = e.NameScope.Find<SelectingItemsControl>(FilterSelectingItemsControlPart);
        if (_filterSuggestionItemsControl is not null)
        {
            _filterSuggestionItemsControl.SelectionChanged += FilterSuggestionItemsControl_OnSelectionChanged;
            _filterSuggestionItemsControl.PointerReleased += FilterSuggestionItemsControl_OnPointerReleased;
        }
    }

    private void DetachFilterSuggestionItemsControl()
    {
        if (_filterSuggestionItemsControl is not null)
        {
            _filterSuggestionItemsControl.SelectionChanged -= FilterSuggestionItemsControl_OnSelectionChanged;
            _filterSuggestionItemsControl.PointerReleased -= FilterSuggestionItemsControl_OnPointerReleased;
        }
        _filterSuggestionItemsControl = null;
    }

    private void FilterSuggestionItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.OfType<FilterCompletionItem>().LastOrDefault() is { } completion)
            _selectedFilterCompletion = completion;
    }

    private void FilterSuggestionItemsControl_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton is not MouseButton.Left)
            return;

        var selectedItem = GetSelectedFilterCompletion();
        Dispatcher.UIThread.Post(() =>
        {
            _ = CommitFilterCompletion(selectedItem ?? GetSelectedFilterCompletion());
        });
    }

    private void FilterAutoSuggestBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Handled)
            return;

        switch (e.Key)
        {
            case Key.Enter:
                if (FilterAutoSuggestBox.IsDropDownOpen && CommitFilterCompletion(GetSelectedFilterCompletion()))
                {
                    e.Handled = true;
                    return;
                }

                SearchRequested?.Invoke(this, new(Text, CaretIndex));
                e.Handled = true;
                break;

            case Key.Tab:
                e.Handled = FilterAutoSuggestBox.IsDropDownOpen && SelectFilterCompletion(1, true);
                break;
        }
    }

    private bool SelectFilterCompletion(int offset, bool wrapToItem = false)
    {
        if (_filterSuggestionItemsControl is not { ItemCount: > 0 } suggestions)
            return false;

        var selectedIndex = suggestions.SelectedIndex;
        suggestions.SelectedIndex = offset > 0
            ? selectedIndex + 1 >= suggestions.ItemCount ? wrapToItem ? 0 : -1 : selectedIndex + 1
            : selectedIndex > 0 ? selectedIndex - 1 : suggestions.ItemCount - 1;
        return true;
    }

    private FilterCompletionItem? GetSelectedFilterCompletion()
        => FilterAutoSuggestBox.SelectedItem as FilterCompletionItem
           ?? _filterSuggestionItemsControl?.SelectedItem as FilterCompletionItem
           ?? _selectedFilterCompletion;

    private bool CommitFilterCompletion(FilterCompletionItem? completion)
    {
        if (completion is null)
            return false;

        var insertText = completion.InsertText;
        ++_filterCompletionUpdateVersion;
        _isCommittingFilterCompletion = true;
        try
        {
            _filterCompletionItems = [];
            _selectedFilterCompletion = null;
            FilterAutoSuggestBox.SelectedItem = null;
            FilterAutoSuggestBox.IsDropDownOpen = false;
            Text = insertText;
        }
        finally
        {
            _isCommittingFilterCompletion = false;
        }

        if (GetFilterTextBox() is { } textBox)
        {
            _ = textBox.Focus();
            textBox.CaretIndex = insertText.Length;
            textBox.SelectionStart = textBox.CaretIndex;
            textBox.SelectionEnd = textBox.CaretIndex;
        }

        return true;
    }

    private void FilterAutoSuggestBox_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (!_isCommittingFilterCompletion)
            QueueUpdateFilterCompletions();
    }

    private void QueueUpdateFilterCompletions()
    {
        var version = ++_filterCompletionUpdateVersion;
        Dispatcher.UIThread.Post(() =>
        {
            if (version == _filterCompletionUpdateVersion && !_isCommittingFilterCompletion)
                UpdateFilterCompletions(Text);
        });
    }

    private void FilterAutoSuggestBox_OnGotFocus(object? sender, RoutedEventArgs e)
    {
        QueueUpdateFilterCompletions();
    }

    private void UpdateFilterCompletions(string? text, FilterAnalysisResult? analysis = null)
    {
        if (Analyze is null)
        {
            ClearFilterCompletionItems();
            return;
        }

        var normalized = text ?? "";
        analysis ??= Analyze(normalized, GetCaretIndex(normalized));
        var suggestions = analysis.Completions
            .Select(completion => completion with { InsertText = ApplyCompletion(normalized, completion) })
            .Where(completion => !string.Equals(completion.InsertText, normalized, StringComparison.Ordinal))
            .ToArray();

        var suggestionsChanged = !_filterCompletionItems.SequenceEqual(suggestions);
        _filterCompletionItems = suggestions;
        var activeCompletion = GetActiveFilterCompletion();
        _selectedFilterCompletion = activeCompletion is not null && suggestions.Contains(activeCompletion)
            ? activeCompletion
            : null;
        if (suggestions.Length is 0)
        {
            ClearFilterCompletionItems();
        }
        else if (suggestionsChanged || FilterAutoSuggestBox.ItemsSource is null)
        {
            FilterAutoSuggestBox.ItemsSource = suggestions;
        }
        FilterAutoSuggestBox.IsDropDownOpen = !_isCommittingFilterCompletion && suggestions.Length > 0 && HasFilterAutoSuggestBoxFocus();
    }

    private int GetCaretIndex(string text)
        => Math.Clamp(GetFilterTextBox()?.CaretIndex ?? text.Length, 0, text.Length);

    private void ClearFilterCompletionItems()
    {
        _filterCompletionItems = [];
        _selectedFilterCompletion = null;
        FilterAutoSuggestBox.SelectedItem = null;
        FilterAutoSuggestBox.ItemsSource = null;
    }

    private bool HasFilterAutoSuggestBoxFocus()
        => FilterAutoSuggestBox.IsKeyboardFocusWithin
           || FilterAutoSuggestBox.IsFocused
           || GetFilterTextBox()?.IsFocused is true;

    private FilterCompletionItem? GetActiveFilterCompletion()
        => FilterAutoSuggestBox.SelectedItem as FilterCompletionItem
           ?? _filterSuggestionItemsControl?.SelectedItem as FilterCompletionItem;

    private static string ApplyCompletion(string source, FilterCompletionItem completion)
    {
        var start = Math.Clamp(completion.ReplacementSpan.Start, 0, source.Length);
        var end = Math.Clamp(completion.ReplacementSpan.End, start, source.Length);
        return string.Concat(source.AsSpan(0, start), completion.InsertText, source.AsSpan(end));
    }

    private TextBox? GetFilterTextBox()
        => _filterTextBox ??= FilterAutoSuggestBox.GetVisualDescendants().OfType<TextBox>().FirstOrDefault();
}
