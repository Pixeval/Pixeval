// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using Pixeval.Controls;
using Pixeval.Controls.Windowing;
using Pixeval.CoreApi.Model;
using WinUI3Utilities;
using Microsoft.UI.Xaml;

namespace Pixeval.Pages.NovelViewer;

public static class NovelViewerHelper
{
    /// <summary>
    /// 此方法无法加载更多小说，加载单个小说使用
    /// </summary>
    public static async Task CreateNovelPageAsync(this FrameworkElement frameworkElement, long id)
    {
        var viewModel = new NovelItemViewModel((await App.AppViewModel.MakoClient.GetNovelFromIdAsync(id)));

        frameworkElement.CreateNovelPage(viewModel, [viewModel]);
    }

    /// <summary>
    /// 此方法无法加载更多小说
    /// </summary>
    /// <typeparam name="T">为了方便协变采用泛型</typeparam>
    /// <param name="frameworkElement"></param>
    /// <param name="novelViewModel">指定的小说ViewModel</param>
    /// <param name="novelViewModels">指定的小说ViewModel所在的列表</param>
    public static void CreateNovelPage<T>(this FrameworkElement frameworkElement, T novelViewModel, IList<T> novelViewModels) where T : NovelItemViewModel
    {
        var index = novelViewModels.IndexOf(novelViewModel);
        frameworkElement.CreateNovelPage(novelViewModel.Entry, (novelViewModels, index));
    }

    /// <summary>
    /// 此方法可以使用<paramref name="novelViewViewModel"/>的<see cref="NovelViewViewModel.DataProvider"/>来加载更多小说
    /// </summary>
    /// <param name="frameworkElement"></param>
    /// <param name="novelViewModel">指定的小说ViewModel</param>
    /// <param name="novelViewViewModel">指定的小说ViewModel所在的<see cref="WorkView"/>的ViewModel</param>
    public static void CreateNovelPage(this FrameworkElement frameworkElement, NovelItemViewModel novelViewModel, NovelViewViewModel novelViewViewModel)
    {
        var index = novelViewViewModel.DataProvider.View.IndexOf(novelViewModel);
        frameworkElement.CreateNovelPage(novelViewModel.Entry, (novelViewViewModel, index));
    }

    public static NovelViewerPageViewModel GetNovelViewerPageViewModelFromHandle(this NovelViewerPage hWnd, object? param)
    {
        return param switch
        {
            (NovelViewViewModel novelViewViewModel, int index) => new NovelViewerPageViewModel(
                novelViewViewModel, index, hWnd),
            (IEnumerable novelViewModels, int index) => new NovelViewerPageViewModel(
                novelViewModels.Cast<NovelItemViewModel>(), index, hWnd),
            _ => ThrowHelper.Argument<object, NovelViewerPageViewModel>(param, "Invalid parameter type.")
        };
    }

    private static void CreateNovelPage(this FrameworkElement frameworkElement, Novel novel, object param)
    {
        if (frameworkElement.FindAscendantOrSelf<TabPage>() is not { } tabPage)
            return;
        tabPage.AddPage(new NavigationViewTag<NovelViewerPage>(novel.Title, param));
    }
}
