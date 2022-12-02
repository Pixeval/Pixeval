// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using CommunityToolkit.Mvvm.Input;

namespace Pixeval;

internal sealed partial class LoginWindow : Window
{
    public LoginWindow()
    {
        ViewModel = new LoginWindowViewModel(this);
    }

    public LoginWindowViewModel ViewModel { get; set; }
}

internal sealed partial class LoginWindowViewModel
{
    private readonly LoginWindow _loginWindow;

    public LoginWindowViewModel(LoginWindow loginWindow)
    {
        _loginWindow = loginWindow;
        _loginWindow.InitializeComponent();
    }

    [RelayCommand]
    private Task LoginAsync()
    {

    }

    [RelayCommand]
    private Task RegisterAsync()
    {

    }
}