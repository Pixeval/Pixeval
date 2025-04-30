// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using System;
using Microsoft.UI.Xaml.Media.Animation;

namespace Pixeval.Controls;

public partial class ZoomableImage
{
    private void StartZoomAnimation(float? oldScale = null)
    {
        var now = Compute(out var scale, out var centerX, out var centerY, out _, out _, oldScale);
        _startScale = scale;
        _startCenterX = centerX;
        _startCenterY = centerY;
        _animationStartTime = now;
        _animationEndTime = _animationStartTime + TimeSpan.FromSeconds(0.3);
    }

    private DateTime Compute(out float scale, out double centerX, out double centerY, out double x, out double y, float? oldScale = null)
    {
        var currentScale = oldScale ?? ImageScale;
        var now = DateTime.Now;
        if (_animationStartTime >= _animationEndTime || now >= _animationEndTime)
        {
            scale = currentScale;
            centerX = ImageCenterX;
            centerY = ImageCenterY;
            x = ImagePositionLeft;
            y = ImagePositionTop;
            return now;
        }

        var progress = (now - _animationStartTime).TotalMilliseconds / (_animationEndTime - _animationStartTime).TotalMilliseconds;
        progress = EaseFunction.Ease(progress);
        scale = (currentScale - _startScale) * (float) progress + _startScale;
        centerX = (ImageCenterX - _startCenterX) * progress + _startCenterX;
        centerY = (ImageCenterY - _startCenterY) * progress + _startCenterY;
        x = centerX - ImageWidth * scale / 2;
        y = centerY - ImageHeight * scale / 2;
        return now;
    }

    private float _startScale;
    private double _startCenterX;
    private double _startCenterY;
    private DateTime _animationStartTime = DateTime.Today;
    private DateTime _animationEndTime = DateTime.Today;

    private CircleEase EaseFunction { get; set; } = new() { EasingMode = EasingMode.EaseOut };

    private class CircleEase
    {
        public EasingMode EasingMode { get; set; }

        private double EaseInCore(double normalizedTime)
        {
            normalizedTime = Math.Max(0.0, Math.Min(1.0, normalizedTime));
            return 1.0 - Math.Sqrt(1.0 - normalizedTime * normalizedTime);
        }
        public double Ease(double normalizedTime)
        {
            switch (EasingMode)
            {
                case EasingMode.EaseIn:
                    return EaseInCore(normalizedTime);
                case EasingMode.EaseOut:
                    // EaseOut is the same as EaseIn, except time is reversed & the result is flipped.
                    return 1.0 - EaseInCore(1.0 - normalizedTime);
                case EasingMode.EaseInOut:
                default:
                    // EaseInOut is a combination of EaseIn & EaseOut fit to the 0-1, 0-1 range.
                    return (normalizedTime < 0.5)
                        ? EaseInCore(normalizedTime * 2.0) * 0.5
                        : (1.0 - EaseInCore((1.0 - normalizedTime) * 2.0)) * 0.5 + 0.5;
            }
        }
    }
}
