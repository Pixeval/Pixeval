// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using CommunityToolkit.Mvvm.ComponentModel;

namespace Pixeval.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _greeting = "Welcome to Avalonia!";
}
