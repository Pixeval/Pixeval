#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/PersonView.cs
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
using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Pixeval.Attributes;
using Pixeval.Controls.Card;


namespace Pixeval.Controls.PersonView;

[TemplatePart(Name = PartContentContainer, Type = typeof(CardControl))]
[DependencyProperty<string>("PersonNickname")]
[DependencyProperty<string>("PersonName")]
[DependencyProperty<Uri>("PersonProfileNavigateUri")]
[DependencyProperty<ImageSource>("PersonPicture")]
public partial class PersonView : Control
{
    private const string PartContentContainer = "ContentContainer";

    private CardControl? _contentContainer;

    public PersonView()
    {
        DefaultStyleKey = typeof(PersonView);
    }

    protected override void OnApplyTemplate()
    {
        if (_contentContainer is not null)
        {
            _contentContainer.PointerEntered -= ContentContainerOnPointerEntered;
            _contentContainer.PointerExited -= ContentContainerOnPointerExited;
            _contentContainer.Tapped -= ContentContainerOnTapped;
        }

        if ((_contentContainer = GetTemplateChild(PartContentContainer) as CardControl) is not null)
        {
            _contentContainer.PointerEntered += ContentContainerOnPointerEntered;
            _contentContainer.PointerExited += ContentContainerOnPointerExited;
            _contentContainer.Tapped += ContentContainerOnTapped;
        }

        base.OnApplyTemplate();
    }

    private async void ContentContainerOnTapped(object sender, TappedRoutedEventArgs e)
    {
        await Launcher.LaunchUriAsync(PersonProfileNavigateUri);
    }

    private void ContentContainerOnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        _contentContainer!.Background = (Brush)Application.Current.Resources["CardBackground"];
    }

    private void ContentContainerOnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        _contentContainer!.Background = (Brush)Application.Current.Resources["ActionableCardPointerOverBackground"];
    }
}