// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;

namespace Pixeval.ViewModels;

public partial class LoginPageViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial Symbol LoginIcon { get; private set; } = Symbol.Person;

    [RelayCommand]
    public void ChangeIcon()
    {
        LoginIcon = LoginIcon is Symbol.Person ? Symbol.PersonAdd : Symbol.Person;
    }
}
