// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Avalonia.Controls;
using Avalonia.Interactivity;
using Mako.Engine;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.I18N;
using Pixeval.Models.Options;
using Pixeval.Models.Subscriptions;
using Pixeval.Utilities;
using Pixeval.ViewModels;

namespace Pixeval.Views.Entry;

public partial class SeriesContainer : UserControl
{
    private readonly SimpleWorkType _workType;
    private readonly long _seriesId;
    private readonly SeriesDetailBase? _seriesDetail;
    private readonly IWorkEntry? _firstWork;

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
    }

    private void AddSubscriptionButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        if (WorkSubscriptionHelper.TryAddOrUpdateSeries(
                _seriesId,
                GetSubscriptionWorkKind(),
                _seriesDetail,
                _firstWork))
            TopLevel.GetTopLevel(this)?.ViewContainer?.ShowSuccess(
                I18NManager.GetResource(WorkSubscriptionsSettingsExpanderResources.SubscriptionAdded));
    }

    private WorkSubscriptionWorkKind GetSubscriptionWorkKind() =>
        _workType is SimpleWorkType.Novel
            ? WorkSubscriptionWorkKind.Novel
            : WorkSubscriptionWorkKind.Manga;
}
