#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/SliderSettingEntry.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Pixeval.Controls.Setting.UI.UserControls;

namespace Pixeval.Controls.Setting.UI.SliderSettingEntry;

[TemplatePart(Name = PartEntryHeader, Type = typeof(SettingEntryHeader))]
[TemplatePart(Name = PartValueSlider, Type = typeof(Slider))]
public class SliderSettingEntry : SettingEntryBase
{
    private const string PartValueSlider = "ValueSlider";

    public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
        nameof(Maximum),
        typeof(double),
        typeof(SliderSettingEntry),
        PropertyMetadata.Create(DependencyProperty.UnsetValue));

    public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
        nameof(Minimum),
        typeof(double),
        typeof(SliderSettingEntry),
        PropertyMetadata.Create(DependencyProperty.UnsetValue));

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(double),
        typeof(SliderSettingEntry),
        PropertyMetadata.Create(DependencyProperty.UnsetValue, (o, args) => ValueChanged(o, args.NewValue)));

    private Slider? _valueSlider;

    public SliderSettingEntry()
    {
        DefaultStyleKey = typeof(SliderSettingEntry);
    }

    public double Maximum
    {
        get => (double) GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public double Minimum
    {
        get => (double) GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

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