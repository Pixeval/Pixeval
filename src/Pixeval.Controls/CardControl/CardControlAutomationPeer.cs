#region Copyright (c) Pixeval/Pixeval.Controls
// GPL v3 License
// 
// Pixeval/Pixeval.Controls
// Copyright (c) 2023 Pixeval.Controls/CardControlAutomationPeer.cs
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

using Microsoft.UI.Xaml.Automation.Peers;

namespace Pixeval.Controls;

/// <summary>
/// AutomationPeer for CardControl
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CardControl"/> class.
/// </remarks>
/// <param name="owner">CardControl</param>
public class CardControlAutomationPeer(CardControl owner) : FrameworkElementAutomationPeer(owner)
{
    /// <summary>
    /// Gets the control type for the element that is associated with the UI Automation peer.
    /// </summary>
    /// <returns>The control type.</returns>
    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.Group;
    }

    /// <summary>
    /// Called by GetClassName that gets a human readable name that, in addition to AutomationControlType,
    /// differentiates the control represented by this AutomationPeer.
    /// </summary>
    /// <returns>The string that contains the name.</returns>
    protected override string GetClassNameCore()
    {
        var classNameCore = Owner.GetType().Name;
#if DEBUG_AUTOMATION
            System.Diagnostics.Debug.WriteLine("CardControlAutomationPeer.GetClassNameCore returns " + classNameCore);
#endif
        return classNameCore;
    }
}
