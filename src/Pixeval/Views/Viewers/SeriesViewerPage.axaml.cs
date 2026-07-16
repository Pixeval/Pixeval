// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.ComponentModel;
using Avalonia.Interactivity;
using Mako.Engine;
using Mako.Model;
using Pixeval.Utilities;
using Pixeval.ViewModels.Viewers;
using Pixeval.Views.Entry;

namespace Pixeval.Views.Viewers;

public partial class SeriesViewerPage : IconContentPage
{
    public SeriesViewerPage() : this(null)
    {
    }

    public SeriesViewerPage(SeriesViewerPageViewModel? viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
        if (viewModel is null)
            return;

        viewModel.PropertyChanged += ViewModel_OnPropertyChanged;
        if (viewModel is { SeriesDetail: { } seriesDetail, FirstWork: { } firstWork }
            && viewModel.TakeWorksViewModel() is { } worksViewModel)
        {
            SeriesContainerHost.Content = new SeriesContainer(
                viewModel.WorkType,
                viewModel.Id,
                worksViewModel,
                seriesDetail,
                firstWork);
        }
        else
        {
            SetWorksEngine(viewModel, viewModel.WorksEngine);
        }
    }

    private void ViewModel_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(SeriesViewerPageViewModel.WorksEngine)
            && sender is SeriesViewerPageViewModel viewModel)
            SetWorksEngine(viewModel, viewModel.WorksEngine);
    }

    private void SetWorksEngine(
        SeriesViewerPageViewModel viewModel,
        IFetchEngine<IWorkEntry>? engine)
    {
        SeriesContainerHost.Content = engine is not null
                                      && viewModel is { SeriesDetail: { } seriesDetail, FirstWork: { } firstWork }
            ? new SeriesContainer(viewModel.WorkType, viewModel.Id, engine, seriesDetail, firstWork)
            : null;
    }

    /// <inheritdoc />
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (DataContext is SeriesViewerPageViewModel viewModel)
            RaiseEvent(new ViewModelDisposalEventArgs(ViewModelDisposal.ViewModelDisposalEvent, viewModel));
    }
}
