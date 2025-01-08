// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Linq;
using Windows.System;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentIcons.Common;
using Microsoft.UI.Xaml.Input;
using Pixeval.Controls;
using Pixeval.Controls.Windowing;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using Pixeval.Util.ComponentModels;

namespace Pixeval.Pages.IllustrationViewer;

public partial class IllustrationViewerPageViewModel : DetailedUiObservableObject, IDisposable
{
    [ObservableProperty]
    public partial bool IsFullScreen { get; set; }

    [ObservableProperty]
    public partial bool ShowPixevalIcon { get; set; } = true;

    [ObservableProperty]
    public partial string AdditionalText { get; set; } = "";

    [ObservableProperty]
    public partial bool AdditionalTextBlockVisible { get; set; } = true;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="illustrationViewModels"></param>
    /// <param name="currentIllustrationIndex"></param>
    /// <param name="hWnd"></param>
    public IllustrationViewerPageViewModel(IEnumerable<IllustrationItemViewModel> illustrationViewModels, int currentIllustrationIndex, ulong hWnd) : base(hWnd)
    {
        IllustrationsSource = illustrationViewModels.ToArray();
        CurrentIllustrationIndex = currentIllustrationIndex;

        InitializeCommands();
        FullScreenCommand.RefreshFullScreenCommand(false);
    }

    /// <summary>
    /// 当拥有DataProvider的时候调用这个构造函数，dispose的时候会自动dispose掉DataProvider
    /// </summary>
    /// <param name="viewModel"></param>
    /// <param name="currentIllustrationIndex"></param>
    /// <param name="hWnd"></param>
    /// <remarks>
    /// illustrations should contain only one item if the illustration is a single
    /// otherwise it contains the entire manga data
    /// </remarks>
    public IllustrationViewerPageViewModel(IllustrationViewViewModel viewModel, int currentIllustrationIndex, ulong hWnd) : base(hWnd)
    {
        ViewModelSource = new IllustrationViewViewModel(viewModel);
        ViewModelSource.DataProvider.View.FilterChanged += (_, _) => CurrentIllustrationIndex = Illustrations.IndexOf(CurrentIllustration);
        CurrentIllustrationIndex = currentIllustrationIndex;

        InitializeCommands();
    }

    private IllustrationViewViewModel? ViewModelSource { get; }

    public IllustrationItemViewModel[]? IllustrationsSource { get; }

    public long IllustrationId => CurrentIllustration.Entry.Id;

