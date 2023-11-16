#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/IllustrationContainer.xaml.cs
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
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Options;
using Pixeval.Controls.IllustrationView;
using Pixeval.Util.UI;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<ObservableCollection<ICommandBarElement>>("PrimaryCommandsSupplements", DependencyPropertyDefaultValue.New)]
[DependencyProperty<ObservableCollection<ICommandBarElement>>("SecondaryCommandsSupplements", DependencyPropertyDefaultValue.New)]
[DependencyProperty<bool>("ShowCommandBar", "true", nameof(OnShowCommandBarChanged))]
[DependencyProperty<object>("Header")]
public sealed partial class IllustrationContainer
{
    public IllustrationViewOption IllustrationViewOption => App.AppViewModel.AppSetting.IllustrationViewOption;

    public ThumbnailDirection ThumbnailDirection => App.AppViewModel.AppSetting.ThumbnailDirection;

    public IllustrationContainer()
    {
        InitializeComponent();
        CommandBarElements = [];
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

    private static void OnShowCommandBarChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
        if (obj is IllustrationContainer { TopCommandBar: { } commandBar } && args.NewValue is bool v)
        {
            if (v)
            {
                commandBar.Show();
                commandBar.IsEnabled = true;
            }
            else
            {
                commandBar.Collapse();
                commandBar.IsEnabled = false;
            }
        }
    }

    public IllustrationViewViewModel ViewModel => IllustrationView.ViewModel;

    /// <summary>
    ///     The command elements that will appear at the left of the <see cref="TopCommandBar" />
    /// </summary>
    public ObservableCollection<UIElement> CommandBarElements { get; }

    private void IllustrationContainer_OnLoaded(object sender, RoutedEventArgs e)
    {
        _ = IllustrationView.Focus(FocusState.Programmatic);
    }

    public void ScrollToTop()
    {
        _ = IllustrationView.ScrollViewer.ChangeView(null, 0, null, false);
    }
}
