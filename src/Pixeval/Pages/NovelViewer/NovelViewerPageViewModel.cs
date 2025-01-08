// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Linq;
using Windows.System;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentIcons.Common;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Pixeval.Controls;
using Pixeval.Controls.Windowing;
using Pixeval.CoreApi.Model;
using Pixeval.Options;
using Pixeval.Util.ComponentModels;
using Pixeval.Util.UI;
using Pixeval.AppManagement;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.Settings;
using Pixeval.Settings.Models;

namespace Pixeval.Pages.NovelViewer;

public partial class NovelViewerPageViewModel : DetailedUiObservableObject, IDisposable
{
    [ObservableProperty]
    public partial bool IsFullScreen { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="novelViewModels"></param>
    /// <param name="currentNovelIndex"></param>
    /// <param name="hWnd"></param>
    public NovelViewerPageViewModel(IEnumerable<NovelItemViewModel> novelViewModels, int currentNovelIndex, ulong hWnd) : base(hWnd)
    {
        NovelsSource = novelViewModels.ToArray();
        CurrentNovelIndex = currentNovelIndex;

        InitializeCommands();
        FullScreenCommand.RefreshFullScreenCommand(false);
    }

    public void OnFrameworkElementOnActualThemeChanged(FrameworkElement frameworkElement, object o)
    {
        NovelBackgroundEntry.ValueReset(Settings);
        NovelFontColorEntry.ValueReset(Settings);
    }

    /// <summary>
    /// 当拥有DataProvider的时候调用这个构造函数，dispose的时候会自动dispose掉DataProvider
    /// </summary>
    /// <param name="viewModel"></param>
    /// <param name="currentNovelIndex"></param>
    /// <param name="hWnd"></param>
    public NovelViewerPageViewModel(NovelViewViewModel viewModel, int currentNovelIndex, ulong hWnd) : base(hWnd)
    {
        ViewModelSource = new NovelViewViewModel(viewModel);
        ViewModelSource.DataProvider.View.FilterChanged += (_, _) => CurrentNovelIndex = Novels.IndexOf(CurrentNovel);
        CurrentNovelIndex = currentNovelIndex;

        InitializeCommands();
    }

    private NovelViewViewModel? ViewModelSource { get; }

    public NovelItemViewModel[]? NovelsSource { get; }

    public long NovelId => CurrentNovel.Entry.Id;

    public UserInfo Illustrator => CurrentNovel.Entry.User;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        foreach (var novelViewModel in Novels)
            novelViewModel.UnloadThumbnail(this);
        ViewModelSource?.Dispose();
    }

    public NavigationViewTag[] Tags =>
    [
        NovelInfoTag,
        CommentsTag
    ];

    public NavigationViewTag<WorkInfoPage, Novel> NovelInfoTag { get; } =
        new(null!) { Content = EntryViewerPageResources.InfoTabContent };

    public NavigationViewTag<CommentsPage, (SimpleWorkType, long Id)> CommentsTag { get; } =
        new(default) { Content = EntryViewerPageResources.CommentsTabContent };

    #region Current相关

    /// <summary>
    /// setter只用于绑定反向更新
    /// </summary>
    public int PageCount
    {
        get;
        set
        {
            field = value;
            OnButtonPropertiesChanged();
        }
    }

    /// <summary>
    /// setter只用于绑定反向更新
    /// </summary>
    public int CurrentPageIndex
    {
        get;
        set
        {
            field = value;
            OnButtonPropertiesChanged();
        }
    } = -1;

    /// <summary>
    /// 插画列表
    /// </summary>
    public IList<NovelItemViewModel> Novels => ViewModelSource?.DataProvider.View ?? (IList<NovelItemViewModel>)NovelsSource!;

    /// <summary>
    /// 当前插画
    /// </summary>
    public NovelItemViewModel CurrentNovel => Novels[CurrentNovelIndex];

    /// <summary>
    /// 当前插画的索引
    /// </summary>
    public int CurrentNovelIndex
    {
        get;
        set
        {
            if (value is -1)
                return;
            if (value == field)
                return;

            var oldValue = field;
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract

            field = value;
            CurrentPageIndex = 0;

            NovelInfoTag.Parameter = CurrentNovel.Entry;
            CommentsTag.Parameter = (SimpleWorkType.Novel, NovelId);

            OnDetailedPropertyChanged(oldValue, value);
            OnPropertyChanged(nameof(CurrentNovel));
            OnPropertyChanged(nameof(NovelId));
        }
    } = -1;

