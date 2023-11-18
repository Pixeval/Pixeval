using System;
using System.Numerics;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Pixeval.Util;
using Pixeval.Util.UI;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<IllustratorProfileViewModel>("ViewModel")]
public sealed partial class IllustratorProfile
{
    public static readonly Vector3 ZoomedScale = new(1.2f, 1.2f, 1.2f);
    public static readonly Vector3 CommonScale = new(1, 1, 1);
    public static readonly Vector3 ElevatedTranslation = new(0, 0, 60);
    public static readonly Vector3 CommonTranslation = new(0, 0, 30);
    public const float RotatedRotation = 10f;
    public const float CommonRotation = 0f;

    public event Func<TeachingTip>? RequestTeachingTip;

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
        for (var i = 0; i < Math.Min(result.Length, 3); i++)
        {
            var image = new Image
            {
                Source = result[i],
                Stretch = Stretch.UniformToFill,
            };
            UIElementExtensions.SetClipToBounds(image, true);
            Grid.SetColumn(image, i);
            BannerContainer.Children.Add(image);
        }
    }

    private void RestoreAvatarButton(object? sender, object e)
    {
        if (!AvatarButton.Flyout.IsOpen)
        {
            AvatarButton.Scale = CommonScale;
            AvatarButton.Translation = CommonTranslation;
            AvatarButton.Rotation = CommonRotation;
            // TODO: BlurEffect BlurOutAnimation.Start(Banner);
        }
    }

    private void GenerateLinkCommandOnExecuteRequested(object sender, RoutedEventArgs routedEventArgs)
    {
        UiHelper.ClipboardSetText(MakoHelper.GenerateIllustratorAppUri(ViewModel.UserId!).ToString());
        RequestTeachingTip?.Invoke().ShowAndHide(IllustratorProfileResources.LinkCopiedToClipboard);
    }

    private void GenerateWebLinkCommandOnExecuteRequested(object sender, RoutedEventArgs routedEventArgs)
    {
        UiHelper.ClipboardSetText(MakoHelper.GenerateIllustratorWebUri(ViewModel.UserId!).ToString());
        RequestTeachingTip?.Invoke().ShowAndHide(IllustratorProfileResources.LinkCopiedToClipboard);
    }
}
