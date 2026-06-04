// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Mako.Global.Enum;
using Mako.Model;
using Misaki;
using Pixeval.AppManagement;
using Pixeval.I18N;
using Pixeval.Models.Options;
using Pixeval.Views.Viewers;

namespace Pixeval.ViewModels.Viewers;

public sealed partial class IllustrationViewerPageViewModel : PagedViewerViewModel, IDisposable
{
    private readonly DispatcherTimer _autoPlayTimer = new();

    private readonly Dictionary<int, IllustrationItemViewModel> _refreshedIllustrations = [];

    private readonly bool _needRefresh;

    private CancellationTokenSource _loadingCts = new();

    private readonly ISourceView<IllustrationItemViewModel>? _sourceView;

    [ObservableProperty]
    public partial bool IsLoading { get; private set; }

    [ObservableProperty]
    public partial string? LoadErrorMessage { get; private set; }

    public string? LogoUri => CurrentIllustration?.Entry.Platform is { } platform ? $"avares://Pixeval/Assets/Platforms/{platform}.png" : null;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="illustrationViewModel"></param>
    /// <param name="needRefresh"></param>
    public IllustrationViewerPageViewModel(IllustrationItemViewModel illustrationViewModel, bool needRefresh)
    {
        _needRefresh = needRefresh;
        CurrentIllustration = illustrationViewModel;
        InitializeAutoPlayTimer();
        CurrentWorkIndex = 0;
    }

    public IllustrationViewerPageViewModel(IIdentityInfo info)
    {
        InitializeAutoPlayTimer();
        _ = LoadSingleIllustrationAsync(info, _loadingCts.Token);
    }

    private async Task LoadSingleIllustrationAsync(IIdentityInfo i, CancellationToken token)
    {
        var illustration = await LoadIllustrationAsync(i, item => CurrentIllustration = item, token);

        if (illustration is not null && !token.IsCancellationRequested)
            CurrentWorkIndex = 0;
    }

    /// <summary>
    /// 当拥有独立DataProvider引用的时候调用这个构造函数，dispose的时候会自动dispose掉DataProvider
    /// </summary>
    /// <param name="dataProvider"></param>
    /// <param name="currentIllustrationIndex"></param>
    /// <param name="needRefresh"></param>
    /// <remarks>
    /// illustrations should contain only one item if the illustration is a single
    /// otherwise it contains the entire manga data
    /// </remarks>
    public IllustrationViewerPageViewModel(ISourceView<IllustrationItemViewModel> dataProvider, int currentIllustrationIndex, bool needRefresh)
    {
        _needRefresh = needRefresh;
        _sourceView = dataProvider;
        InitializeAutoPlayTimer();
        CurrentWorkIndex = currentIllustrationIndex;
    }

    public IReadOnlyList<Page> PanePages =>
        CurrentImage?.ThumbnailViewModel.Entry is Illustration { Id: var id } illustration
            ?
            [
                new WorkInfoPage(illustration)
                {
                    ActionZone = new Border
                    {
                        Width = 32,
                        Height = 32,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Top,
                        IsHitTestVisible = false
                    }
                },
                new CommentsPage(new CommentsViewViewModel(SimpleWorkType.IllustrationAndManga, id)),
                new RelatedWorksPage { IllustrationId = id }
            ]
            : [];

    #region Current相关

    /// <summary>
    /// 当前插画
    /// </summary>
    public IllustrationItemViewModel? CurrentIllustration
    {
        get
        {
            if (_refreshedIllustrations.TryGetValue(CurrentWorkIndex, out var value))
                return value;

            if (field is not null)
                return field;

            return CurrentWorkIndex < 0 || CurrentWorkIndex >= WorkCount
                ? null
                : _sourceView?.View[CurrentWorkIndex];
        }
        private set
        {
            if (Equals(value, field))
                return;
            field = value;
            NotifyCurrentIllustrationChanged();
        }
    }

    /// <summary>
    /// 当前图集的ViewModel
    /// </summary>
    public ImageViewerViewModel? CurrentImage
    {
        get;
        private set
        {
            if (field == value)
                return;
            field?.PropertyChanged -= CurrentImageOnPropertyChanged;
            field?.Dispose();
            field = value;
            field?.PropertyChanged += CurrentImageOnPropertyChanged;
            OnPropertyChanged();
            ResetCurrentPageIndex();
            OnPropertyChanged(nameof(PageCount));
            NotifyCurrentIllustrationChanged();
        }
    } = null!;

