using Windows.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Controls;
using Pixeval.Controls.Windowing;
using WinUI3Utilities;
using Pixeval.CoreApi.Net.Response;

namespace Pixeval.Pages.IllustratorViewer;

public static class IllustratorViewerHelper
{
    public static IllustratorViewerPageViewModel GetViewModel(this FrameworkElement element, object? param)
    {
        return param switch
        {
            PixivSingleUserResponse userDetail => new IllustratorViewerPageViewModel(userDetail, element),
            _ => ThrowHelper.Argument<object, IllustratorViewerPageViewModel>(param, "Invalid parameter type.")
        };
    }

    public static void CreateWindowWithPage(PixivSingleUserResponse userDetail)
    {
        WindowFactory.RootWindow.Fork(out var w)
            .WithLoaded((o, _) => o.To<Microsoft.UI.Xaml.Controls.Frame>().NavigateTo<IllustratorViewerPage>(w,
                userDetail,
                new SuppressNavigationTransitionInfo()))
            .WithSizeLimit(640, 360)
            .Init(userDetail.UserEntity.Name, new SizeInt32(1280, 720))
            .Activate();
    }
}
