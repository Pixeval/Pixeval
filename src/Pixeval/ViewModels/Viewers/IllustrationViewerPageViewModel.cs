// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Utilities;
using Pixeval.Views.Viewers;

namespace Pixeval.ViewModels.Viewers;

public sealed partial class IllustrationViewerPageViewModel : PagedViewerViewModel, IDisposable
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="illustrationViewModels"></param>
    /// <param name="currentIllustrationIndex"></param>
    public IllustrationViewerPageViewModel(IEnumerable<IllustrationItemViewModel> illustrationViewModels, int currentIllustrationIndex)
    {
        IllustrationsSource = [.. illustrationViewModels];
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
        ViewModelSource.DataProvider.View.FilterChanged += (_, _) => CurrentWorkIndex = Illustrations.IndexOf(CurrentIllustration);
        CurrentWorkIndex = currentIllustrationIndex;
    }

    private IllustrationViewViewModel? ViewModelSource { get; }

    public IllustrationItemViewModel[]? IllustrationsSource { get; }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Pages = null!;
        Images = null!;
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
            // 这里可以触发总页数的更新
            Pages = [.. CurrentIllustration.GetMangaIllustrationViewModels()];
            // 保证_pages里所有的IllustrationViewModel都是生成的，从而删除的时候一律DisposeForce
            Images = [.. Pages.Select(p => new ImageViewerPageViewModel(p, CurrentIllustration))];

            
            // TODO: I would suggest use ViewLocator here, to keep the ViewModel separated from the View.
            // new WorkInfoPage(CurrentIllustration.Entry)
            var list = new List<Page>(2) {  };
            if (CurrentIllustration.Entry is Illustration { Id: var id })
            {
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
        get;
        set
        {
            // 不检查值是否变化，强制触发更新事件
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CurrentPage));
            OnPropertyChanged(nameof(CurrentImage));
            OnPropertyChanged(nameof(PrevButtonText));
            OnPropertyChanged(nameof(NextButtonText));
            PrevCommand.NotifyCanExecuteChanged();
            NextCommand.NotifyCanExecuteChanged();
            PrevWorkCommand.NotifyCanExecuteChanged();
            NextWorkCommand.NotifyCanExecuteChanged();
        }
    }

    public override int PageCount => Pages.Length;

    public override int WorkCount => Illustrations.Count;

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
}
