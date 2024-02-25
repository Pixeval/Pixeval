using System;

namespace Pixeval.Controls;

public partial class ZoomableImage
{
    private void StartZoomAnimation(float? oldScale = null)
    {
        var now = Compute(out var scale, out var centerX, out var centerY, out _, out _, oldScale);
        _startScale = scale;
        _startCenterX = centerX;
        _startCenterY = centerY;
        _startTime = now;
        _endTime = _startTime + TimeSpan.FromSeconds(0.5);
    }

    private DateTime Compute(out float scale, out double centerX, out double centerY, out double x, out double y, float? oldScale = null)
    {
        var currentScale = oldScale ?? ImageScale;
        var now = DateTime.Now;
        if (_startTime >= _endTime || now >= _endTime)
        {
            scale = currentScale;
            centerX = ImageCenterX;
            centerY = ImageCenterY;
            x = ImagePositionLeft;
            y = ImagePositionTop;
            return now;
        }

        var progress = ((now - _startTime).TotalMilliseconds / (_endTime - _startTime).TotalMilliseconds);
        scale = (currentScale - _startScale) * (float)progress + _startScale;
        centerX = (ImageCenterX - _startCenterX) * progress + _startCenterX;
        centerY = (ImageCenterY - _startCenterY) * progress + _startCenterY;
        x = centerX - ImageWidth * scale / 2;
        y = centerY - ImageHeight * scale / 2;
        return now;
    }

    private float _startScale;
    private double _startCenterX;
    private double _startCenterY;
    private DateTime _startTime = DateTime.Today;
    private DateTime _endTime = DateTime.Today;
}
