#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/NotifyOnLoadedCalendarDatePicker.cs
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
using Microsoft.UI.Xaml.Controls;

namespace Pixeval.Controls.NotifyOnLoadedCalendarDatePicker;

public class NotifyOnLoadedCalendarDatePicker : CalendarDatePicker
{
    private TypedEventHandler<CalendarDatePicker, CalendarDatePickerDateChangedEventArgs>? _dateChangedWhenLoaded;

    public NotifyOnLoadedCalendarDatePicker()
    {
        DefaultStyleKey = typeof(NotifyOnLoadedCalendarDatePicker);
        DateChanged += (sender, args) =>
        {
            if (IsLoaded)
            {
                _dateChangedWhenLoaded?.Invoke(sender, args);
            }
        };
    }

    public event TypedEventHandler<CalendarDatePicker, CalendarDatePickerDateChangedEventArgs> DateChangedWhenLoaded
    {
        add => _dateChangedWhenLoaded += value;
        remove => _dateChangedWhenLoaded -= value;
    }
}