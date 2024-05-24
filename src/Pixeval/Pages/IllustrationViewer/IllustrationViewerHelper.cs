#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IllustrationViewerHelper.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
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

namespace Pixeval.Pages.IllustrationViewer;

public static class IllustrationViewerHelper
{
    /// <summary>
    /// 此方法无法加载更多插画
    /// </summary>
    public static async Task CreateWindowWithPageAsync(long id, ICollection<long> otherIds)
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

        viewModel.CreateWindowWithPage(viewModels);
    }

    /// <summary>
    /// 此方法无法加载更多插画，加载单张图使用
    /// </summary>
    public static async Task CreateWindowWithPageAsync(long id)
    {
        var viewModel = new IllustrationItemViewModel(await App.AppViewModel.MakoClient.GetIllustrationFromIdAsync(id));

        viewModel.CreateWindowWithPage([viewModel]);
    }

    /// <summary>
    /// 此方法无法加载更多插画
    /// </summary>
    /// <typeparam name="T">为了方便协变采用泛型</typeparam>
    /// <param name="illustrationViewModel">指定的插画ViewModel</param>
    /// <param name="illustrationViewModels">指定的插画ViewModel所在的列表</param>
    public static void CreateWindowWithPage<T>(this T illustrationViewModel, IList<T> illustrationViewModels) where T : IllustrationItemViewModel
    {
        var index = illustrationViewModels.IndexOf(illustrationViewModel);
        CreateWindowWithPage(illustrationViewModel.Entry, (illustrationViewModels, index));
    }

    /// <summary>
    /// 此方法可以使用<paramref name="illustrationViewViewModel"/>的<see cref="IllustrationViewViewModel.DataProvider"/>来加载更多插画
    /// </summary>
    /// <param name="illustrationViewModel">指定的插画ViewModel</param>
    /// <param name="illustrationViewViewModel">指定的插画ViewModel所在的<see cref="WorkView"/>的ViewModel</param>
    public static void CreateWindowWithPage(this IllustrationItemViewModel illustrationViewModel, IllustrationViewViewModel illustrationViewViewModel)
    {
        var index = illustrationViewViewModel.DataProvider.View.IndexOf(illustrationViewModel);
        CreateWindowWithPage(illustrationViewModel.Entry, (illustrationViewViewModel, index));
    }

    public static IllustrationViewerPageViewModel GetViewModel(this ulong hWnd, object? param)
    {
        return param switch
        {
            (IllustrationViewViewModel illustrationViewViewModel, int index) => new IllustrationViewerPageViewModel(
                illustrationViewViewModel, index, hWnd),
            (IEnumerable illustrationViewModels, int index) => new IllustrationViewerPageViewModel(
                illustrationViewModels.Cast<IllustrationItemViewModel>(), index, hWnd),
            _ => ThrowHelper.Argument<object, IllustrationViewerPageViewModel>(param, "Invalid parameter type.")
        };
    }

    private static void CreateWindowWithPage(Illustration illustration, object param)
    {
        var (width, height) = DetermineWindowSize(illustration.Width, illustration.Width / (double)illustration.Height);

        WindowFactory.RootWindow.Fork(out var h)
            .WithLoaded((o, _) => o.To<Frame>().NavigateTo<IllustrationViewerPage>(h,
                param,
                new SuppressNavigationTransitionInfo()))
            .WithSizeLimit(640, 360)
            .Init(illustration.Title, new SizeInt32(width, height), WindowFactory.RootWindow.IsMaximize)
            .Activate();
        return;

        static (int windowWidth, int windowHeight) DetermineWindowSize(int illustWidth, double illustRatio)
        {
            /*
            var windowHandle = User32.MonitorFromWindow((nint)CurrentContext.HWnd, User32.MonitorOptions.MONITOR_DEFAULTTONEAREST);
            User32.GetMonitorInfo(windowHandle, out var monitorInfoEx);
            var devMode = DEVMODE.Create();
            while (!User32.EnumDisplaySettings(
                       monitorInfoEx.DeviceName,
                       User32.ENUM_CURRENT_SETTINGS,
                       &devMode))
            { }

            var monitorWidth = devMode.dmPelsWidth;
            var monitorHeight = devMode.dmPelsHeight;
            */

            var (monitorWidth, monitorHeight) = WindowHelper.GetScreenSize();

            var determinedWidth = illustWidth switch
            {
                not 1500 => 1500 + Random.Shared.Next(0, 200),
                _ => 1500
            };
            var windowWidth = determinedWidth > monitorWidth ? monitorWidth - 100 : determinedWidth;
            // 51: determined through calculation, it is the height of the title bar
            // 80: estimated working area height
            var windowHeight = windowWidth / illustRatio + 51 is var height && height > monitorHeight - 80
                ? monitorHeight - 100
                : height;
            return (windowWidth, (int)windowHeight);
        }
    }
}
