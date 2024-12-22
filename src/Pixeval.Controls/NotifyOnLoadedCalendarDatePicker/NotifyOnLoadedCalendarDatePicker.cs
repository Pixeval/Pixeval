#region Copyright (c) Pixeval/Pixeval.Controls
// GPL v3 License
// 
// Pixeval/Pixeval.Controls
// Copyright (c) 2023 Pixeval.Controls/NotifyOnLoadedCalendarDatePicker.cs
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

using System;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;
using WinUI3Utilities;

namespace Pixeval.Controls;

public partial class NotifyOnLoadedCalendarDatePicker : CalendarDatePicker
{
    public NotifyOnLoadedCalendarDatePicker()
    {
        DefaultStyleKey = typeof(NotifyOnLoadedCalendarDatePicker);
        DateChanged += (sender, args) =>
        {
            var picker = sender.To<NotifyOnLoadedCalendarDatePicker>();
            if (picker.Date is null)
                picker.Date = args.OldDate;
            else if (IsLoaded && picker._oldDate != picker.Date)
            {
                DateChangedWhenLoaded?.Invoke(sender, args);
                picker._oldDate = picker.Date;
            }
        };

        Loaded += (sender, args) =>
        {
            var picker = sender.To<NotifyOnLoadedCalendarDatePicker>();
            picker._oldDate = picker.Date;
        };
    }

    private DateTimeOffset? _oldDate;

    public event TypedEventHandler<CalendarDatePicker, CalendarDatePickerDateChangedEventArgs>? DateChangedWhenLoaded;
}
