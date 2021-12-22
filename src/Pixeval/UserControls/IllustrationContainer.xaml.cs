﻿#region Copyright (c) Pixeval/Pixeval

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

namespace Pixeval.UserControls;

public sealed partial class IllustrationContainer
{
    public static DependencyProperty PrimaryCommandSupplementsProperty = DependencyProperty.Register(
        nameof(PrimaryCommandsSupplements),
        typeof(ObservableCollection<ICommandBarElement>),
        typeof(IllustrationContainer),
        PropertyMetadata.Create(
            new ObservableCollection<ICommandBarElement>()));

    public static DependencyProperty SecondaryCommandsSupplementsProperty = DependencyProperty.Register(
        nameof(SecondaryCommandsSupplements),
        typeof(ObservableCollection<ICommandBarElement>),
        typeof(IllustrationContainer),
        PropertyMetadata.Create(
            new ObservableCollection<ICommandBarElement>()));

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

    public static readonly DependencyProperty GridHeaderProperty = DependencyProperty.Register(
        nameof(GridHeader),
        typeof(object),
        typeof(IllustrationContainer),
        PropertyMetadata.Create(DependencyProperty.UnsetValue));

    public object GridHeader
    {
        get => (object)GetValue(GridHeaderProperty);
        set => SetValue(GridHeaderProperty, value);
    }

    public ObservableCollection<ICommandBarElement> PrimaryCommandsSupplements
    {
        get => (ObservableCollection<ICommandBarElement>)GetValue(PrimaryCommandSupplementsProperty);
        set => SetValue(PrimaryCommandSupplementsProperty, value);
    }

    public ObservableCollection<ICommandBarElement> SecondaryCommandsSupplements
    {
        get => (ObservableCollection<ICommandBarElement>)GetValue(SecondaryCommandsSupplementsProperty);
        set => SetValue(SecondaryCommandsSupplementsProperty, value);
    }

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