    /// <summary>
    /// 当前插画的索引
    /// </summary>
    public override int CurrentWorkIndex
    {
        get;
        set
        {
            if (value is -1)
                return;
            if (value == field)
                return;

            field = value;

            _ = LoadCurrentIllustrationAsync();
            OnPropertyChanged();
            NotifyCurrentIllustrationChanged();
            NotifyPageNavigationChanged();
        }
        // 第一次赋值属性时会判断 value == field，如果是0则无法进入set方法体
        // ReSharper disable once MemberInitializerValueIgnored
    } = -1;

    private async Task LoadCurrentIllustrationAsync()
    {
        if (_disposed)
            return;

        var index = CurrentWorkIndex;
        var token = ResetLoadingToken();
        LoadErrorMessage = null;
        CurrentImage = null;

        if (await GetCurrentIllustrationAsync(index, token) is { } currentIllustration
            && !token.IsCancellationRequested
            && !_disposed
            && index == CurrentWorkIndex)
        {
            CurrentImage = new ImageViewerViewModel(currentIllustration);
        }
        return;

        async ValueTask<IllustrationItemViewModel?> GetCurrentIllustrationAsync(int index, CancellationToken token)
        {
            if (!_needRefresh)
            {
                IsLoading = false;
                return CurrentIllustration;
            }

            if (_sourceView is not null && _refreshedIllustrations.TryGetValue(index, out var cached))
            {
                IsLoading = false;
                return cached;
            }

            // 需刷新
            if (CurrentIllustration is not { Entry: IIdentityInfo info })
            {
                IsLoading = false;
                return null;
            }

            return await LoadIllustrationAsync(
                info,
                item =>
                {
                    // 单项刷新写回当前项，列表刷新只缓存到当前 Viewer，避免污染原始 SourceView。
                    if (_sourceView is null)
                        CurrentIllustration = item;
                    else
                        _refreshedIllustrations[index] = item;
                },
                token);
        }
    }

    private CancellationToken ResetLoadingToken()
    {
        _loadingCts.Cancel();
        _loadingCts.Dispose();
        _loadingCts = new();
        return _loadingCts.Token;
    }

