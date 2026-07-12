// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Mako.Global.Enum;
using Mako.Model;
using Misaki;
using Pixeval.Collections;
using Pixeval.Controls;
using Pixeval.Filters.Analysis;
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
        set
        {
            if (field == value)
                return;

            SetAndRaise(IsRefreshEnabledProperty, ref field, value);
            UpdateRefreshButton();
            return;

            void UpdateRefreshButton()
            {
                if (IsRefreshEnabled)
                {
                    if (!RightToolBar.SecondaryCommands.Contains(RefreshButton))
                        RightToolBar.SecondaryCommands.Insert(0, RefreshButton);

                    return;
                }

                _ = RightToolBar.SecondaryCommands.Remove(RefreshButton);
            }
        }
    } = true;

    public event EventHandler<RoutedEventArgs>? RefreshRequested;

    /// <summary>
    /// The command elements that will appear at the left of the TopCommandBar
    /// </summary>
    public AvaloniaList<Control> CommandBarElements { get; } = [];

    public AvaloniaList<ICommandBarElement> CommandBarSubElements { get; } = [];

    public WorkContainer()
    {
        InitializeComponent();

        CommandBarElements.CollectionChanged += (_, e) =>
        {
            if (e is { Action: NotifyCollectionChangedAction.Add, NewItems: { } newItems })
                foreach (Control argsNewItem in newItems)
                    ExtraCommandsBar.Children.Insert(0, argsNewItem);
            else
                throw new ArgumentException("This collection does not support operations except the Add");
        };

        CommandBarSubElements.CollectionChanged += (_, e) =>
        {
            if (e is { Action: NotifyCollectionChangedAction.Add, NewItems: { } newItems })
                foreach (ICommandBarElement argsNewItem in newItems)
                    RightToolBar.SecondaryCommands.Add(argsNewItem);
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
        await AddToBookmarkAsync(null, (false, null)).ConfigureAwait(false);
    }

    private async void AddAllToBookmarkButton_OnRightClicked(object? sender, ContextRequestedEventArgs e)
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

    private async Task AddToBookmarkAsync(IWorkViewModel? target, (bool IsPrivate, IReadOnlyList<string>? Tags) e)
    {
        if (target is not null)
        {
            await target.AddToBookmarkCommand.ExecuteAsync((e.Tags, e.IsPrivate, this));
            TopLevel.GetTopLevel(this)?.ViewContainer?.ShowSuccess(I18NManager.GetResource(EntryViewResources.AddedToBookmark));
            return;
        }

        if (DataContext is not IOperableViewViewModel viewModel)
            return;

        if (TopLevel.GetTopLevel(this)?.ViewContainer is { } viewContainer
            && viewModel.SelectedEntries.Count >= 20
            && await viewContainer.CreateOkCancelAsync(
                I18NManager.GetResource(WorkContainerResources.SelectedTooManyItemsForBookmarkTitle),
                I18NManager.GetResource(WorkContainerResources.SelectedTooManyItemsContent)) is not ContentDialogResult.Primary)
            return;

        foreach (var i in viewModel.SelectedEntries)
            await i.AddToBookmarkCommand.ExecuteAsync((e.Tags, e.IsPrivate, this));
        if (viewModel.SelectedEntries.Count is var c and > 0)
            TopLevel.GetTopLevel(this)?.ViewContainer?.ShowSuccess(I18NManager.GetResource(WorkContainerResources.AddedAllToBookmarkContentFormatted, c));
    }

    private SimpleWorkType GetTagSelectorWorkType(IWorkViewModel? target)
        => target is NovelItemViewModel || DataContext is NovelViewViewModel or SimpleOperableViewViewModel<NovelItemViewModel>
            ? SimpleWorkType.Novel
            : SimpleWorkType.Illustration;

    private async void SaveAllButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not IOperableViewViewModel viewModel)
            return;

        if (TopLevel.GetTopLevel(this)?.ViewContainer is { } viewContainer
            && viewModel.SelectedEntries.Count >= 20
            && await viewContainer.CreateOkCancelAsync(
                I18NManager.GetResource(WorkContainerResources.SelectedTooManyItemsForSaveTitle),
                I18NManager.GetResource(WorkContainerResources.SelectedTooManyItemsContent)) is not ContentDialogResult.Primary)
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
                I18NManager.GetResource(WorkContainerResources.SelectedTooManyItemsForOpenInBrowserTitle),
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

    private void WorkFilterAutoSuggestBox_OnSearchRequested(object? sender, WorkFilterSearchRequestedEventArgs e)
        => PerformSearch(e.Text, e.CaretIndex);

    public void PerformSearch(string? text) => PerformSearch(text, WorkFilterAutoSuggestBox.CaretIndex);

    private void PerformSearch(string? text, int caret)
    {
        if (DataContext is not IOperableViewViewModel viewModel)
            return;

        if (string.IsNullOrWhiteSpace(text))
        {
            viewModel.UserFilter = null;
            WorkFilterAutoSuggestBox.ClearSelection();
            WorkFilterAutoSuggestBox.RefreshCompletions();
            return;
        }

        var analysis = AnalyzeFilter(text, caret);
        WorkFilterAutoSuggestBox.RefreshCompletions(analysis);
        if (!analysis.IsSuccess || analysis.Query is not { } query)
        {
            if (analysis.Diagnostics.Count > 0)
            {
                WorkFilterAutoSuggestBox.HighlightDiagnostic(analysis.Diagnostics[0].Span);
                TopLevel.GetTopLevel(this)?.ViewContainer?.ShowError(
                    I18NManager.GetResource(FilterResources.FilterQueryError),
                    FormatDiagnosticMessage(analysis));
            }

            return;
        }

        viewModel.UserFilter = query.HasPredicates
            ? IFilter<IWorkViewModel>.Create(o => o.Filter(query.Root), false)
            : null;
        WorkFilterAutoSuggestBox.ClearSelection();
    }

    private FilterAnalysisResult AnalyzeFilter(string? text, int caret)
        => WorkFilterLanguage.Instance.Analyze(text, caret, GetFilterValueCompletions);

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
        var authors = new HashSet<IUser>();
        foreach (var work in source)
        {
            foreach (var author in work.Entry.Authors)
                _ = authors.Add(author);

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
        _authorValueCompletions = [.. authors.OrderBy(a => a.Name, StringComparer.OrdinalIgnoreCase).Select(a => new FilterCompletionDefinition($"author:{a.Name}", a.Name, a.Name, a is UserInfo user ? user.Account : null))];
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

    public void ResetEngine(IAsyncEnumerable<IArtworkInfo> newEngine)
    {
        WorkView.ResetEngine(newEngine);
    }

    /// <inheritdoc cref="WorkView.SetViewModel" />
    public void SetViewModel(IWorkViewViewModel viewModel)
    {
        WorkView.SetViewModel(viewModel);
    }

    public void SetSource(IReadOnlyCollection<IArtworkInfo> source, SimpleWorkType workType, bool needRefreshOnOpen = false)
    {
        WorkView.SetSource(source, workType, needRefreshOnOpen);
    }

    public static readonly FuncValueConverter<int, string> CancelSelectionButtonConverter = new(i => i > 0
        ? I18NManager.GetResource(WorkContainerResources.CancelSelectionButtonFormatted, i)
        : I18NManager.GetResource(WorkContainerResources.CancelSelectionButtonDefaultLabel));
}
