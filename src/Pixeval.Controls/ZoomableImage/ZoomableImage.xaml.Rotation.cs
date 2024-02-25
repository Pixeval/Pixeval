using Microsoft.UI.Xaml;
using WinUI3Utilities;

namespace Pixeval.Controls;

public partial class ZoomableImage
{
    private static void OnImageRotationDegreeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (EnsureNotDisposed(d) is not { } zoomableImage)
            return;

        if (zoomableImage.ImageRotationDegree % 90 is not 0)
        {
            ThrowHelper.Argument(zoomableImage.ImageRotationDegree, $"{nameof(ImageRotationDegree)} must be a multiple of 90");
        }

        switch (zoomableImage.ImageRotationDegree)
        {
            case >= 360:
                zoomableImage.ImageRotationDegree %= 360;
                return;
            case <= -360:
                zoomableImage.ImageRotationDegree = zoomableImage.ImageRotationDegree % 360 + 360;
                return;
            case < 0:
                zoomableImage.ImageRotationDegree += 360;
                return;
        }

        // 更新图片大小
        zoomableImage.OnPropertyChanged(nameof(ImageWidth));
        zoomableImage.OnPropertyChanged(nameof(ImageHeight));

        // 更新图片位置
        zoomableImage.OnPropertyChanged(nameof(ImagePositionLeft));
        zoomableImage.OnPropertyChanged(nameof(ImagePositionTop));
        zoomableImage.OnPropertyChanged(nameof(ImagePositionRight));
        zoomableImage.OnPropertyChanged(nameof(ImagePositionBottom));
    }
}
