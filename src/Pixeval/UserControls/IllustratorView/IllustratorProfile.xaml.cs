using System.Linq;
using System.Numerics;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Pixeval.Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.UserControls.IllustratorView;

[DependencyProperty<IllustratorViewModel>("ViewModel")]
public sealed partial class IllustratorProfile
{
    public static readonly Vector3 ZoomedScale = new(1.2f, 1.2f, 1.2f);
    public static readonly Vector3 CommonScale = new(1, 1, 1);
    public static readonly Vector3 ElevatedTranslation = new(0, 0, 60);
    public static readonly Vector3 CommonTranslation = new(0, 0, 30);
    public static readonly float RotatedRotation = 10f;
    public static readonly float CommonRotation = 0f;

    public IllustratorProfile()
    {
        InitializeComponent();
    }

    private void AvatarButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
    }

    private async void IllustratorProfile_OnLoaded(object sender, RoutedEventArgs e)
    {
        var result = await ViewModel.BannerImageTask;
        var averageLength = 300 / result.Length;
        var images = result.Select(source => new Image
        {
            Source = source,
            Stretch = Stretch.UniformToFill,
            Width = averageLength,
            Height = 100
        }.Apply(i => UIElementExtensions.SetClipToBounds(i, true)));
        BannerContainer.Children.AddRange(images);
    }

    private void AvatarButton_OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        RestoreAvatarButton();
    }

    private void AvatarButtonMenuFlyout_OnClosed(object? sender, object e)
    {
        RestoreAvatarButton();
    }

    private void RestoreAvatarButton()
    {
        if (!AvatarButton.Flyout.IsOpen)
        {
            AvatarButton.Scale = CommonScale;
            AvatarButton.Translation = CommonTranslation;
            AvatarButton.Rotation = CommonRotation;
            BlurOutAnimation.Start(Banner);
        }
    }
}