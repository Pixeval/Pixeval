using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Pixeval.Util.Threading;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.UserControls.RecommendIllustratorProfile;

[DependencyProperty<RecommendIllustratorProfileViewModel>("ViewModel")]
public sealed partial class RecommendIllustratorProfile
{
    private const string ButtonPointerOverBackgroundKey = "ButtonBackgroundPointerOver";

    public RecommendIllustratorProfile()
    {
        InitializeComponent();
    }

    private void RecommendIllustratorProfile_OnLoaded(object sender, RoutedEventArgs e)
    {
        ThreadingHelper.Fork(async () =>
        {
            if (ViewModel.AvatarTask is { } task)
            {
                var result = await task;
                AvatarPersonPicture.ProfilePicture = result;
            }
        });
        ThreadingHelper.Fork(async () =>
        {
            if (ViewModel.DisplayImagesTask is { } task)
            {
                var results = await task;
                var averageLength = 300 / results.Length;
                var images = results.Select(s => new Image
                {
                    Width = averageLength,
                    Height = 100,
                    Stretch = Stretch.UniformToFill,
                    Source = s
                });
                IllustratorDisplayImages.Children.AddRange(images);
            }
        });
    }

    private void FollowButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        if (ViewModel.Followed)
        {
            ViewModel.Unfollow();
        }
        else
        {
            ViewModel.Follow();
        }

        Resources[ButtonPointerOverBackgroundKey] = new SolidColorBrush(ViewModel.GetButtonBackground(ViewModel.Followed).Color.Brighten(25));
    }

    private void FollowButton_OnLoaded(object sender, RoutedEventArgs e)
    {
        Resources[ButtonPointerOverBackgroundKey] = new SolidColorBrush(ViewModel.GetButtonBackground(ViewModel.Followed).Color.Brighten(25));
    }
}