    private async Task<IllustrationItemViewModel?> LoadIllustrationAsync(IIdentityInfo info, Action<IllustrationItemViewModel> onLoaded, CancellationToken token)
    {
        IsLoading = true;
        LoadErrorMessage = null;
        try
        {
            var entry = await info.TryGetArtworkInfoAsync();
            token.ThrowIfCancellationRequested();
            if (entry is null)
            {
                LoadErrorMessage = I18NManager.GetResource(EntryViewerPageResources.LoadFailed);
                return null;
            }

            var item = IllustrationItemViewModel.CreateInstance(entry);
            onLoaded(item);
            return item;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (Exception e)
        {
            if (!token.IsCancellationRequested)
                LoadErrorMessage = e.Message;
            return null;
        }
        finally
        {
            if (!token.IsCancellationRequested)
                IsLoading = false;
        }
    }

    /// <summary>
    /// 当前插画的页面索引
    /// </summary>
    public override int CurrentPageIndex
    {
        get => CurrentImage?.SelectedPageIndex ?? 0;
        set
        {
            if (CurrentImage is null)
                return;

            CurrentImage.SelectedPageIndex = value;
            // 不检查值是否变化，强制触发更新事件
            NotifyCurrentPageIndexChanged();
        }
    }

    private void ResetCurrentPageIndex()
    {
        CurrentImage?.SelectedPageIndex = 0;

        NotifyCurrentPageIndexChanged();
    }

    private void NotifyCurrentPageIndexChanged()
    {
        OnPropertyChanged(nameof(CurrentPageIndex));
        NotifyPageNavigationChanged();
    }

    private void NotifyCurrentIllustrationChanged()
    {
        OnPropertyChanged(nameof(CurrentIllustration));
        OnPropertyChanged(nameof(LogoUri));
        OnPropertyChanged(nameof(PanePages));
    }

    private void NotifyPageNavigationChanged()
    {
        OnPropertyChanged(nameof(PrevButtonText));
        OnPropertyChanged(nameof(NextButtonText));
        PrevCommand.NotifyCanExecuteChanged();
        NextCommand.NotifyCanExecuteChanged();
        PrevWorkCommand.NotifyCanExecuteChanged();
        NextWorkCommand.NotifyCanExecuteChanged();
    }

    private void CurrentImageOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ImageViewerViewModel.SelectedPageIndex))
            CurrentPageIndex = CurrentImage?.SelectedPageIndex ?? 0;
    }

    public override int PageCount => CurrentImage?.PageCount ?? 1;

    public override int WorkCount => Illustrations?.Count ?? 1;

    /// <summary>
    /// 插画列表
    /// </summary>
    public IReadOnlyList<IllustrationItemViewModel>? Illustrations => _sourceView?.View;

    #endregion

    #region AutoPlay

    public int AutoPlayInterval
    {
        get => App.AppViewModel.AppSettings.IllustrationViewerAutoPlayInterval;
        set
        {
            value = Math.Clamp(value, 1, 60);
            if (App.AppViewModel.AppSettings.IllustrationViewerAutoPlayInterval == value)
                return;

            App.AppViewModel.AppSettings.IllustrationViewerAutoPlayInterval = value;
            SaveAutoPlaySettings();
            OnPropertyChanged();
            UpdateAutoPlayTimerInterval();
        }
    }

    public IllustrationViewerAutoPlayMode AutoPlayMode
    {
        get => App.AppViewModel.AppSettings.IllustrationViewerAutoPlayMode;
        set
        {
            if (App.AppViewModel.AppSettings.IllustrationViewerAutoPlayMode == value)
                return;

            App.AppViewModel.AppSettings.IllustrationViewerAutoPlayMode = value;
            SaveAutoPlaySettings();
            OnPropertyChanged();
        }
    }

    public IllustrationViewerAutoPlayScope AutoPlayScope
    {
        get => App.AppViewModel.AppSettings.IllustrationViewerAutoPlayScope;
        set
        {
            if (App.AppViewModel.AppSettings.IllustrationViewerAutoPlayScope == value)
                return;

            App.AppViewModel.AppSettings.IllustrationViewerAutoPlayScope = value;
            SaveAutoPlaySettings();
            OnPropertyChanged();
        }
    }

    [ObservableProperty]
    public partial bool IsAutoPlaying { get; set; }

    partial void OnIsAutoPlayingChanged(bool value)
    {
        if (value)
            StartAutoPlay();
        else
            StopAutoPlay();
        return;

        void StartAutoPlay()
        {
            UpdateAutoPlayTimerInterval();
            _autoPlayTimer.Start();
        }

        void StopAutoPlay() => _autoPlayTimer.Stop();
    }

    private void InitializeAutoPlayTimer()
    {
        _autoPlayTimer.Tick += AutoPlayTimerOnTick;
        UpdateAutoPlayTimerInterval();
    }

    private void UpdateAutoPlayTimerInterval() => _autoPlayTimer.Interval = TimeSpan.FromSeconds(AutoPlayInterval);

    private void AutoPlayTimerOnTick(object? sender, EventArgs e) => MoveAutoPlayNext();

    private void MoveAutoPlayNext()
    {
        switch (AutoPlayScope, AutoPlayMode)
        {
            case (IllustrationViewerAutoPlayScope.CurrentWork, IllustrationViewerAutoPlayMode.Sequential):
                if (CurrentPageIndex < PageCount - 1)
                    CurrentPageIndex++;
                else
                    IsAutoPlaying = false;
                break;
            case (IllustrationViewerAutoPlayScope.CurrentWork, IllustrationViewerAutoPlayMode.Loop):
                CurrentPageIndex = (CurrentPageIndex + 1) % PageCount;
                break;
            case (IllustrationViewerAutoPlayScope.AllWorks, IllustrationViewerAutoPlayMode.Sequential):
                if (NextAction is PagedBehavior.None)
                    IsAutoPlaying = false;
                else
                    NextCommand.Execute(null);
                break;
            case (IllustrationViewerAutoPlayScope.AllWorks, IllustrationViewerAutoPlayMode.Loop):
                if (NextAction is PagedBehavior.None)
                    CurrentWorkIndex = 0;
                else
                    NextCommand.Execute(null);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static void SaveAutoPlaySettings() => AppInfo.SaveSettings(App.AppViewModel.AppSettings);

    #endregion

    #region Dispose

    private bool _disposed;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (_disposed)
            return;

        _disposed = true;
        _loadingCts.Cancel();
        _loadingCts.Dispose();
        IsAutoPlaying = false;
        CurrentImage = null!;
        _sourceView?.Dispose();
    }

    ~IllustrationViewerPageViewModel() => Dispose();

    #endregion
}
