// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Numerics;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Pixeval.Controls;

public sealed partial class IllustratorItem
{
    [GeneratedDependencyProperty]
    public partial IllustratorItemViewModel ViewModel { get; set; }

    public event Action<IllustratorItem, IllustratorItemViewModel>? ViewModelChanged;

    public event Func<TeachingTip> RequestTeachingTip = null!;

    partial void OnViewModelPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        ViewModelChanged?.Invoke(this, ViewModel);
    }

    private const float RotatedRotation = 10f;
    private const float CommonRotation = 0f;

    private static readonly Vector3 _ZoomedScale = new(1.2f, 1.2f, 1.2f);
    private static readonly Vector3 _CommonScale = new(1, 1, 1);
    private static readonly Vector3 _ElevatedTranslation = new(0, 0, 60);
    private static readonly Vector3 _CommonTranslation = new(0, 0, 30);

    private TeachingTip QrCodeTeachingTip => RequestTeachingTip();

    public IllustratorItem() => InitializeComponent();

    private void AvatarButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
    }

    private void RestoreAvatarButton(object? sender, object e)
    {
        if (!AvatarButton.Flyout.IsOpen)
        {
            AvatarButton.Scale = _CommonScale;
            AvatarButton.Translation = _CommonTranslation;
            AvatarButton.Rotation = CommonRotation;
        }
    }
}
