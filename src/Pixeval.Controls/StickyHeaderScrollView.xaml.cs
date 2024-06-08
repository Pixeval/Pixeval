using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<FrameworkElement>("Header")]
[DependencyProperty<Brush>("HeaderBackGround")]
[DependencyProperty<FrameworkElement>("StickyContent")]
[DependencyProperty<double>("ScrollRatio")]
public sealed partial class StickyHeaderScrollView
{
    public StickyHeaderScrollView() => InitializeComponent();

    /// <summary>
    /// 这个方法获取滚动视图的滚动长度，与<see cref="Header"/>没有关系。
    /// 实际上<see cref="Header"/>和<see cref="StickyContent"/>合并为一个控件都是没问题的，
    /// 这样写只是方便外部的使用
    /// </summary>
    public event Func<double>? GetScrollableLength;

    public event Func<ScrollView?>? SetInnerScrollView;

    private void FrameworkElement_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var height = GetScrollableLength?.Invoke() ?? 0;
        ScrollDockPanel.Height = View.ActualHeight + height;
    }

    private void ScrollView_OnViewChanged(ScrollView sender, object args)
    {
        ScrollRatio = sender.VerticalOffset / sender.ScrollableHeight;
        RaiseSetInnerScrollView();
    }

    /// <summary>
    /// 当子<see cref="ScrollView"/>第一次加载出来时，可以调用这个方法来初始化设置
    /// </summary>
    public void RaiseSetInnerScrollView()
    {
        if (SetInnerScrollView?.Invoke() is not { } innerScrollView)
            return;
        if (ScrollRatio is 1)
        {
            innerScrollView.VerticalScrollBarVisibility = ScrollingScrollBarVisibility.Auto;
            innerScrollView.VerticalScrollMode = ScrollingScrollMode.Auto;
        }
        else
        {
            innerScrollView.VerticalScrollBarVisibility = ScrollingScrollBarVisibility.Hidden;
            innerScrollView.VerticalScrollMode = ScrollingScrollMode.Disabled;
        }
    }
}
