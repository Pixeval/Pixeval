#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/IllustrationContainer.cs
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

using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Misc;

namespace Pixeval.Controls;

public class IllustrationContainer : UserControl, IScrollViewProvider
{
    public ObservableCollection<UIElement> CommandBarElements => EntryContainer.CommandBarElements;

    public ObservableCollection<ICommandBarElement> PrimaryCommandsSupplements => EntryContainer.PrimaryCommandsSupplements;

    public ObservableCollection<ICommandBarElement> SecondaryCommandsSupplements => EntryContainer.SecondaryCommandsSupplements;

    public IllustrationContainer() =>
        Content = EntryContainer = new()
        {
            EntryView = IllustrationView = new()
            {
                LayoutType = App.AppViewModel.AppSettings.ItemsViewLayoutType,
                ThumbnailDirection = App.AppViewModel.AppSettings.ThumbnailDirection
            }
        };

    public EntryContainer EntryContainer { get; }

    public IllustrationView IllustrationView { get; }

    public IllustrationViewViewModel ViewModel => IllustrationView.ViewModel;

    public ScrollView ScrollView => IllustrationView.ScrollView;
}
