using System.Threading.Tasks;
using Windows.Graphics;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Controls;
using Pixeval.Controls.Windowing;
using WinUI3Utilities;
using Pixeval.CoreApi.Net.Response;

namespace Pixeval.Pages.IllustratorViewer;

public static class IllustratorViewerHelper
{
    public static IllustratorViewerPageViewModel GetViewModel(this ulong hWnd, object? param)
    {
        return param switch
        {
            PixivSingleUserResponse userDetail => new IllustratorViewerPageViewModel(userDetail, hWnd),
            _ => ThrowHelper.Argument<object, IllustratorViewerPageViewModel>(param, "Invalid parameter type.")
        };
    }

    public static async Task CreateWindowWithPageAsync(long userId)
    {
        var userDetail = await App.AppViewModel.MakoClient.GetUserFromIdAsync(userId, App.AppViewModel.AppSettings.TargetFilter);
        CreateWindowWithPage(userDetail);
    }

    public static void CreateWindowWithPage(PixivSingleUserResponse userDetail)
    {
        WindowFactory.RootWindow.Fork(out var h)
            .WithLoaded((o, _) => o.To<Microsoft.UI.Xaml.Controls.Frame>().NavigateTo<IllustratorViewerPage>(h,
                userDetail,
                new SuppressNavigationTransitionInfo()))
            .WithSizeLimit(640, 360)
            .Init(userDetail.UserEntity.Name, new SizeInt32(1280, 720), WindowFactory.RootWindow.IsMaximize)
            .Activate();
    }
}
