// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Mako.Engine;
using Mako.Global.Enum;
using Mako.Model;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Models.Options;
using Pixeval.Models.Subscriptions;
using Pixeval.Utilities;
using Pixeval.ViewModels;
using Pixeval.Views.Work;

namespace Pixeval.Views.Entry;

public partial class SeriesContainer : UserControl
{
    private readonly SimpleWorkType _workType;
    private readonly long _seriesId;
    private readonly SeriesDetailBase? _seriesDetail;
    private readonly IWorkEntry? _firstWork;

    private static IWorkSubscriptionService SubscriptionService =>
        App.AppViewModel.AppServiceProvider.GetRequiredService<IWorkSubscriptionService>();

    public SeriesContainer()
    {
        InitializeComponent();
    }

    public SeriesContainer(SimpleWorkType workType, long seriesId)
        : this(workType, seriesId, null, null)
    {
        ChangeSource();
    }

    public SeriesContainer(
        SimpleWorkType workType,
        long seriesId,
        IWorkViewViewModel viewModel,
        SeriesDetailBase seriesDetail,
        IWorkEntry firstWork)
        : this(workType, seriesId, seriesDetail, firstWork)
    {
        WorkContainer.IsRefreshEnabled = false;
        WorkContainer.SetViewModel(viewModel);
        UpdateSubscriptionButtons();
    }

    public SeriesContainer(
        SimpleWorkType workType,
        long seriesId,
        IFetchEngine<IWorkEntry> engine,
        SeriesDetailBase seriesDetail,
        IWorkEntry firstWork)
        : this(workType, seriesId, seriesDetail, firstWork)
    {
        WorkContainer.IsRefreshEnabled = false;
        SetEngine(engine);
    }

    private SeriesContainer(
        SimpleWorkType workType,
        long seriesId,
        SeriesDetailBase? seriesDetail,
        IWorkEntry? firstWork)
    {
        _workType = workType;
        _seriesId = seriesId;
        _seriesDetail = seriesDetail;
        _firstWork = firstWork;
        InitializeComponent();
    }

    private void WorkContainer_OnRefreshRequested(object? sender, RoutedEventArgs e) => ChangeSource();

    private void ChangeSource() => SetEngine(App.AppViewModel.MakoClient.WorkSeries(_workType, _seriesId));

    private void SetEngine(IFetchEngine<IWorkEntry> engine)
    {
        WorkContainer.ResetEngine(engine);
        App.AppViewModel.QueueWorkSubscriptionSyncCurrentSource(
            _seriesId,
            WorkSubscriptionType.Series,
            GetSubscriptionWorkKind(),
            engine);
        UpdateSubscriptionButtons();
    }

    private async void AddSubscriptionButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        var workKind = GetSubscriptionWorkKind();
        _ = await WorkSubscriptionButtonHelper.RunAsync(
            AddSubscriptionButton,
            RemoveSubscriptionButton,
            () => Task.FromResult(WorkSubscriptionHelper.TryAddOrUpdateSeries(_seriesId, workKind, _seriesDetail, _firstWork)),
            UpdateSubscriptionButtons);
    }

    private async void RemoveSubscriptionButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        _ = await WorkSubscriptionButtonHelper.RunAsync(
            AddSubscriptionButton,
            RemoveSubscriptionButton,
            RemoveCurrentSubscriptionAsync,
            UpdateSubscriptionButtons);
    }

    private void UpdateSubscriptionButtons() =>
        WorkSubscriptionButtonHelper.UpdateVisibility(
            AddSubscriptionButton,
            RemoveSubscriptionButton,
            SubscriptionService.TryGetSubscription(_seriesId, WorkSubscriptionType.Series, GetSubscriptionWorkKind()) is not null);

    private async Task<bool> RemoveCurrentSubscriptionAsync()
    {
        if (SubscriptionService.TryGetSubscription(_seriesId, WorkSubscriptionType.Series, GetSubscriptionWorkKind()) is not { HistoryEntryId: var historyEntryId })
            return false;

        _ = await SubscriptionService.TryRemoveAsync(historyEntryId);
        return true;
    }

    private WorkSubscriptionWorkKind GetSubscriptionWorkKind() =>
        _workType is SimpleWorkType.Novel
            ? WorkSubscriptionWorkKind.Novel
            : WorkSubscriptionWorkKind.Manga;
}
