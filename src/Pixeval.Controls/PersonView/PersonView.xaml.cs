// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using System;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.System;

namespace Pixeval.Controls;

/// <summary>
/// Used to display contributors, not illustrators
/// </summary>
public partial class PersonView
{
    [GeneratedDependencyProperty]
    public partial string? PersonNickname { get; set; }

    [GeneratedDependencyProperty]
    public partial string? PersonName { get; set; }

    [GeneratedDependencyProperty]
    public partial Uri? PersonProfileNavigateUri { get; set; }

    [GeneratedDependencyProperty]
    public partial ImageSource? PersonPicture { get; set; }

    public PersonView() => InitializeComponent();

    private async void ContentContainerOnClicked(object sender, RoutedEventArgs e) => await Launcher.LaunchUriAsync(PersonProfileNavigateUri);
}
