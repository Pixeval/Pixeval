#region Copyright (c) Pixeval/Pixeval.Controls
// GPL v3 License
// 
// Pixeval/Pixeval.Controls
// Copyright (c) 2025 Pixeval.Controls/IStructuralDisposalCompleter.cs
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
using System.Collections.Generic;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;

namespace Pixeval.Controls;

public interface IStructuralDisposalCompleter : IPageDisposalCompleter
{
    IStructuralDisposalCompleter? ParentCompleter => FindParentCompleterRecursively(this as FrameworkElement);

    List<Action> ChildrenCompletes { get; }

    bool CompleterRegistered { get; set; }

    bool CompleterDisposed { get; set; }

    void CompleteDisposalRecursively()
    {
        if (CompleterDisposed) return;
        CompleterDisposed = true;
        CompleteDisposal();
        foreach (var childrenComplete in ChildrenCompletes)
        {
            childrenComplete();
        }
    }

    /// <summary>
    /// Call this after the UI is loaded
    /// </summary>
    public void Hook()
    {
        if (CompleterRegistered) return;
        CompleterRegistered = true;
        ParentCompleter?.ChildrenCompletes.Add(CompleteDisposalRecursively);
    }

    private static IStructuralDisposalCompleter? FindParentCompleterRecursively(DependencyObject? uiElement)
    {
        return uiElement?.FindAscendant<FrameworkElement>(x => x is IStructuralDisposalCompleter) as IStructuralDisposalCompleter;
    }
}
