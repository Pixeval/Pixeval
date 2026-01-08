// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Pixeval.Controls.Windowing;
using Mako.Net.Response;
using WinUI3Utilities;
using Pixeval.Util.UI;

namespace Pixeval.Pages.IllustratorViewer;

public static class IllustratorViewerHelper
{
    extension(FrameworkElement frameworkElement)
    {
        public IllustratorViewerPageViewModel GetIllustratorViewerPageViewModel(object? param)
        {
            return param switch
            {
                PixivSingleUserResponse userDetail => new IllustratorViewerPageViewModel(userDetail, frameworkElement),
                _ => ThrowHelper.Argument<object, IllustratorViewerPageViewModel>(param, "Invalid parameter type.")
            };
        }

        public async Task CreateIllustratorPageAsync(long userId)
        {
            var userDetail = await App.AppViewModel.MakoClient.GetUserFromIdAsync(userId, App.AppViewModel.AppSettings.TargetFilter);
            frameworkElement.CreateIllustratorPage(userDetail);
        }

        public void CreateIllustratorPage(PixivSingleUserResponse userDetail)
        {
            if (frameworkElement.FindAscendantOrSelf<TabPage>() is { } tabPage)
                tabPage.AddPage(new NavigationViewTag<IllustratorViewerPage>(userDetail.UserEntity.Name, userDetail)
                {
                    ImageUri = ViewerPageTag.GetPlatformUri()
                });
        }
    }
}
