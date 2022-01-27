#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/SwitchSettingEntry.cs
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

using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Attributes;
using Pixeval.Controls.Setting.UI.UserControls;


namespace Pixeval.Controls.Setting.UI.SwitchSettingEntry;

/// <summary>
/// The <see cref="SwitchSettingEntry"/> is a preset setting entry with a <see cref="ToggleSwitch"/> as its content, user can control over
/// it by <see cref="IsOn"/> property, two-way binding is supported
/// </summary>
[TemplatePart(Name = PartEntryHeader, Type = typeof(SettingEntryHeader))]
[TemplatePart(Name = PartSwitch, Type = typeof(ToggleSwitch))]
[DependencyProperty("IsOn", typeof(bool), nameof(OnIsOnChanged))]
public partial class SwitchSettingEntry : SettingEntryBase
{
    private const string PartSwitch = "Switch";

    private ToggleSwitch? _switch;

    private TypedEventHandler<SwitchSettingEntry, RoutedEventArgs>? _toggled;

    public SwitchSettingEntry()
    {
        DefaultStyleKey = typeof(SwitchSettingEntry);
    }

    private static void OnIsOnChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
    {
        IsOnChanged(o, args.NewValue);
    }

    public event TypedEventHandler<SwitchSettingEntry, RoutedEventArgs> Toggled
    {
        add => _toggled += value;
        remove => _toggled -= value;
    }

    private static void IsOnChanged(DependencyObject d, object newValue)
    {
        if (d is SwitchSettingEntry { _switch: { } sh })
        {
            sh.IsOn = (bool) newValue;
        }
    }

    private void SwitchOnToggled(object sender, RoutedEventArgs e)
    {
        IsOn = _switch!.IsOn;
        _toggled?.Invoke(this, e);
    }

    protected override void Update()
    {
        IsOnChanged(this, IsOn);
    }

    protected override void OnApplyTemplate()
    {
        if (_switch is not null)
        {
            _switch.Toggled -= SwitchOnToggled;
        }

        if ((_switch = GetTemplateChild(PartSwitch) as ToggleSwitch) is not null)
        {
            _switch.Toggled += SwitchOnToggled;
        }

        base.OnApplyTemplate();
    }
}