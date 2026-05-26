// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Pixeval.ViewModels;
using Pixeval.ViewModels.Viewers;
using Pixeval.Views.ViewContainers;

namespace Pixeval.Views.Viewers;

public static class NovelViewerHelper
{
    /// <param name="control"></param>
    extension(ViewContainerBase control)
    {
        /// <summary>
        /// 此方法无法加载更多小说，加载单个小说使用。
        /// </summary>
        public async Task CreateNovelPageAsync(long id)
        {
            var novel = await App.AppViewModel.MakoClient.GetNovelFromIdAsync(id);
            var viewModel = NovelItemViewModel.CreateInstance(novel);
            control.CreateNovelPage(viewModel, [viewModel]);
        }

        /// <summary>
        /// 此方法无法加载更多小说。
        /// </summary>
        /// <typeparam name="T">为了方便协变采用泛型。</typeparam>
        /// <param name="novelViewModel">指定的小说ViewModel。</param>
        /// <param name="novelViewModels">指定的小说ViewModel所在的列表。</param>
        public void CreateNovelPage<T>(T novelViewModel, IList<T> novelViewModels) where T : NovelItemViewModel
        {
            var index = novelViewModels.IndexOf(novelViewModel);
            control.NavigateTo(new NovelViewerPage(new NovelViewerPageViewModel(novelViewModels, index)));
        }

        /// <summary>
        /// 此方法可以使用<paramref name="novelViewViewModel"/>的DataProvider来加载更多小说。
        /// </summary>
        /// <param name="novelViewModel">指定的小说ViewModel。</param>
        /// <param name="novelViewViewModel">指定的小说ViewModel所在的WorkView的ViewModel。</param>
        public void CreateNovelPage(NovelItemViewModel novelViewModel, NovelViewViewModel novelViewViewModel)
        {
            var index = novelViewViewModel.View.IndexOf(novelViewModel);
            control.NavigateTo(new NovelViewerPage(new NovelViewerPageViewModel(novelViewViewModel, index)));
        }
    }
}
