using Tabalonia.Controls;

namespace Tabalonia.Panels;

public sealed class TopPanel : Panel
{
    private PartsStruct? _parts;
    private int _lastChildrenCount;

    private PartsStruct? TryGetParts()
    {
        if (_parts.HasValue && _lastChildrenCount == Children.Count)
            return _parts.Value;

        if (Children.Count < 7)
            return null;

        var leftContent = Children.FirstOrDefault(a => a.Name == "PART_LeftHeaderContent");
        var leftThumb = Children.FirstOrDefault(a => a.Name == "PART_LeftDragWindowThumb");
        var tabs = Children.FirstOrDefault(a => a.Name == "PART_ItemsPresenter");
        var addBtn = Children.FirstOrDefault(a => a.Name == "PART_AddItemButton");
        var midThumb = Children.FirstOrDefault(a => a.Name == "PART_MiddleDragWindowThumb");
        var rightThumb = Children.FirstOrDefault(a => a.Name == "PART_RightDragWindowThumb");
        var rightContent = Children.FirstOrDefault(a => a.Name == "PART_RightHeaderContent");

        if (leftContent is null || leftThumb is null || tabs is null || addBtn is null
            || midThumb is null || rightThumb is null || rightContent is null)
            return null;

        _lastChildrenCount = Children.Count;
        _parts = new PartsStruct
        {
            LeftHeaderContent = leftContent,
            LeftDragWindowThumb = leftThumb,
            TabsControl = tabs,
            AddTabButton = addBtn,
            MiddleDragWindowThumb = midThumb,
            RightDragWindowThumb = rightThumb,
            RightHeaderContent = rightContent,
        };

        return _parts.Value;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        // Defer a layout pass so that children are ready when we first try to resolve parts
        Dispatcher.UIThread.Post(InvalidateMeasure, DispatcherPriority.Loaded);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (Children.Count == 0)
            return new();

        var parts = TryGetParts();
        if (parts is null)
            return new();

        var p = parts.Value;
        var height = 0d;
        var width = 0d;
        var availableWidth = availableSize.Width;
        var availableHeight = availableSize.Height;

        MeasureControl(p.LeftDragWindowThumb);
        MeasureControl(p.RightDragWindowThumb);
        MeasureControl(p.LeftHeaderContent);
        MeasureControl(p.RightHeaderContent);
        MeasureControl(p.AddTabButton);
        MeasureControl(p.TabsControl);
        MeasureControl(p.MiddleDragWindowThumb);

        width = Math.Max(availableWidth, width);

        return new Size(width, height);

        void MeasureControl(Control control)
        {
            var currentAvailableWidth = availableWidth - width;
            control.Measure(new Size(currentAvailableWidth, availableHeight));
            width += control.DesiredSize.Width;
            height = Math.Max(height, control.DesiredSize.Height);
        }
    }


    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Children.Count is 0)
            return finalSize;

        var parts = TryGetParts();
        if (parts is null)
            return finalSize;

        var p = parts.Value;
        var partsWidth = new PartsWidth
        {
            LeftDragWindowThumb = p.LeftDragWindowThumb.DesiredSize.Width,
            LeftHeaderContent = p.LeftHeaderContent.DesiredSize.Width,
            TabsControl = p.TabsControl.DesiredSize.Width,
            AddTabButton = p.AddTabButton.DesiredSize.Width,
            MiddleDragWindowThumb = p.MiddleDragWindowThumb.DesiredSize.Width,
            RightHeaderContent = p.RightHeaderContent.DesiredSize.Width,
            RightDragWindowThumb = p.RightDragWindowThumb.DesiredSize.Width,
        };

        var tabsHeight = Math.Max(p.TabsControl.DesiredSize.Height, finalSize.Height);

        var withoutTabsWidth =
            partsWidth.LeftDragWindowThumb
            + partsWidth.LeftHeaderContent
            + partsWidth.AddTabButton
            + partsWidth.MiddleDragWindowThumb
            + partsWidth.RightHeaderContent
            + partsWidth.RightDragWindowThumb;
        
        var availableTabsWidth = finalSize.Width - withoutTabsWidth;

        if (partsWidth.TabsControl < availableTabsWidth)
        {
            ArrangeWhenTabsFit(p, partsWidth, tabsHeight, finalSize.Width);
            return finalSize;
        }

        ArrangeWhenTabsUnfit(p, partsWidth, tabsHeight, availableTabsWidth);
        return finalSize;
    }

    /// <summary>
    /// |leftThumb|leftContent|tab1    |tab2    |addTabButton|---middleThumb---|rightContent|rightThumb|
    /// </summary>
    private static void ArrangeWhenTabsFit(PartsStruct parts, PartsWidth widths, double tabsHeight, double finalWidth)
    {
        double x = 0;
        parts.LeftDragWindowThumb.Arrange(new Rect(x, 0, widths.LeftDragWindowThumb, tabsHeight));
        x += widths.LeftDragWindowThumb;
        parts.LeftHeaderContent.Arrange(new Rect(x, 0, widths.LeftHeaderContent, tabsHeight));
        x += widths.LeftHeaderContent;
        parts.TabsControl.Arrange(new Rect(x, 0, widths.TabsControl, tabsHeight));
        x += widths.TabsControl;

        ArrangeCenterVertical(parts.AddTabButton, x, tabsHeight);
        x += widths.AddTabButton;

        var availableSpaceWidth =
            finalWidth
            - widths.LeftDragWindowThumb
            - widths.LeftHeaderContent
            - widths.TabsControl
            - widths.AddTabButton
            - widths.RightHeaderContent
            - widths.RightDragWindowThumb;

        parts.MiddleDragWindowThumb.Arrange(new Rect(x, 0, availableSpaceWidth, tabsHeight));
        x += availableSpaceWidth;
        parts.RightHeaderContent.Arrange(new Rect(x, 0, widths.RightHeaderContent, tabsHeight));
        x += widths.RightHeaderContent;
        parts.RightDragWindowThumb.Arrange(new Rect(x, 0, widths.RightDragWindowThumb, tabsHeight));
    }

    /// <summary>
    /// |leftThumb|leftContent|tab1|tab2|tab3|tab4|tab5|tab6|tab7|addTabButton|rightContent|rightThumb|
    /// </summary>
    private static void ArrangeWhenTabsUnfit(PartsStruct parts, PartsWidth widths, double tabsHeight,
        double availableTabsWidth)
    {
        double x = 0;
        parts.LeftDragWindowThumb.Arrange(new Rect(x, 0, widths.LeftDragWindowThumb, tabsHeight));
        x += widths.LeftDragWindowThumb;
        parts.LeftHeaderContent.Arrange(new Rect(x, 0, widths.LeftHeaderContent, tabsHeight));
        x += widths.LeftHeaderContent;

        parts.TabsControl.Arrange(new Rect(x, 0, availableTabsWidth, tabsHeight));
        x += availableTabsWidth;

        ArrangeCenterVertical(parts.AddTabButton, x, tabsHeight);
        x += widths.AddTabButton;

        parts.MiddleDragWindowThumb.Arrange(new Rect(x, 0, 0, tabsHeight));
        parts.RightHeaderContent.Arrange(new Rect(x, 0, widths.RightHeaderContent, tabsHeight));
        x += widths.RightHeaderContent;
        parts.RightDragWindowThumb.Arrange(new Rect(x, 0, widths.RightDragWindowThumb, tabsHeight));
    }

    private static void ArrangeCenterVertical(Layoutable control, double x, double fullHeight)
    {
        double width = control.DesiredSize.Width;
        double height = control.DesiredSize.Height;

        double y = (fullHeight - height) / 2;

        control.Arrange(new Rect(x, y, width, height));
    }

    private readonly struct PartsStruct
    {
        public Control LeftDragWindowThumb { get; init; }
        public Control LeftHeaderContent { get; init; }
        public Control TabsControl { get; init; }
        public Control AddTabButton { get; init; }
        public Control MiddleDragWindowThumb { get; init; }
        public Control RightHeaderContent { get; init; }
        public Control RightDragWindowThumb { get; init; }
    }

    private readonly ref struct PartsWidth
    {
        public double LeftDragWindowThumb { get; init; }
        public double LeftHeaderContent { get; init; }
        public double TabsControl { get; init; }
        public double AddTabButton { get; init; }
        public double MiddleDragWindowThumb { get; init; }
        public double RightHeaderContent { get; init; }
        public double RightDragWindowThumb { get; init; }
    }
}
