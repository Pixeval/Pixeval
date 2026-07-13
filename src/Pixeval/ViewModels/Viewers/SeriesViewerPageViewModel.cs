// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mako.Engine;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.I18N;
using Pixeval.Utilities;

namespace Pixeval.ViewModels.Viewers;

public sealed partial class SeriesViewerPageViewModel : ViewModelBase, IDisposable
{
    private readonly CancellationTokenSource _loadingCts = new();

    [ObservableProperty] public partial bool IsLoading { get; private set; }

    [ObservableProperty] public partial string? LoadErrorMessage { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Header))]
    [NotifyPropertyChangedFor(nameof(AuthorId))]
    [NotifyPropertyChangedFor(nameof(AuthorName))]
    [NotifyPropertyChangedFor(nameof(Caption))]
    [NotifyPropertyChangedFor(nameof(ContentCountText))]
    [NotifyPropertyChangedFor(nameof(CoverUrl))]
    public partial SeriesDetailBase? SeriesDetail { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CoverUrl))]
    public partial IWorkEntry? FirstWork { get; private set; }

    [ObservableProperty] public partial IFetchEngine<IWorkEntry>? WorksEngine { get; private set; }

    [ObservableProperty] public partial bool IsWatchlistAdded { get; private set; }

    private IWorkViewViewModel? _worksViewModel;

    public SeriesViewerPageViewModel(SimpleWorkType workType, long seriesId)
    {
        WorkType = workType;
        Id = seriesId;
        _ = LoadSeriesAsync();
    }

    public SeriesViewerPageViewModel(
        SimpleWorkType workType,
        long seriesId,
        SeriesDetailBase seriesDetail,
        IWorkEntry firstWork,
        IWorkViewViewModel worksViewModel)
    {
        WorkType = workType;
        Id = seriesId;
        FirstWork = firstWork;
        SeriesDetail = seriesDetail;
        _worksViewModel = worksViewModel;
    }

    public SimpleWorkType WorkType { get; }

    public long Id { get; }

    public string Header => SeriesDetail?.Title ?? Id.ToString();

    public long AuthorId => SeriesDetail?.User.Id ?? 0;

    public string? AuthorName => SeriesDetail?.User.Name;

    public string? Caption => SeriesDetail?.Caption;

    public string ContentCountText => I18NManager.GetResource(
        SeriesViewerPageResources.WorksCountFormatted,
        SeriesDetail switch
        {
            NovelSeriesDetail novel => novel.ContentCount,
            MangaSeriesDetail manga => manga.SeriesWorkCount,
            _ => 0
        });

    public string? CoverUrl => SeriesDetail is MangaSeriesDetail manga
        ? manga.CoverImageUrls.Medium
        : FirstWork?.GetThumbnailUrl();

    public IWorkViewViewModel? TakeWorksViewModel()
    {
        var viewModel = _worksViewModel;
        _worksViewModel = null;
        return viewModel;
    }

    partial void OnSeriesDetailChanged(SeriesDetailBase? value)
    {
        IsWatchlistAdded = value?.WatchlistAdded is true;
        AddToWatchlistCommand.NotifyCanExecuteChanged();
        RemoveFromWatchlistCommand.NotifyCanExecuteChanged();
    }

    private async Task LoadSeriesAsync()
    {
        var token = _loadingCts.Token;
        IsLoading = true;
        LoadErrorMessage = null;
        try
        {
            var (detail, first, engine) = await App.AppViewModel.MakoClient.GetWorkSeriesAsync(WorkType, Id);
            token.ThrowIfCancellationRequested();

            if (_disposed)
            {
                engine.EngineHandle.Cancel();
                return;
            }

            FirstWork = first;
            SeriesDetail = detail;
            WorksEngine = engine;
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception e)
        {
            if (!token.IsCancellationRequested)
                LoadErrorMessage = e.Message;
        }
        finally
        {
            if (!token.IsCancellationRequested && !_disposed)
                IsLoading = false;
        }
    }

    private bool CanChangeWatchlist => SeriesDetail is not null;

    [RelayCommand(CanExecute = nameof(CanChangeWatchlist))]
    private async Task AddToWatchlistAsync()
    {
        if (!await App.AppViewModel.MakoClient.PostWorkSeriesWatchlistAsync(WorkType, Id))
            return;

        SeriesDetail!.WatchlistAdded = true;
        IsWatchlistAdded = true;
    }

    [RelayCommand(CanExecute = nameof(CanChangeWatchlist))]
    private async Task RemoveFromWatchlistAsync()
    {
        if (!await App.AppViewModel.MakoClient.RemoveWorkSeriesWatchlistAsync(WorkType, Id))
            return;

        SeriesDetail!.WatchlistAdded = false;
        IsWatchlistAdded = false;
    }

    #region Dispose

    private bool _disposed;

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        IsLoading = false;
        _loadingCts.Cancel();
        _loadingCts.Dispose();
        var worksEngine = WorksEngine;
        WorksEngine = null;
        if (worksEngine is not null)
            worksEngine.EngineHandle.Cancel();
        _worksViewModel?.Dispose();
        _worksViewModel = null;
    }

    #endregion
}
