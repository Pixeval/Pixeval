using System;
using Microsoft.UI.Xaml;

namespace Pixeval.Util.UI
{
    public static class ZoomHelper
    {
        private static double _oldValue = 1;

        public static DependencyProperty ScaleProperty = DependencyProperty.RegisterAttached(
            "Scale", 
            typeof(double),
            typeof(ZoomHelper),
            PropertyMetadata.Create(1, ScalePropertyChangedCallback));

        private static void ScalePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (_onZoomValueChanged is not null)
            {
                foreach (var invocation in _onZoomValueChanged.GetInvocationList())
                {
                    var @delegate = (EventHandler<ZoomValueChangedEventArg>) invocation;
                    var delta = (double) e.NewValue - _oldValue;
                    @delegate(d, new ZoomValueChangedEventArg(_oldValue, delta));
                    _oldValue += delta;
                }
            }
        }

        public static double GetScale(DependencyObject obj)
        {
            return (double) obj.GetValue(ScaleProperty);
        }

        public static void SetScale(DependencyObject obj, double value)
        {
            obj.SetValue(ScaleProperty, value);
        }
        
        private static EventHandler<ZoomValueChangedEventArg>? _onZoomValueChanged; 

        public static event EventHandler<ZoomValueChangedEventArg> OnZoomValueChanged
        {
            add => _onZoomValueChanged += value;
            remove
            {
                if (_onZoomValueChanged != null)
                {
                    _onZoomValueChanged -= value;
                }
            }
        }
    }

    public readonly struct ZoomValueChangedEventArg
    {
        public readonly double Delta;

        public readonly double OldValue;

        public ZoomValueChangedEventArg(double oldValue, double delta)
        {
            Delta = delta;
            OldValue = oldValue;
        }
    }
}