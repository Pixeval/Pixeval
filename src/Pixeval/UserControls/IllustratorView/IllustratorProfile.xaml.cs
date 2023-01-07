using System.Linq;
using System.Numerics;
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

    private async void IllustratorToolTip_OnOpened(object sender, RoutedEventArgs e)
    {
        const int singleImageSize = 100;
        const int imageSpacing = 5;
        const int containerWidth = 300;
        if (await ViewModel.GetIllustratorDisplayImagesAsync() is { Length: > 0 and var length } sources)
        {
            IllustratorToolTipContainer.Width = singleImageSize * length + (length - 1) * imageSpacing;
            var images = sources.Select(s => new Border
            {
                Width = (double)containerWidth / length,
                Height = (double)containerWidth / length,
                CornerRadius = new CornerRadius(5),
                Child = new Image
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Source = s,
                    Stretch = Stretch.UniformToFill
                }
            });
            IllustratorDisplayImagesContainer.Children.AddRange(images);
        }
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