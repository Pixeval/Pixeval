#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IIllustrationView.cs
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

using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Pixeval.UserControls.IllustrationView;

public interface IIllustrationView
{ 
    FrameworkElement SelfIllustrationView { get; }

    IllustrationViewViewModel ViewModel { get; }

    ScrollViewer ScrollViewer { get; }

    /// <summary>
    /// Loads items continuously until the elements fill the <see cref="IIllustrationView"/>,
    /// i.e., more elements than those in the viewport of the <see cref="IIllustrationView"/>
    /// </summary>
    Task TryFillClientAreaAsync();

    UIElement? GetItemContainer(IllustrationViewModel viewModel);
}