#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/LoginPage.xaml.cs
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

using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Messages;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using System;
using System.Threading.Tasks;

namespace Pixeval.Pages.Login;


internal sealed partial class LoginPage
{
    public LoginPage()
    {
    }

    public LoginPageViewModel ViewModel { get; set; }


    private async void ButtonLogin_Click(object sender, RoutedEventArgs e)
    {
        LoginPopup.IsOpen = true;
    }

    private async void ButtonRegister_Click(object sender, RoutedEventArgs e)
    {
        RegisterPopup.IsOpen = true;
    }
}