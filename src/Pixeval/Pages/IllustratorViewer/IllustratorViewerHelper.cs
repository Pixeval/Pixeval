// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Threading.Tasks;
using Pixeval.Controls.Windowing;
using WinUI3Utilities;
using Pixeval.CoreApi.Net.Response;
using Microsoft.UI.Xaml;
using CommunityToolkit.WinUI;

namespace Pixeval.Pages.IllustratorViewer;

public static class IllustratorViewerHelper
{
    public static IllustratorViewerPageViewModel GetIllustratorViewerPageViewModel(this FrameworkElement frameworkElement, object? param)
    {
        return param switch
        {
            PixivSingleUserResponse userDetail => new IllustratorViewerPageViewModel(userDetail, frameworkElement),
            _ => ThrowHelper.Argument<object, IllustratorViewerPageViewModel>(param, "Invalid parameter type.")
        };
    }

    public static async Task CreateIllustratorPageAsync(this FrameworkElement frameworkElement, long userId)
    {
        var userDetail = await App.AppViewModel.MakoClient.GetUserFromIdAsync(userId, App.AppViewModel.AppSettings.TargetFilter);
        frameworkElement.CreateIllustratorPage(userDetail);
    }

    public static void CreateIllustratorPage(this FrameworkElement frameworkElement, PixivSingleUserResponse userDetail)
    {
        if (frameworkElement.FindAscendantOrSelf<TabPage>() is not { } tabPage)
            return;
        tabPage.AddPage(new NavigationViewTag<IllustratorViewerPage>(userDetail.UserEntity.Name, userDetail));
    }
}
