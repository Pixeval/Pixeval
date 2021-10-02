using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Pixeval.Controls.Setting.UI.UserControls;

namespace Pixeval.Controls.Setting.UI.SliderSettingEntry
{
    [TemplatePart(Name = PartEntryHeader, Type = typeof(SettingEntryHeader))]
    [TemplatePart(Name = PartValueSlider, Type = typeof(Slider))]
    public class SliderSettingEntry : SettingEntryBase
    {
        private const string PartValueSlider = "ValueSlider";

        private Slider? _valueSlider;

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            nameof(Maximum), 
            typeof(double),
            typeof(SliderSettingEntry), 
            PropertyMetadata.Create(DependencyProperty.UnsetValue));

        public double Maximum
        {
            get => (double) GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            nameof(Minimum),
            typeof(double),
            typeof(SliderSettingEntry),
            PropertyMetadata.Create(DependencyProperty.UnsetValue));

        public double Minimum
        {
            get => (double) GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(SliderSettingEntry),
            PropertyMetadata.Create(DependencyProperty.UnsetValue, (o, args) => ValueChanged(o, args.NewValue)));

        public double Value
        {
            get => (double) GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        private static void ValueChanged(DependencyObject d, object newValue)
        {
            if (d is SliderSettingEntry { _valueSlider: { } slider } && newValue is double value)
            {
                slider.Value = value;
            }
        }

        private void ValueSliderOnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Value = e.NewValue;
        }

        public SliderSettingEntry()
        {
            DefaultStyleKey = typeof(SliderSettingEntry);
        }

        protected override void OnApplyTemplate()
        {
            if (_valueSlider is not null)
            {
                _valueSlider.ValueChanged -= ValueSliderOnValueChanged;
            }

            if ((_valueSlider = GetTemplateChild(PartValueSlider) as Slider) is not null)
            {
                _valueSlider.ValueChanged += ValueSliderOnValueChanged;
            }

            base.OnApplyTemplate();
        }

        protected override void Update()
        {
            ValueChanged(this, Value);
        }
    }
}