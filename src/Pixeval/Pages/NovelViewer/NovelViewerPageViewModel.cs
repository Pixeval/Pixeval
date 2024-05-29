#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/NovelViewerPageViewModel.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

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
    private bool _isFullScreen;

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
        FullScreenCommand.GetFullScreenCommand(false);
    }

    public void OnFrameworkElementOnActualThemeChanged(FrameworkElement frameworkElement, object o)
    {
        NovelBackgroundEntry.ValueReset();
        NovelFontColorEntry.ValueReset();
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

    private int _pageCount;

    private int _currentPageIndex = -1;

    /// <summary>
    /// setter只用于绑定反向更新
    /// </summary>
    public int PageCount
    {
        get => _pageCount;
        set
        {
            _pageCount = value;
            OnButtonPropertiesChanged();
        }
    }

    /// <summary>
    /// setter只用于绑定反向更新
    /// </summary>
    public int CurrentPageIndex
    {
        get => _currentPageIndex;
        set
        {
            _currentPageIndex = value;
            OnButtonPropertiesChanged();
        }
    }

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
        get => _currentNovelIndex;
        set
        {
            if (value is -1)
                return;
            if (value == _currentNovelIndex)
                return;

            var oldValue = _currentNovelIndex;
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract

            _currentNovelIndex = value;
            CurrentPageIndex = 0;

            NovelInfoTag.Parameter = CurrentNovel.Entry;
            CommentsTag.Parameter = (SimpleWorkType.Novel, NovelId);

            OnDetailedPropertyChanged(oldValue, value);
            OnPropertyChanged(nameof(CurrentNovel));
            OnPropertyChanged(nameof(NovelId));
        }
    }

    /// <inheritdoc cref="CurrentNovelIndex"/>
    private int _currentNovelIndex = -1;

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
        FullScreenCommand.GetFullScreenCommand(IsFullScreen);
    }

    public XamlUICommand NovelSettingsCommand { get; } =
        EntryViewerPageResources.NovelSettings.GetCommand(Symbol.Settings);

    public XamlUICommand InfoAndCommentsCommand { get; } =
        EntryViewerPageResources.InfoAndComments.GetCommand(Symbol.Info, VirtualKey.F12);

    public XamlUICommand AddToBookmarkCommand { get; } = EntryItemResources.AddToBookmark.GetCommand(Symbol.Bookmark);

    public XamlUICommand FullScreenCommand { get; } = "".GetCommand(Symbol.ArrowMaximize);

    #endregion

    #region Settings

    public static AppSettings AppSettings => App.AppViewModel.AppSettings;

    public FontAppSettingsEntry NovelFontFamilyEntry { get; } = new(AppSettings, t => t.NovelFontFamily) { ValueChanged = ValueChanged };

    public ColorAppSettingsEntry NovelBackgroundEntry { get; } = new(AppSettings, t => t.NovelBackground) { ValueChanged = ValueChanged };

    public ColorAppSettingsEntry NovelFontColorEntry { get; } = new(AppSettings, t => t.NovelFontColor) { ValueChanged = ValueChanged };

    public EnumAppSettingsEntry<FontWeightsOption> NovelFontWeightEntry { get; } = new(AppSettings, t => t.NovelFontWeight) { ValueChanged = ValueChanged };

    public IntAppSettingsEntry NovelFontSizeEntry { get; } = new(AppSettings, t => t.NovelFontSize)
    {
        ValueChanged = ValueChanged,
        Max = 100,
        Min = 5
    };

    public IntAppSettingsEntry NovelLineHeightEntry { get; } = new(AppSettings, t => t.NovelLineHeight)
    {
        ValueChanged = ValueChanged,
        Max = 150,
        Min = 0
    };

    public IntAppSettingsEntry NovelMaxWidthEntry { get; } = new(AppSettings, t => t.NovelMaxWidth)
    {
        ValueChanged = ValueChanged,
        LargeChange = 100,
        SmallChange = 50,
        Max = 10000,
        Min = 50
    };

    public ISettingsEntry[] Entries =>
    [
        NovelFontFamilyEntry,
        NovelBackgroundEntry,
        NovelFontColorEntry,
        NovelFontWeightEntry,
        NovelFontSizeEntry,
        NovelLineHeightEntry,
        NovelMaxWidthEntry
    ];

    private static void ValueChanged<T>(T value) => AppInfo.SaveConfig(App.AppViewModel.AppSettings);

    #endregion
}
