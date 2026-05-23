// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Pixeval.ViewModels.Search;

public partial class OptionalDateSearchOptionViewModel : ViewModelBase
{
    public OptionalDateSearchOptionViewModel(bool isEnabled = false, DateTime? value = null)
    {
        IsEnabled = isEnabled;
        Value = value;
    }

    [ObservableProperty]
    public partial bool IsEnabled { get; set; }

    [ObservableProperty]
    public partial DateTime? Value { get; set; }

    public DateTimeOffset? ToNullableDateTimeOffset()
    {
        if (!IsEnabled || Value is not { } date)
            return null;

        return new DateTimeOffset(DateTime.SpecifyKind(date.Date, DateTimeKind.Local));
    }
}