    private void OnButtonPropertiesChanged()
    {
        OnPropertyChanged(nameof(NextButtonText));
        OnPropertyChanged(nameof(PrevButtonText));
    }

    #endregion

    #region Helper Functions

    public string? NextButtonText => NextButtonAction switch
    {
        true => EntryViewerPageResources.NextPageOrNovel,
        false => EntryViewerPageResources.NextNovel,
        _ => null
    };

    /// <summary>
    /// <see langword="true"/>: next page<br/>
    /// <see langword="false"/>: next novel<br/>
    /// <see langword="null"/>: none
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:转换为条件表达式")]
    public bool? NextButtonAction
    {
        get
        {
            if (CurrentPageIndex < PageCount - 1)
                return true;

            if (CurrentNovelIndex < Novels.Count - 1)
                return false;

            return null;
        }
    }

    public string? PrevButtonText => PrevButtonAction switch
    {
        true => EntryViewerPageResources.PrevPageOrNovel,
        false => EntryViewerPageResources.PrevNovel,
        _ => null
    };

    /// <summary>
    /// <see langword="true"/>: prev page<br/>
    /// <see langword="false"/>: prev novel<br/>
    /// <see langword="null"/>: none
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:转换为条件表达式")]
    public bool? PrevButtonAction
    {
        get
        {
            if (CurrentPageIndex > 0)
                return true;

            if (CurrentNovelIndex > 0)
                return false;

            return null;
        }
    }

    #endregion

    #region Commands

    private void InitializeCommands()
    {
        FullScreenCommand.ExecuteRequested += FullScreenCommandOnExecuteRequested;
    }

    private void FullScreenCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        IsFullScreen = !IsFullScreen;
        FullScreenCommand.RefreshFullScreenCommand(IsFullScreen);
    }

    public XamlUICommand NovelSettingsCommand { get; } =
        EntryViewerPageResources.NovelSettings.GetCommand(Symbol.Settings);

    public XamlUICommand InfoAndCommentsCommand { get; } =
        EntryViewerPageResources.InfoAndComments.GetCommand(Symbol.Info, VirtualKey.F12);

    public XamlUICommand AddToBookmarkCommand { get; } = EntryItemResources.AddToBookmark.GetCommand(Symbol.Bookmark);

    public XamlUICommand FullScreenCommand { get; } = "".GetCommand(Symbol.ArrowMaximize);

    #endregion

    #region Settings

    public static AppSettings Settings => App.AppViewModel.AppSettings;

    public FontAppSettingsEntry NovelFontFamilyEntry { get; } = new(Settings, t => t.NovelFontFamily);

    public ColorAppSettingsEntry NovelBackgroundEntry { get; } = new(Settings, t => t.NovelBackground);

    public ColorAppSettingsEntry NovelFontColorEntry { get; } = new(Settings, t => t.NovelFontColor);

    public EnumAppSettingsEntry NovelFontWeightEntry { get; } = new(Settings, t => t.NovelFontWeight, FontWeightsOptionExtension.GetItems());

    public IntAppSettingsEntry NovelFontSizeEntry { get; } = new(Settings, t => t.NovelFontSize) { Max = 100, Min = 5 };

    public IntAppSettingsEntry NovelLineHeightEntry { get; } = new(Settings, t => t.NovelLineHeight) { Max = 150, Min = 0 };

    public IntAppSettingsEntry NovelMaxWidthEntry { get; } = new(Settings, t => t.NovelMaxWidth)
    {
        LargeChange = 100,
        SmallChange = 50,
        Max = 10000,
        Min = 50
    };

    public IAppSettingEntry<AppSettings>[] Entries =>
    [
        NovelFontFamilyEntry,
        NovelBackgroundEntry,
        NovelFontColorEntry,
        NovelFontWeightEntry,
        NovelFontSizeEntry,
        NovelLineHeightEntry,
        NovelMaxWidthEntry
    ];

    #endregion
}
