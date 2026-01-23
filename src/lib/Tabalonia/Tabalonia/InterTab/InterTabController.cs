using Tabalonia.Controls;

namespace Tabalonia.InterTab;

/// <summary>
/// Configuration for inter-tab (cross-window) dragging behavior.
/// Set this on <see cref="TabsControl.InterTabController"/> to enable tear-off and merge.
/// </summary>
public class InterTabController
{
    /// <summary>
    /// How far (in DIPs) the pointer must move horizontally beyond the tab header area
    /// before a tear-off is initiated. Default is 8.
    /// </summary>
    public double HorizontalPopoutGrace { get; set; } = 8;

    /// <summary>
    /// How far (in DIPs) the pointer must move vertically beyond the tab header area
    /// before a tear-off is initiated. Default is 50.
    /// </summary>
    public double VerticalPopoutGrace { get; set; } = 50;

    /// <summary>
    /// The client responsible for creating new windows and handling empty tab controls.
    /// Defaults to <see cref="DefaultInterTabClient"/>.
    /// </summary>
    public IInterTabClient InterTabClient { get; set; } = new DefaultInterTabClient();

    /// <summary>
    /// An optional partition key. Only <see cref="TabsControl"/> instances with the same
    /// partition value can exchange tabs via drag. <c>null</c> matches other <c>null</c> partitions.
    /// </summary>
    public string? Partition { get; set; }
}
