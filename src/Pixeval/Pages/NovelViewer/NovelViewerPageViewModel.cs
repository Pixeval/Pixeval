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
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Pixeval.Controls;
using Pixeval.Controls.Windowing;
using Pixeval.CoreApi.Model;
using Pixeval.Options;
using Pixeval.Pages.Misc;
using Pixeval.Util.ComponentModels;
using Pixeval.Util.UI;
using Pixeval.AppManagement;
using System.Runtime.CompilerServices;
using WinUI3Utilities.Controls;

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
    /// <param name="content"></param>
    public NovelViewerPageViewModel(IEnumerable<NovelItemViewModel> novelViewModels, int currentNovelIndex, FrameworkElement content) : base(content)
    {
        content.ActualThemeChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(NovelBackground));
            OnPropertyChanged(nameof(NovelFontColor));
        };
        NovelsSource = novelViewModels.ToArray();
        CurrentNovelIndex = currentNovelIndex;

        InitializeCommands();
        FullScreenCommand.GetFullScreenCommand(false);
    }

    /// <summary>
    /// 当拥有DataProvider的时候调用这个构造函数，dispose的时候会自动dispose掉DataProvider
    /// </summary>
    /// <param name="viewModel"></param>
    /// <param name="currentNovelIndex"></param>
    /// <param name="content"></param>
    /// <remarks>
    /// novels should contain only one item if the novel is a single
    /// otherwise it contains the entire manga data
    /// </remarks>
    public NovelViewerPageViewModel(NovelViewViewModel viewModel, int currentNovelIndex, FrameworkElement content) : base(content)
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

    public NavigationViewTag<CommentsPage, (CommentType, long Id)> CommentsTag { get; } =
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
            CommentsTag.Parameter = (CommentType.Novel, NovelId);

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
        EntryViewerPageResources.NovelSettings.GetCommand(IconGlyph.SettingsE713);

    public XamlUICommand InfoAndCommentsCommand { get; } =
        EntryViewerPageResources.InfoAndComments.GetCommand(IconGlyph.InfoE946, VirtualKey.F12);

    public XamlUICommand AddToBookmarkCommand { get; } = EntryItemResources.AddToBookmark.GetCommand(IconGlyph.BookmarksE8A4);

    public XamlUICommand FullScreenCommand { get; } = "".GetCommand(IconGlyph.FullScreenE740);

    #endregion

    #region Settings

    public static IEnumerable<string> AvailableFonts => SettingsPageViewModel.AvailableFonts;

    public uint NovelBackground
    {
        get => FrameworkElement.ActualTheme is ElementTheme.Light ? App.AppViewModel.AppSettings.NovelBackgroundInLightMode : App.AppViewModel.AppSettings.NovelBackgroundInDarkMode;
        set => _ = FrameworkElement.ActualTheme is ElementTheme.Light
            ? SetSettings(App.AppViewModel.AppSettings.NovelBackgroundInLightMode, value, App.AppViewModel.AppSettings, (setting, value) => setting.NovelBackgroundInLightMode = value)
            : SetSettings(App.AppViewModel.AppSettings.NovelBackgroundInDarkMode, value, App.AppViewModel.AppSettings, (setting, value) => setting.NovelBackgroundInDarkMode = value);
    }

    public uint NovelFontColor
    {
        get => FrameworkElement.ActualTheme is ElementTheme.Light ? App.AppViewModel.AppSettings.NovelFontColorInLightMode : App.AppViewModel.AppSettings.NovelFontColorInDarkMode;
        set => _ = FrameworkElement.ActualTheme is ElementTheme.Light
            ? SetSettings(App.AppViewModel.AppSettings.NovelFontColorInLightMode, value, App.AppViewModel.AppSettings, (setting, value) => setting.NovelFontColorInLightMode = value)
            : SetSettings(App.AppViewModel.AppSettings.NovelFontColorInDarkMode, value, App.AppViewModel.AppSettings, (setting, value) => setting.NovelFontColorInDarkMode = value);
    }

    public FontWeightsOption NovelFontWeight
    {
        get => App.AppViewModel.AppSettings.NovelFontWeight;
        set => SetSettings(App.AppViewModel.AppSettings.NovelFontWeight, value, App.AppViewModel.AppSettings, (setting, value) => setting.NovelFontWeight = value);
    }

    public double NovelFontSize
    {
        get => App.AppViewModel.AppSettings.NovelFontSize;
        set => SetSettings(App.AppViewModel.AppSettings.NovelFontSize, value, App.AppViewModel.AppSettings, (setting, value) => setting.NovelFontSize = value);
    }

    public double NovelLineHeight
    {
        get => App.AppViewModel.AppSettings.NovelLineHeight;
        set => SetSettings(App.AppViewModel.AppSettings.NovelLineHeight, value, App.AppViewModel.AppSettings, (setting, value) => setting.NovelLineHeight = value);
    }

    public double NovelMaxWidth
    {
        get => App.AppViewModel.AppSettings.NovelMaxWidth;
        set => SetSettings(App.AppViewModel.AppSettings.NovelMaxWidth, value, App.AppViewModel.AppSettings, (setting, value) => setting.NovelMaxWidth = value);
    }

    public string NovelFontFamily
    {
        get => App.AppViewModel.AppSettings.NovelFontFamily;
        set => SetSettings(App.AppViewModel.AppSettings.NovelFontFamily, value, App.AppViewModel.AppSettings, (setting, value) => setting.NovelFontFamily = value);
    }

    protected bool SetSettings<TModel, T>(T oldValue, T newValue, TModel model, Action<TModel, T> callback, [CallerMemberName] string? propertyName = null)
        where TModel : class
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(callback);

        if (EqualityComparer<T>.Default.Equals(oldValue, newValue))
            return false;

        OnPropertyChanging(propertyName);

        callback(model, newValue);

        OnPropertyChanged(propertyName);

        AppInfo.SaveConfig(App.AppViewModel.AppSettings);

        return true;
    }

    #endregion
}
