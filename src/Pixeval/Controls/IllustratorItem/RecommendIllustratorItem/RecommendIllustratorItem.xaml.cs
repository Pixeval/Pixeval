#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/RecommendIllustratorProfile.xaml.cs
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

using Microsoft.UI.Xaml;
using System;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<RecommendIllustratorItemViewModel>("ViewModel", propertyChanged: nameof(OnViewModelChanged))]
public sealed partial class RecommendIllustratorItem : IViewModelControl
{
    object IViewModelControl.ViewModel => ViewModel;

    public event Action<RecommendIllustratorItem, RecommendIllustratorItemViewModel>? ViewModelChanged;

    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d as RecommendIllustratorItem is { } item)
        {
            item.ViewModelChanged?.Invoke(item, item.ViewModel);
        }
    }

    public RecommendIllustratorItem() => InitializeComponent();
}
