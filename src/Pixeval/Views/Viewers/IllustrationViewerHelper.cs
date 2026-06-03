// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Misaki;
using Pixeval.Utilities;
using Pixeval.ViewModels;
using Pixeval.ViewModels.Viewers;
using Pixeval.Views.ViewContainers;

namespace Pixeval.Views.Viewers;

public static class IllustrationViewerHelper
{
    /// <param name="control"></param>
    extension(ViewContainerBase control)
    {
        /// <summary>
        /// 此方法无法加载更多插画，加载单张图使用
        /// </summary>
        public Task CreateIllustrationPageAsync(IIdentityInfo id)
            => control.CreateIllustrationPageAsync(id.Id, id.Platform);

        /// <summary>
        /// 此方法无法加载更多插画，加载单张图使用
        /// </summary>
        public async Task CreateIllustrationPageAsync(string id, string platform)
        {
            if (await id.TryGetIArtworkInfoAsync(platform) is not { } entry)
                return;

            var viewModel = IllustrationItemViewModel.CreateInstance(entry);

            control.CreateIllustrationPage(viewModel, [viewModel]);
        }

        /// <summary>
        /// 此方法无法加载更多插画
        /// </summary>
        /// <typeparam name="T">为了方便协变采用泛型</typeparam>
        /// <param name="illustrationViewModel">指定的插画ViewModel</param>
        /// <param name="illustrationViewModels">指定的插画ViewModel所在的列表</param>
        public void CreateIllustrationPage<T>(T illustrationViewModel, IList<T> illustrationViewModels) where T : IllustrationItemViewModel
        {
            var index = illustrationViewModels.IndexOf(illustrationViewModel);
            CreateIllustrationPage(control, new IllustrationViewerPageViewModel(illustrationViewModels, index));
        }

        /// <summary>
        /// 此方法可以使用<paramref name="sourceView"/>来加载更多插画
        /// </summary>
        /// <param name="illustrationViewModel">指定的插画ViewModel</param>
        /// <param name="sourceView">指定的插画ViewModel所在的DataProvider</param>
        public void CreateIllustrationPage(IllustrationItemViewModel illustrationViewModel, ISourceView<IllustrationItemViewModel> sourceView)
        {
            var index = sourceView.View.IndexOf(illustrationViewModel);
            CreateIllustrationPage(control, new IllustrationViewerPageViewModel(sourceView, index));
        }
    }

    private static void CreateIllustrationPage(ViewContainerBase control, IllustrationViewerPageViewModel param)
    {
        control.NavigateTo(new IllustrationViewerPage(param));
    }

    public static async Task<IArtworkInfo?> TryGetIArtworkInfoAsync(this string id, string platform)
    {
        var getArtworkService = App.AppViewModel.GetPlatformService<IGetArtworkService>(platform);
        if (getArtworkService is null)
            return null;
        try
        {
            return await getArtworkService.GetArtworkAsync(id);
        }
        catch (Exception e)
        {
            var logger = App.AppViewModel.AppServiceProvider.GetRequiredService<FileLogger>();
            logger.LogError(nameof(TryGetIArtworkInfoAsync), e);
            return null;
        }
    }
}
