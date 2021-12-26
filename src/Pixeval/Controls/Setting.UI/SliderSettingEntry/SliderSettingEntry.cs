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
using Pixeval.Attributes;
using Pixeval.Controls.Setting.UI.UserControls;
using Pixeval.Misc;

namespace Pixeval.Controls.Setting.UI.SliderSettingEntry;

[TemplatePart(Name = PartEntryHeader, Type = typeof(SettingEntryHeader))]
[TemplatePart(Name = PartValueSlider, Type = typeof(Slider))]
[DependencyProperty("Maximum", typeof(double))]
[DependencyProperty("Minimum", typeof(double))]
[DependencyProperty("Value", typeof(double), nameof(OnValueChanged))]
public partial class SliderSettingEntry : SettingEntryBase
{
    private const string PartValueSlider = "ValueSlider";

    private Slider? _valueSlider;

    public SliderSettingEntry()
    {
        DefaultStyleKey = typeof(SliderSettingEntry);
    }

    private static void OnValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
    {
        ValueChanged(o, args.NewValue);
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