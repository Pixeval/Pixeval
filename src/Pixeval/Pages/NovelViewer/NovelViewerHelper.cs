// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Graphics;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Controls;
using Pixeval.Controls.Windowing;
using Pixeval.CoreApi.Model;
using WinUI3Utilities;

namespace Pixeval.Pages.NovelViewer;

public static class NovelViewerHelper
{
    /// <summary>
    /// 此方法无法加载更多小说，加载单个小说使用
    /// </summary>
    public static async Task CreateWindowWithPageAsync(long id)
    {
        var viewModel = new NovelItemViewModel((await App.AppViewModel.MakoClient.GetNovelFromIdAsync(id)));

        viewModel.CreateWindowWithPage([viewModel]);
    }

    /// <summary>
    /// 此方法无法加载更多小说
    /// </summary>
    /// <typeparam name="T">为了方便协变采用泛型</typeparam>
    /// <param name="novelViewModel">指定的小说ViewModel</param>
    /// <param name="novelViewModels">指定的小说ViewModel所在的列表</param>
    public static void CreateWindowWithPage<T>(this T novelViewModel, IList<T> novelViewModels) where T : NovelItemViewModel
    {
        var index = novelViewModels.IndexOf(novelViewModel);
        CreateWindowWithPage(novelViewModel.Entry, (novelViewModels, index));
    }

    /// <summary>
    /// 此方法可以使用<paramref name="novelViewViewModel"/>的<see cref="NovelViewViewModel.DataProvider"/>来加载更多小说
    /// </summary>
    /// <param name="novelViewModel">指定的小说ViewModel</param>
    /// <param name="novelViewViewModel">指定的小说ViewModel所在的<see cref="WorkView"/>的ViewModel</param>
    public static void CreateWindowWithPage(this NovelItemViewModel novelViewModel, NovelViewViewModel novelViewViewModel)
    {
        var index = novelViewViewModel.DataProvider.View.IndexOf(novelViewModel);
        CreateWindowWithPage(novelViewModel.Entry, (novelViewViewModel, index));
    }

    public static NovelViewerPageViewModel GetNovelViewerPageViewModelFromHandle(this ulong hWnd, object? param)
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

    public static void CreateWindowWithPage(Novel novel, object param)
    {
        WindowFactory.RootWindow.Fork(out var h)
            .WithLoaded((o, _) => o.To<Frame>().NavigateTo<NovelViewerPage>(h,
                param,
                new SuppressNavigationTransitionInfo()))
            .WithSizeLimit(640, 360)
            .Init(novel.Title, new SizeInt32(1280, 720), WindowFactory.RootWindow.IsMaximize)
            .Activate();
    }
}
