// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Threading.Tasks;
using Mako.Net.Responses;
using Microsoft.Extensions.DependencyInjection;
using Misaki;
using Pixeval.Utilities;
using Pixeval.ViewModels;
using Pixeval.ViewModels.Viewers;
using Pixeval.Views.ViewContainers;

namespace Pixeval.Views.Viewers;

public static class ViewerHelper
{
    /// <param name="control"></param>
    extension(ViewContainerBase control)
    {
        #region Illustration

        /// <summary>
        /// 此方法无法加载更多插画，加载单张图使用
        /// </summary>
        public void CreateIllustrationPage(IIdentityInfo info)
        {
            control.NavigateTo(new IllustrationViewerPage(new(info)));
        }

        /// <summary>
        /// 此方法无法加载更多插画，加载单张图使用
        /// </summary>
        public void CreateIllustrationPage(string id, string platform)
        {
            control.NavigateTo(new IllustrationViewerPage(new(new SimpleIdentityInfo(id, platform))));
        }

        /// <summary>
        /// 此方法无法加载更多插画
        /// </summary>
        /// <typeparam name="T">为了方便协变采用泛型</typeparam>
        /// <param name="illustrationViewModel">指定的插画ViewModel</param>
        /// <param name="needRefresh"></param>
        public void CreateIllustrationPage<T>(T illustrationViewModel, bool needRefresh = false) where T : IllustrationItemViewModel
        {
            control.NavigateTo(new IllustrationViewerPage(new(illustrationViewModel, needRefresh)));
        }

        /// <summary>
        /// 此方法可以使用<paramref name="sourceView"/>来加载更多插画
        /// </summary>
        /// <param name="illustrationViewModel">指定的插画ViewModel</param>
        /// <param name="sourceView">指定的插画ViewModel所在的SourceView</param>
        /// <param name="needRefresh">是否需要刷新插画（如从数据库中加载的则需要刷新）</param>
        public void CreateIllustrationPage(IllustrationItemViewModel illustrationViewModel, ISourceView<IllustrationItemViewModel> sourceView, bool needRefresh = false)
        {
            var index = sourceView.View.IndexOf(illustrationViewModel);
            control.NavigateTo(new IllustrationViewerPage(new(sourceView, index, needRefresh)));
        }

        #endregion

        #region Novel

        /// <summary>
        /// 此方法无法加载更多小说，加载单个小说使用
        /// </summary>
        public void CreateNovelPage(long id)
        {
            control.NavigateTo(new NovelViewerPage(new(id)));
        }

        /// <summary>
        /// 此方法无法加载更多小说
        /// </summary>
        /// <typeparam name="T">为了方便协变采用泛型</typeparam>
        /// <param name="novelViewModel">指定的小说ViewModel</param>
        /// <param name="needRefresh">是否需要刷新小说（如从数据库中加载的则需要刷新）</param>
        public void CreateNovelPage<T>(T novelViewModel, bool needRefresh = false) where T : NovelItemViewModel
        {
            control.NavigateTo(new NovelViewerPage(new(novelViewModel, needRefresh)));
        }

        /// <summary>
        /// 此方法可以使用<paramref name="sourceView"/>来加载更多小说
        /// </summary>
        /// <param name="novelViewModel">指定的小说ViewModel</param>
        /// <param name="sourceView">指定的小说ViewModel所在的SourceView</param>
        /// <param name="needRefresh">是否需要刷新小说（如从数据库中加载的则需要刷新）</param>
        public void CreateNovelPage(NovelItemViewModel novelViewModel, ISourceView<NovelItemViewModel> sourceView, bool needRefresh = false)
        {
            var index = sourceView.View.IndexOf(novelViewModel);
            control.NavigateTo(new NovelViewerPage(new(sourceView, index, needRefresh)));
        }

        #endregion

        #region User

        public void CreateUserPage(long userId)
        {
            control.NavigateTo(new UserViewerPage(new(userId)));
        }

        public void CreateUserPage(SingleUserResponse userDetail)
        {
            var viewModel = new UserViewerPageViewModel(userDetail);
            control.NavigateTo(new UserViewerPage(viewModel));
        }

        #endregion
    }

    extension(IIdentityInfo info)
    {
        public async Task<IArtworkInfo?> TryGetArtworkInfoAsync()
        {
            var getArtworkService = App.AppViewModel.GetPlatformService<IGetArtworkService>(info.Platform);
            if (getArtworkService is null)
                return null;
            try
            {
                return await getArtworkService.GetArtworkAsync(info.Id);
            }
            catch (Exception e)
            {
                var logger = App.AppViewModel.AppServiceProvider.GetRequiredService<FileLogger>();
                logger.LogError(nameof(TryGetArtworkInfoAsync), e);
                return null;
            }
        }
    }
}

file record SimpleIdentityInfo(string Id, string Platform) : IIdentityInfo;