    public UserInfo Illustrator => CurrentIllustration.Entry.User;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        foreach (var illustrationViewModel in Illustrations)
            illustrationViewModel.UnloadThumbnail(this);
        Pages = null!;
        Images = null!;
        ViewModelSource?.Dispose();
    }

    public NavigationViewTag[] Tags =>
    [
        IllustrationInfoTag,
        CommentsTag,
        RelatedWorksTag
    ];

    public NavigationViewTag<WorkInfoPage, Illustration> IllustrationInfoTag { get; } =
        new(null!) { Content = EntryViewerPageResources.InfoTabContent };

    public NavigationViewTag<CommentsPage, (SimpleWorkType, long Id)> CommentsTag { get; } =
        new(default) { Content = EntryViewerPageResources.CommentsTabContent };

    public NavigationViewTag<RelatedWorksPage, long> RelatedWorksTag { get; } =
        new(0) { Content = EntryViewerPageResources.RelatedWorksTabContent };

    #region Current相关

    /// <summary>
    /// 当前插画
    /// </summary>
    public IllustrationItemViewModel CurrentIllustration => Illustrations[CurrentIllustrationIndex];

    /// <summary>
    /// 当前插画的页面
    /// </summary>
    public IllustrationItemViewModel CurrentPage => Pages[CurrentPageIndex];

    /// <summary>
    /// 当前图片的ViewModel
    /// </summary>
    public ImageViewerPageViewModel CurrentImage => Images[CurrentPageIndex];

    /// <summary>
    /// 当前插画的索引
    /// </summary>
    public int CurrentIllustrationIndex
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
            var oldTag = Pages?[CurrentPageIndex].Id ?? 0;

            field = value;
            // 这里可以触发总页数的更新
            Pages = CurrentIllustration.GetMangaIllustrationViewModels().ToArray();
            // 保证_pages里所有的IllustrationViewModel都是生成的，从而删除的时候一律DisposeForce
            Images = Pages.Select(p => new ImageViewerPageViewModel(p, CurrentIllustration, HWnd)).ToArray();

            IllustrationInfoTag.Parameter = CurrentIllustration.Entry;
            CommentsTag.Parameter = (SimpleWorkType.IllustAndManga, IllustrationId);
            RelatedWorksTag.Parameter = IllustrationId;

            // 此处不要触发CurrentPageIndex的OnDetailedPropertyChanged，否则会导航两次
            _currentPageIndex = 0;
            OnButtonPropertiesChanged();
            // 用OnPropertyChanged不会触发导航，但可以让UI页码更新
            OnPropertyChanged(nameof(CurrentPageIndex));
            OnPropertyChanged(nameof(CurrentPage));
            OnPropertyChanged(nameof(CurrentImage));

            OnDetailedPropertyChanged(oldValue, value, oldTag, CurrentPage.Id);
            OnPropertyChanged(nameof(CurrentIllustration));
        }
    } = -1;

    /// <summary>
    /// 当前插画的页面索引
    /// </summary>
    public int CurrentPageIndex
    {
        get => _currentPageIndex;
        set
        {
            if (value == _currentPageIndex)
                return;
            var oldValue = _currentPageIndex;
            _currentPageIndex = value;
            OnButtonPropertiesChanged();
            OnPropertyChanged(nameof(CurrentPage));
            OnPropertyChanged(nameof(CurrentImage));
            OnDetailedPropertyChanged(oldValue, value);
        }
    }

    private void OnButtonPropertiesChanged()
    {
        OnPropertyChanged(nameof(NextButtonText));
        OnPropertyChanged(nameof(PrevButtonText));
    }

    public int PageCount => Pages.Length;

    /// <inheritdoc cref="CurrentPageIndex"/>
    private int _currentPageIndex = -1;

    /// <summary>
    /// 插画列表
    /// </summary>
    public IList<IllustrationItemViewModel> Illustrations => ViewModelSource?.DataProvider.View ?? (IList<IllustrationItemViewModel>)IllustrationsSource!;

    /// <summary>
    /// 一个插画所有的页面
    /// </summary>
    public IllustrationItemViewModel[] Pages
    {
        get;
        set
        {
            if (field == value)
                return;
            field?.ForEach(i => i.Dispose());
            field = value;
            if (field != null!)
                OnPropertyChanged(nameof(PageCount));
        }
    } = null!;

    public ImageViewerPageViewModel[] Images
    {
        get;
        set
        {
            if (field == value)
                return;
            field?.ForEach(i => i.Dispose());
            field = value;
        }
    } = null!;

    #endregion

    #region Helper Functions

    public string? NextButtonText => NextButtonAction switch
    {
        true => EntryViewerPageResources.NextPageOrIllustration,
        false => EntryViewerPageResources.NextIllustration,
        _ => null
    };

    /// <summary>
    /// <see langword="true"/>: next page<br/>
    /// <see langword="false"/>: next illustration<br/>
    /// <see langword="null"/>: none
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:转换为条件表达式")]
    public bool? NextButtonAction
    {
        get
        {
            if (CurrentPageIndex < PageCount - 1)
                return true;

            if (CurrentIllustrationIndex < Illustrations.Count - 1)
                return false;

            return null;
        }
    }

    public string? PrevButtonText => PrevButtonAction switch
    {
        true => EntryViewerPageResources.PrevPageOrIllustration,
        false => EntryViewerPageResources.PrevIllustration,
        _ => null
    };

    /// <summary>
    /// <see langword="true"/>: prev page<br/>
    /// <see langword="false"/>: prev illustration<br/>
    /// <see langword="null"/>: none
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:转换为条件表达式")]
    public bool? PrevButtonAction
    {
        get
        {
            if (CurrentPageIndex > 0)
                return true;

            if (CurrentIllustrationIndex > 0)
                return false;

            return null;
        }
    }

    #endregion

    #region Commands

    private void InitializeCommands()
    {
        FullScreenCommand.ExecuteRequested += FullScreenCommandOnExecuteRequested;
        FullScreenCommand.RefreshFullScreenCommand(false);
    }

    private void FullScreenCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        IsFullScreen = !IsFullScreen;
        FullScreenCommand.RefreshFullScreenCommand(IsFullScreen);
    }

    public XamlUICommand InfoAndCommentsCommand { get; } =
        EntryViewerPageResources.InfoAndComments.GetCommand(Symbol.Info, VirtualKey.F12);

    public XamlUICommand AddToBookmarkCommand { get; } = EntryItemResources.AddToBookmark.GetCommand(Symbol.Bookmark);

    public XamlUICommand FullScreenCommand { get; } = "".GetCommand(Symbol.ArrowMaximize);

    #endregion
}
