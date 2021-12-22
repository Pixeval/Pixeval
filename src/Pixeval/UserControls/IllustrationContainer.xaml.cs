#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/IllustrationContainer.xaml.cs
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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Misc;

namespace Pixeval.UserControls;

[DependencyProperty("PrimaryCommandsSupplements", typeof(ObservableCollection<ICommandBarElement>), DefaultValue = "new ObservableCollection<ICommandBarElement>()")]
[DependencyProperty("SecondaryCommandsSupplements", typeof(ObservableCollection<ICommandBarElement>), DefaultValue = "new ObservableCollection<ICommandBarElement>()")]
public sealed partial class IllustrationContainer
{
    public IllustrationContainer()
    {
        InitializeComponent();
        CommandBarElements = new ObservableCollection<UIElement>();
        CommandBarElements.CollectionChanged += (_, args) =>
        {
            switch (args)
            {
                case { Action: NotifyCollectionChangedAction.Add }:
                    if (args is { NewItems: not null })
                    {
                        foreach (UIElement argsNewItem in args.NewItems)
                        {
                            TopCommandBar.CommandBarElements.Add(argsNewItem);
                        }
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        };
    }

    public IllustrationGridViewModel ViewModel => IllustrationGrid.ViewModel;


    /// <summary>
    ///     The command elements that will appear at the left of the <see cref="TopCommandBar" />
    /// </summary>
    public ObservableCollection<UIElement> CommandBarElements { get; }

    private void IllustrationContainer_OnLoaded(object sender, RoutedEventArgs e)
    {
        IllustrationGrid.Focus(FocusState.Programmatic);
    }

    public void ScrollToTop()
    {
        IllustrationGrid.IllustrationGridView.FindDescendant<ScrollViewer>()?.ChangeView(null, 0, null, false);
    }
}