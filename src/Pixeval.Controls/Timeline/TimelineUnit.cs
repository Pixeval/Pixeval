using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls.Timeline;

[DependencyProperty<TimelineAxisPlacement>("FoldedDefaultPlacement", "Pixeval.Controls.Timeline.TimelineAxisPlacement.Left")]
[DependencyProperty<double>("FoldThreshold", "-1.0")]
[DependencyProperty<TimelineAxisPlacement>("TimelineAxisPlacement", "Pixeval.Controls.Timeline.TimelineAxisPlacement.Left", nameof(TimelineAxisPlacementPropertyChangedCallback))]
[DependencyProperty<IconSource>("TitleIcon")]
public sealed partial class TimelineUnit : ContentControl
{
    public TimelineUnit()
    {
        DefaultStyleKey = typeof(TimelineUnit);
    }

    private bool _folded;
    private bool _differentDefaultAxisPlacement;
    private Grid _leftIndicatorAxis = null!;
    private Grid _rightIndicatorAxis = null!;
    private Grid _leftIconContainer = null!;
    private Grid _rightIconContainer = null!;
    private ContentControl _contentPresenter = null!;
    private double _containerHeightFixed;
    

    private static void TimelineAxisPlacementPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TimelineUnit { IsLoaded: true } unit && e.NewValue is TimelineAxisPlacement placement)
        {
            unit._folded = !unit._folded;
            unit.AdjustAxisPlacement(placement, true); // unit._folded;
        }
    }

    private double GetTimelineAxisHeight(bool isAuxAxis)
    {
        if (ActualHeight is not 0 && _containerHeightFixed is 0)
        {
            _containerHeightFixed = ActualHeight;
            return isAuxAxis ? _containerHeightFixed : _containerHeightFixed - 45 is >= 0 and var value ? value : 0;
        }
        else
            return isAuxAxis ? _containerHeightFixed : _containerHeightFixed - 45 is >= 0 and var value ? value : 0;
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _leftIndicatorAxis = (GetTemplateChild("LeftIndicatorAxis") as Grid)!;
        _rightIndicatorAxis = (GetTemplateChild("RightIndicatorAxis") as Grid)!;
        _leftIconContainer = (GetTemplateChild("LeftIconContainer") as Grid)!;
        _rightIconContainer = (GetTemplateChild("RightIconContainer") as Grid)!;
        _contentPresenter = (GetTemplateChild("ContentPresenter") as ContentControl)!;

        _contentPresenter.Loaded += ContentContainerOnLoaded;

        SizeChanged += OnSizeChanged;
    }

    private bool FoldCriteria()
    {
        return FoldThreshold is not -1 && ActualWidth < FoldThreshold;
    }


    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (FoldCriteria())
        {
            if (TimelineAxisPlacement == FoldedDefaultPlacement)
            {
                AdjustAxisPlacement(TimelineAxisPlacement, true);
            }
            else
            {
                _differentDefaultAxisPlacement = true;
                TimelineAxisPlacement = FoldedDefaultPlacement;
            }
        }

        if (FoldThreshold is not -1 &&
            ActualWidth > FoldThreshold &&
            TimelineAxisPlacement != FoldedDefaultPlacement.Inverse())
        {
            if (!_differentDefaultAxisPlacement)
            {
                AdjustAxisPlacement(TimelineAxisPlacement, true);
            }
            else
            {
                TimelineAxisPlacement = FoldedDefaultPlacement.Inverse();
            }
        }
    }

    private void AdjustAxisPlacement(TimelineAxisPlacement placement, bool isFolding)
    {
        switch (placement)
        {
            case TimelineAxisPlacement.Left:
                _leftIconContainer.Visibility = Visibility.Visible;
                _rightIconContainer.Visibility = Visibility.Collapsed;

                _leftIndicatorAxis.Margin = isFolding
                    ? new Thickness(0, 5, 0, 5)
                    : new Thickness(0, 5, 0, 0);
                _leftIndicatorAxis.CornerRadius = isFolding
                    ? new CornerRadius(2)
                    : new CornerRadius(2, 2, 0, 0);

                if (isFolding)
                {
                    _rightIndicatorAxis.Visibility = Visibility.Collapsed;
                }
                else
                {
                    _leftIndicatorAxis.Visibility = Visibility.Visible;
                    _rightIndicatorAxis.Visibility = Visibility.Visible;
                    _rightIndicatorAxis.Margin = new Thickness(0, 0, 0, 5);
                    _rightIndicatorAxis.CornerRadius = new CornerRadius(0, 0, 2, 2);
                }

                _contentPresenter.HorizontalAlignment = HorizontalAlignment.Left;
                AdjustIndicatorAxisSize();
                break;
            case TimelineAxisPlacement.Right:
                _leftIconContainer.Visibility = Visibility.Collapsed;
                _rightIconContainer.Visibility = Visibility.Visible;
                _rightIndicatorAxis.Margin = isFolding
                    ? new Thickness(0, 5, 0, 5)
                    : new Thickness(0, 5, 0, 0);
                _rightIndicatorAxis.CornerRadius = isFolding
                    ? new CornerRadius(2)
                    : new CornerRadius(2, 2, 0, 0);

                if (isFolding)
                {
                    _leftIndicatorAxis.Visibility = Visibility.Collapsed;
                }
                else
                {
                    _rightIndicatorAxis.Visibility = Visibility.Visible;
                    _leftIndicatorAxis.Visibility = Visibility.Visible;
                    _leftIndicatorAxis.Margin = new Thickness(0, 0, 0, 5);
                    _leftIndicatorAxis.CornerRadius = new CornerRadius(0, 0, 2, 2);
                }

                _contentPresenter.HorizontalAlignment = HorizontalAlignment.Right;
                AdjustIndicatorAxisSize();
                break;
        }
    }

    private void ContentContainerOnLoaded(object sender, RoutedEventArgs e)
    {
        AdjustIndicatorAxisSize();
        AdjustAxisPlacement(TimelineAxisPlacement, true);
    }

    public void Reset()
    {
        AdjustIndicatorAxisSize();
        AdjustAxisPlacement(TimelineAxisPlacement, true);
    }

    private void AdjustIndicatorAxisSize()
    {
        _leftIndicatorAxis.Height = GetTimelineAxisHeight(TimelineAxisPlacement is TimelineAxisPlacement.Right);
        _rightIndicatorAxis.Height = GetTimelineAxisHeight(TimelineAxisPlacement is TimelineAxisPlacement.Left);
    }
}
