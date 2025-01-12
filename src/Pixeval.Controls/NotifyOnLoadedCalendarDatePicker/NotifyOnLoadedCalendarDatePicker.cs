// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

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
