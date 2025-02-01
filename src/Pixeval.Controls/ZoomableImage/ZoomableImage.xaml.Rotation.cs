// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using Microsoft.UI.Xaml;
using WinUI3Utilities;

namespace Pixeval.Controls;

public partial class ZoomableImage
{
    partial void OnImageRotationDegreePropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        if (IsDisposed)
            return;

        if (ImageRotationDegree % 90 is not 0)
        {
            ThrowHelper.Argument(ImageRotationDegree, $"{nameof(ImageRotationDegree)} must be a multiple of 90");
        }

        switch (ImageRotationDegree)
        {
            case >= 360:
                ImageRotationDegree %= 360;
                return;
            case <= -360:
                ImageRotationDegree = ImageRotationDegree % 360 + 360;
                return;
            case < 0:
                ImageRotationDegree += 360;
                return;
        }

        // 更新图片大小
        OnPropertyChanged(nameof(ImageWidth));
        OnPropertyChanged(nameof(ImageHeight));

        // 更新图片位置
        OnPropertyChanged(nameof(ImagePositionLeft));
        OnPropertyChanged(nameof(ImagePositionTop));
        OnPropertyChanged(nameof(ImagePositionRight));
        OnPropertyChanged(nameof(ImagePositionBottom));
    }
}
