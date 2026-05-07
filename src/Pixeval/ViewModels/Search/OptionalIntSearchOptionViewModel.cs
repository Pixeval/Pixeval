// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using CommunityToolkit.Mvvm.ComponentModel;

namespace Pixeval.ViewModels.Search;

public partial class OptionalIntSearchOptionViewModel : ViewModelBase
{
    public OptionalIntSearchOptionViewModel(bool isEnabled = false, int value = 0)
    {
        IsEnabled = isEnabled;
        Value = value;
    }

    [ObservableProperty]
    public partial bool IsEnabled { get; set; }

    [ObservableProperty]
    public partial int Value { get; set; }

    public int? NullableInt => IsEnabled ? Value : null;
}
