// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Misaki;
using Pixeval.Utilities;
using Pixeval.ViewModels;
using Pixeval.ViewModels.Viewers;
using Pixeval.Views.ViewContainers;
using Pixeval.Views.Viewers;

namespace Pixeval.Views.Work;

public static class IllustrationViewerHelper
{
    /// <param name="control"></param>
    extension(ViewContainerBase control)
    {
        /// <summary>
        /// 此方法无法加载更多插画
        /// </summary>
        public async Task CreateIllustrationPageAsync(long id, ICollection<long> otherIds)
        {
            var viewModel = null as IllustrationItemViewModel;
            var viewModels = new List<IllustrationItemViewModel>();
            foreach (var otherId in otherIds)
            {
                var illustrationItemViewModel = IllustrationItemViewModel.CreateInstance(await App.AppViewModel.MakoClient.GetIllustrationFromIdAsync(id));
                viewModels.Add(illustrationItemViewModel);
                if (otherId == id)
                {
                    viewModel = illustrationItemViewModel;
                }
            }

            if (viewModel is null)
                throw new InvalidOperationException("Specified illustration not found in the list.");

            control.CreateIllustrationPage(viewModel, viewModels);
        }

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
            CreateIllustrationPage(control, illustrationViewModel.Entry, new IllustrationViewerPageViewModel(illustrationViewModels, index));
        }

        /// <summary>
        /// 此方法可以使用<paramref name="illustrationViewViewModel"/>的<see cref="IllustrationViewViewModel.DataProvider"/>来加载更多插画
        /// </summary>
        /// <param name="illustrationViewModel">指定的插画ViewModel</param>
        /// <param name="illustrationViewViewModel">指定的插画ViewModel所在的<see cref="WorkView"/>的ViewModel</param>
        public void CreateIllustrationPage(IllustrationItemViewModel illustrationViewModel, IllustrationViewViewModel illustrationViewViewModel)
        {
            var index = illustrationViewViewModel.DataProvider.View.IndexOf(illustrationViewModel);
            CreateIllustrationPage(control, illustrationViewModel.Entry, new IllustrationViewerPageViewModel(illustrationViewViewModel, index));
        }
    }

    public static IllustrationViewerPageViewModel GetViewModelFromParameter(object? param)
    {
        return param switch
        {
            (IllustrationViewViewModel illustrationViewViewModel, int index) => new IllustrationViewerPageViewModel(
                illustrationViewViewModel, index),
            (IEnumerable illustrationViewModels, int index) => new IllustrationViewerPageViewModel(
                illustrationViewModels.Cast<IllustrationItemViewModel>(), index),
            _ => throw new ArgumentException("Invalid parameter type.", nameof(param))
        };
    }

    private static void CreateIllustrationPage(ViewContainerBase control, IArtworkInfo illustration, IllustrationViewerPageViewModel param)
    {
        control.NavigateTo<IllustrationViewerPage, IllustrationViewerPageViewModel>(null, illustration.Title, param);
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
