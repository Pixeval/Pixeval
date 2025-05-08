// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Pixeval.Controls.Windowing;
using Mako.Net.Response;
using WinUI3Utilities;
using Pixeval.Util.UI;
using Mako.Model;

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
        var user = await App.AppViewModel.GetFromJsonAsync<UserEntity>("get/user", -1, ("followedUserId", userId.ToString()));
        var userDetail = new PixivSingleUserResponse()
        {
            UserEntity = user,
            UserProfile = Profile.CreateDefault(),
            UserProfilePublicity = ProfilePublicity.CreateDefault(),
            UserWorkspace = Workspace.CreateDefault()
        };
        frameworkElement.CreateIllustratorPage(userDetail);
    }

    public static void CreateIllustratorPage(this FrameworkElement frameworkElement, PixivSingleUserResponse userDetail)
    {
        if (frameworkElement.FindAscendantOrSelf<TabPage>() is { } tabPage)
            tabPage.AddPage(new NavigationViewTag<IllustratorViewerPage>(userDetail.UserEntity.Name, userDetail)
            {
                ImageUri = ViewerPageTag.GetPlatformUri()
            });
    }
}
