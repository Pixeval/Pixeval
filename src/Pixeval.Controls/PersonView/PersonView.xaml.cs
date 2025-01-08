// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using System;
using Microsoft.UI.Xaml.Media;
using Windows.System;
using Microsoft.UI.Xaml;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

/// <summary>
/// Used to display contributors, not illustrators
/// </summary>
[DependencyProperty<string>("PersonNickname")]
[DependencyProperty<string>("PersonName")]
[DependencyProperty<Uri>("PersonProfileNavigateUri")]
[DependencyProperty<ImageSource>("PersonPicture")]
public partial class PersonView
{
    public PersonView() => InitializeComponent();

    private async void ContentContainerOnClicked(object sender, RoutedEventArgs e) => await Launcher.LaunchUriAsync(PersonProfileNavigateUri);
}
