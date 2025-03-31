// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Pixeval.Controls;
using Pixeval.Controls.Windowing;
using Mako.Model;
using WinUI3Utilities;

namespace Pixeval.Pages.IllustrationViewer;

public static class IllustrationViewerHelper
{
    /// <summary>
    /// 此方法无法加载更多插画
    /// </summary>
    public static async Task CreateIllustrationPageAsync(this FrameworkElement frameworkElement, long id, ICollection<long> otherIds)
    {
        var viewModel = null as IllustrationItemViewModel;
        var viewModels = new List<IllustrationItemViewModel>();
        foreach (var otherId in otherIds)
        {
            var illustrationItemViewModel = new IllustrationItemViewModel(await App.AppViewModel.MakoClient.GetIllustrationFromIdAsync(id));
            viewModels.Add(illustrationItemViewModel);
            if (otherId == id)
            {
                viewModel = illustrationItemViewModel;
            }
        }

        if (viewModel is null)
            ThrowHelper.InvalidOperation("Specified illustration not found in the list.");

        frameworkElement.CreateIllustrationPage(viewModel, viewModels);
    }

    /// <summary>
    /// 此方法无法加载更多插画，加载单张图使用
    /// </summary>
    public static async Task CreateIllustrationPageAsync(this FrameworkElement frameworkElement, long id)
    {
        var viewModel = new IllustrationItemViewModel(await App.AppViewModel.MakoClient.GetIllustrationFromIdAsync(id));

        frameworkElement.CreateIllustrationPage(viewModel, [viewModel]);
    }

    /// <summary>
    /// 此方法无法加载更多插画
    /// </summary>
    /// <typeparam name="T">为了方便协变采用泛型</typeparam>
    /// <param name="frameworkElement"></param>
    /// <param name="illustrationViewModel">指定的插画ViewModel</param>
    /// <param name="illustrationViewModels">指定的插画ViewModel所在的列表</param>
    public static void CreateIllustrationPage<T>(this FrameworkElement frameworkElement, T illustrationViewModel, IList<T> illustrationViewModels) where T : IllustrationItemViewModel
    {
        var index = illustrationViewModels.IndexOf(illustrationViewModel);
        CreateIllustrationPage(frameworkElement, illustrationViewModel.Entry, (illustrationViewModels, index));
    }

    /// <summary>
    /// 此方法可以使用<paramref name="illustrationViewViewModel"/>的<see cref="IllustrationViewViewModel.DataProvider"/>来加载更多插画
    /// </summary>
    /// <param name="frameworkElement"></param>
    /// <param name="illustrationViewModel">指定的插画ViewModel</param>
    /// <param name="illustrationViewViewModel">指定的插画ViewModel所在的<see cref="WorkView"/>的ViewModel</param>
    public static void CreateIllustrationPage(this FrameworkElement frameworkElement, IllustrationItemViewModel illustrationViewModel, IllustrationViewViewModel illustrationViewViewModel)
    {
        var index = illustrationViewViewModel.DataProvider.View.IndexOf(illustrationViewModel);
        CreateIllustrationPage(frameworkElement, illustrationViewModel.Entry, (illustrationViewViewModel, index));
    }

    public static IllustrationViewerPageViewModel GetIllustrationViewerPageViewModelFromHandle(this IllustrationViewerPage page, object? param)
    {
        return param switch
        {
            (IllustrationViewViewModel illustrationViewViewModel, int index) => new IllustrationViewerPageViewModel(
                illustrationViewViewModel, index, page),
            (IEnumerable illustrationViewModels, int index) => new IllustrationViewerPageViewModel(
                illustrationViewModels.Cast<IllustrationItemViewModel>(), index, page),
            _ => ThrowHelper.Argument<object, IllustrationViewerPageViewModel>(param, "Invalid parameter type.")
        };
    }

    private static void CreateIllustrationPage(FrameworkElement frameworkElement, Illustration illustration, object param)
    {
        if (frameworkElement.FindAscendantOrSelf<TabPage>() is not { } tabPage)
            return;
        tabPage.AddPage(new NavigationViewTag<IllustrationViewerPage>(illustration.Title, param));
    }
}
