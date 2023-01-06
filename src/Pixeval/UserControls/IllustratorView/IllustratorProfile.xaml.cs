using System.Numerics;
using Microsoft.UI.Xaml.Input;
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
}