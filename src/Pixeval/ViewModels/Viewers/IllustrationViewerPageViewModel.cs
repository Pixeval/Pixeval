// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.AppManagement;
using Pixeval.Models.Options;
using Pixeval.Views.Viewers;

namespace Pixeval.ViewModels.Viewers;

public sealed partial class IllustrationViewerPageViewModel : PagedViewerViewModel, IDisposable
{
    private readonly DispatcherTimer _autoPlayTimer = new();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="illustrationViewModels"></param>
    /// <param name="currentIllustrationIndex"></param>
    public IllustrationViewerPageViewModel(IEnumerable<IllustrationItemViewModel> illustrationViewModels, int currentIllustrationIndex)
    {
        IllustrationsSource = [.. illustrationViewModels];
        InitializeAutoPlayTimer();
        CurrentWorkIndex = currentIllustrationIndex;
    }

    /// <summary>
    /// 当拥有DataProvider的时候调用这个构造函数，dispose的时候会自动dispose掉DataProvider
    /// </summary>
    /// <param name="viewModel"></param>
    /// <param name="currentIllustrationIndex"></param>
    /// <remarks>
    /// illustrations should contain only one item if the illustration is a single
    /// otherwise it contains the entire manga data
    /// </remarks>
    public IllustrationViewerPageViewModel(IllustrationViewViewModel viewModel, int currentIllustrationIndex)
    {
        ViewModelSource = new IllustrationViewViewModel(viewModel);
        ViewModelSource.View.FilterChanged += (_, _) => CurrentWorkIndex = Illustrations.IndexOf(CurrentIllustration);
        InitializeAutoPlayTimer();
        CurrentWorkIndex = currentIllustrationIndex;
    }

    private IllustrationViewViewModel? ViewModelSource { get; }

    public IllustrationItemViewModel[]? IllustrationsSource { get; }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        IsAutoPlaying = false;
        CurrentImage = null!;
        ViewModelSource?.Dispose();
    }

    ~IllustrationViewerPageViewModel() => Dispose();

    [ObservableProperty]
    public partial IReadOnlyList<Page>? PanePages { get; private set; }

    #region Current相关

    /// <summary>
    /// 当前插画
    /// </summary>
    public IllustrationItemViewModel CurrentIllustration => Illustrations[CurrentWorkIndex];

    /// <summary>
    /// 当前图集的ViewModel
    /// </summary>
    public ImageViewerViewModel CurrentImage
    {
        get;
        private set
        {
            if (field == value)
                return;
            field?.PropertyChanged -= CurrentImageOnPropertyChanged;
            field?.Dispose();
            field = value;
            if (field == null!)
                return;
            field.PropertyChanged += CurrentImageOnPropertyChanged;
            OnPropertyChanged();
            NotifyPagingStateChanged();
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

            CurrentImage = new ImageViewerViewModel(CurrentIllustration);

            // TODO: I would suggest use ViewLocator here, to keep the ViewModel separated from the View.
            // new WorkInfoPage(CurrentIllustration.Entry)
            var list = new List<Page>(3);
            if (CurrentIllustration.Entry is Illustration { Id: var id } illustration)
            {
                var workInfoPage = new WorkInfoPage(illustration)
                {
                    ActionZone = new Border
                    {
                        Width = 32,
                        Height = 32,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Top,
                        IsHitTestVisible = false
                    }
                };

                list.Add(workInfoPage);  
                list.Add(new CommentsPage(new CommentsViewViewModel(SimpleWorkType.IllustrationAndManga, id)));
                list.Add(new RelatedWorksPage { IllustrationId = id });
            }

            PanePages = list;

            CurrentPageIndex = 0;

            OnPropertyChanged();
            OnPropertyChanged(nameof(CurrentIllustration));
        }
        // 第一次赋值属性时会判断 value == field，如果是0则无法进入set方法体
        // ReSharper disable once MemberInitializerValueIgnored
    } = -1;

    /// <summary>
    /// 当前插画的页面索引
    /// </summary>
    public override int CurrentPageIndex
    {
        get => CurrentImage.SelectedPageIndex;
        set
        {
            CurrentImage.SelectedPageIndex = value;
            // 不检查值是否变化，强制触发更新事件
            OnPropertyChanged();
            OnPropertyChanged(nameof(PrevButtonText));
            OnPropertyChanged(nameof(NextButtonText));
            PrevCommand.NotifyCanExecuteChanged();
            NextCommand.NotifyCanExecuteChanged();
            PrevWorkCommand.NotifyCanExecuteChanged();
            NextWorkCommand.NotifyCanExecuteChanged();
        }
    }

    private void CurrentImageOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ImageViewerViewModel.SelectedPageIndex))
            NotifyPagingStateChanged();
    }

    private void NotifyPagingStateChanged()
    {
        // 触发更新
        CurrentPageIndex = CurrentImage.SelectedPageIndex;
    }

    public override int PageCount => CurrentImage.PageCount;

    public override int WorkCount => Illustrations.Count;

    /// <summary>
    /// 插画列表
    /// </summary>
    public IList<IllustrationItemViewModel> Illustrations => ViewModelSource?.View ?? (IList<IllustrationItemViewModel>) IllustrationsSource!;

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
}
