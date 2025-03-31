// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.Controls.Windowing;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Options;
using Pixeval.Settings;
using Pixeval.Settings.Models;
using Pixeval.Util.ComponentModels;
using Pixeval.Util.UI;
using Windows.System;
using Symbol = FluentIcons.Common.Symbol;

namespace Pixeval.Pages.NovelViewer;

public partial class NovelViewerPageViewModel : DetailedUiObservableObject, IDisposable
{
    [ObservableProperty]
    public partial bool IsBottomListOpen { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="novelViewModels"></param>
    /// <param name="currentNovelIndex"></param>
    /// <param name="page"></param>
    public NovelViewerPageViewModel(IEnumerable<NovelItemViewModel> novelViewModels, int currentNovelIndex, NovelViewerPage page) : base(page)
    {
        NovelsSource = [.. novelViewModels];
        CurrentNovelIndex = currentNovelIndex;
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
    /// <param name="page"></param>
    public NovelViewerPageViewModel(NovelViewViewModel viewModel, int currentNovelIndex, NovelViewerPage page) : base(page)
    {
        ViewModelSource = new NovelViewViewModel(viewModel);
        ViewModelSource.DataProvider.View.FilterChanged += (_, _) => CurrentNovelIndex = Novels.IndexOf(CurrentNovel);
        CurrentNovelIndex = currentNovelIndex;
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

    public NavigationViewTag<WorkInfoPage, Novel> NovelInfoTag { get; } =
        new(EntryViewerPageResources.InfoTabContent, null!);

    public NavigationViewTag<CommentsPage, (SimpleWorkType, long Id)> CommentsTag { get; } =
        new(EntryViewerPageResources.CommentsTabContent, default);

    public IReadOnlyList<NavigationViewTag> Tags =>
    [
        NovelInfoTag,
        CommentsTag
    ];

    #region Current相关

    public event PropertyChangedEventHandler? CurrentDocumentPropertyChanged;

    [ObservableProperty]
    public partial DocumentViewerViewModel CurrentDocument { get; private set; } = null!;

    /// <summary>
    /// 小说列表
    /// </summary>
    public IList<NovelItemViewModel> Novels => ViewModelSource?.DataProvider.View ?? (IList<NovelItemViewModel>) NovelsSource!;

    /// <summary>
    /// 当前小说
    /// </summary>
    public NovelItemViewModel CurrentNovel => Novels[CurrentNovelIndex];

    /// <summary>
    /// 当前小说的索引
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

            field = value;

            NovelInfoTag.Parameter = CurrentNovel.Entry;
            CommentsTag.Parameter = (SimpleWorkType.Novel, NovelId);

            if (CurrentDocument != null!)
                CurrentDocument.PropertyChanged -= OnCurrentDocumentOnPropertyChanged;
            CurrentDocument = new DocumentViewerViewModel(FrameworkElement);
            _ = CurrentDocument.LoadAsync(CurrentNovel);
            CurrentDocument.PropertyChanged += OnCurrentDocumentOnPropertyChanged;

            OnDetailedPropertyChanged(oldValue, value);
            OnPropertyChanged(nameof(CurrentNovel));
            OnPropertyChanged(nameof(NovelId));

            return;

            void OnCurrentDocumentOnPropertyChanged(object? o, PropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case nameof(DocumentViewerViewModel.CurrentPage):
                        OnButtonPropertiesChanged();
                        break;
                }

                CurrentDocumentPropertyChanged?.Invoke(o, e);
            }
        }
        // 第一次赋值属性时会判断 value == field，如果是0则无法进入get方法体
        // ReSharper disable once MemberInitializerValueIgnored
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
            if (CurrentDocument.CurrentPage < CurrentDocument.PageCount - 1)
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
            if (CurrentDocument.CurrentPage > 0)
                return true;

            if (CurrentNovelIndex > 0)
                return false;

            return null;
        }
    }

    #endregion

    #region Commands

    partial void OnIsBottomListOpenChanged(bool value) => BottomListCommand.RefreshBottomPanelCommand(value);

    public XamlUICommand NovelSettingsCommand { get; } =
        EntryViewerPageResources.NovelSettings.GetCommand(Symbol.Settings);

    public XamlUICommand InfoAndCommentsCommand { get; } =
        EntryViewerPageResources.InfoAndComments.GetCommand(Symbol.Info, VirtualKey.F12);

    public XamlUICommand BottomListCommand { get; } = MiscResources.OpenBottomList.GetCommand(Symbol.PanelBottomExpand, VirtualKey.F);

    public XamlUICommand AddToBookmarkCommand { get; } = EntryItemResources.AddToBookmark.GetCommand(Symbol.Bookmark);

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
