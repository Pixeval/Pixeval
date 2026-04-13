// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.I18N;
using Pixeval.Utilities;
using Pixeval.Views.Viewers;

namespace Pixeval.ViewModels.Viewers;

public partial class IllustrationViewerPageViewModel : ViewModelBase, IDisposable
{
    [ObservableProperty]
    public partial bool IsBottomListOpen { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="illustrationViewModels"></param>
    /// <param name="currentIllustrationIndex"></param>
    public IllustrationViewerPageViewModel(IEnumerable<IllustrationItemViewModel> illustrationViewModels, int currentIllustrationIndex)
    {
        IllustrationsSource = [.. illustrationViewModels];
        CurrentIllustrationIndex = currentIllustrationIndex;
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
        ViewModelSource.DataProvider.View.FilterChanged += (_, _) => CurrentIllustrationIndex = Illustrations.IndexOf(CurrentIllustration);
        CurrentIllustrationIndex = currentIllustrationIndex;
    }

    private IllustrationViewViewModel? ViewModelSource { get; }

    public IllustrationItemViewModel[]? IllustrationsSource { get; }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        foreach (var illustrationViewModel in Illustrations)
            illustrationViewModel.UnloadThumbnail(this);
        Pages = null!;
        Images = null!;
        ViewModelSource?.Dispose();
    }

    [ObservableProperty]
    public partial IReadOnlyList<ContentPage>? PanePages { get; private set; }

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

            field = value;
            // 这里可以触发总页数的更新
            Pages = [.. CurrentIllustration.GetMangaIllustrationViewModels()];
            // 保证_pages里所有的IllustrationViewModel都是生成的，从而删除的时候一律DisposeForce
            Images = [.. Pages.Select(p => new ImageViewerPageViewModel(p, CurrentIllustration))];

            var list = new List<ContentPage>(3) { new WorkInfoPage { DataContext = CurrentIllustration.Entry } };
            if (CurrentIllustration.Entry is Illustration { Id: var id })
            {
                var engine = App.AppViewModel.MakoClient.IllustrationComments(id);
                list.Add(new CommentsPage
                {
                    DataContext = new CommentsViewViewModel(engine, SimpleWorkType.IllustrationAndManga, id)
                });
                list.Add(new RelatedWorksPage { IllustrationId = id });
            }

            PanePages = list;

            // 此处不要触发CurrentPageIndex的OnDetailedPropertyChanged，否则会导航两次
            _currentPageIndex = 0;
            OnButtonPropertiesChanged();
            // 用OnPropertyChanged不会触发导航，但可以让UI页码更新
            OnPropertyChanged(nameof(CurrentPageIndex));
            OnPropertyChanged(nameof(CurrentPage));
            OnPropertyChanged(nameof(CurrentImage));

            OnPropertyChanged();
            OnPropertyChanged(nameof(CurrentIllustration));
        }
        // 第一次赋值属性时会判断 value == field，如果是0则无法进入get方法体
        // ReSharper disable once MemberInitializerValueIgnored
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
            OnPropertyChanged();
        }
    }

    private void OnButtonPropertiesChanged()
    {
        OnPropertyChanged(nameof(NextButtonText));
        OnPropertyChanged(nameof(PrevButtonText));
        NextCommand.NotifyCanExecuteChanged();
        PrevCommand.NotifyCanExecuteChanged();
        NextIllustrationCommand.NotifyCanExecuteChanged();
        PrevIllustrationCommand.NotifyCanExecuteChanged();
    }

    public int PageCount => Pages.Length;

    /// <inheritdoc cref="CurrentPageIndex"/>
    private int _currentPageIndex = -1;

    /// <summary>
    /// 插画列表
    /// </summary>
    public IList<IllustrationItemViewModel> Illustrations => ViewModelSource?.DataProvider.View ?? (IList<IllustrationItemViewModel>) IllustrationsSource!;

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
        true => I18NManager.GetResource(EntryViewerPageResources.NextPageOrIllustration),
        false => I18NManager.GetResource(EntryViewerPageResources.NextIllustration),
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
        true => I18NManager.GetResource(EntryViewerPageResources.PrevPageOrIllustration),
        false => I18NManager.GetResource(EntryViewerPageResources.PrevIllustration),
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

    private bool CanNext() => NextButtonAction is not null;

    [RelayCommand(CanExecute = nameof(CanNext))]
    private void Next()
    {
        switch (NextButtonAction)
        {
            case true: CurrentPageIndex++; break;
            case false: CurrentIllustrationIndex++; break;
        }
    }

    private bool CanPrev() => PrevButtonAction is not null;

    [RelayCommand(CanExecute = nameof(CanPrev))]
    private void Prev()
    {
        switch (PrevButtonAction)
        {
            case true: CurrentPageIndex--; break;
            case false: CurrentIllustrationIndex--; break;
        }
    }

    private bool CanNextIllustration() => CurrentIllustrationIndex < Illustrations.Count - 1;

    [RelayCommand(CanExecute = nameof(CanNextIllustration))]
    private void NextIllustration() => CurrentIllustrationIndex++;

    private bool CanPrevIllustration() => CurrentIllustrationIndex > 0;

    [RelayCommand(CanExecute = nameof(CanPrevIllustration))]
    private void PrevIllustration() => CurrentIllustrationIndex--;

    #endregion
}
