using Tabalonia.Controls;

namespace Tabalonia.InterTab;

/// <summary>
/// Provides the strategy for creating new windows when tabs are torn off,
/// and for handling scenarios where all tabs have been removed from a window.
/// </summary>
public interface IInterTabClient
{
    /// <summary>
    /// Called when a tab is torn off and a new host window is needed.
    /// The returned window should contain a <see cref="TabsControl"/> ready to receive the dragged tab.
    /// The window should NOT be shown yet — the caller will position and show it.
    /// </summary>
    TabHost GetNewHost(InterTabController controller, TabHost host);
}
