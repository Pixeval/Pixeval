// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using System;
using CommunityToolkit.WinUI;
using WinUI3Utilities;

namespace Pixeval.Controls;

public partial class ZoomableImage
{
    public object? Source
    {
        get;
        set
        {
            if (IsDisposed || field == value)
                return;
            IsPlaying = true;
            _timerRunning = false;
            field = value;
            // 使CanvasControl具有大小，否则不会触发CanvasControlOnDraw
            OriginalImageWidth = OriginalImageHeight = 10;
            _needInitSource = true;
        }
    }
    public bool IsPlaying {
        get;
        set
        {
            if(!value)//开始暂停
            {
                _pauseStart = DateTime.Now;
            }
            else
            {
                _gifStartTime += DateTime.Now - _pauseStart;
            }
            field = value;

        } 
    } = true;
    public int ImageRotationDegree
    {
        get;
        set
        {
            if (IsDisposed || field == value)
                return;
            field = value;
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
    } = 0;

    public bool ImageIsMirrored { get; set; } = false;

    public float ImageScale {
        get;
        set
        {
            if (IsDisposed)
                return;
            OnImageScaleChangedInternal(field.To<float>());
            field = value;
        }
    }  = 1f;

    public ZoomableImageMode Mode { 
        get;
        set {
            if (IsDisposed || field == value)
                return;
            field = value;
            switch (Mode)
            {
                case ZoomableImageMode.Original:
                    ImageScale = 1;
                    SetPosition(InitPosition);
                    break;
                case ZoomableImageMode.Fit:
                    ImageScale = (float) ScaledFactor;
                    SetPosition(InitPosition);
                    break;
                case ZoomableImageMode.NotFit:
                    break;
                default:
                    ThrowHelper.ArgumentOutOfRange(value.To<ZoomableImageMode>());
                    break;
            }
        } }
    public ZoomableImageMode InitMode { get; set; }
    public ZoomableImagePosition InitPosition { get; set; }
}
