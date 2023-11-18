#region Copyright (c) Pixeval/Pixeval.Controls
// GPL v3 License
// 
// Pixeval/Pixeval.Controls
// Copyright (c) 2023 Pixeval.Controls/PersonView.xaml.cs
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
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.System;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

/// <summary>
/// Used to display contributors, not illustrators
/// </summary>
[DependencyProperty<string>("PersonNickname")]
[DependencyProperty<string>("PersonName")]
[DependencyProperty<Uri>("PersonProfileNavigateUri")]
[DependencyProperty<ImageSource>("PersonPicture")]
public partial class PersonView : UserControl
{
    public PersonView()
    {
        InitializeComponent();
    }

    private async void ContentContainerOnTapped(object sender, TappedRoutedEventArgs e)
    {
        _ = await Launcher.LaunchUriAsync(PersonProfileNavigateUri);
    }
}
