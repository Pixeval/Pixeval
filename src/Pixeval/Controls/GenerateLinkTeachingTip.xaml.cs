using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace Pixeval.Controls;

public sealed partial class GenerateLinkTeachingTip
{
    public event TypedEventHandler<FrameworkElement, object>? ImageLoading;

    public GenerateLinkTeachingTip() => InitializeComponent();

    private void GenerateLinkToThisPageButtonTeachingTip_OnActionButtonClick(TeachingTip sender, object args)
    {
        IsOpen = false;
        App.AppViewModel.AppSettings.DisplayTeachingTipWhenGeneratingAppLink = false;
    }

    private void Image_OnLoading(FrameworkElement sender, object args) => ImageLoading?.Invoke(sender, args);
}
