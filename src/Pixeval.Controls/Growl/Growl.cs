using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace Pixeval.Controls;

public static partial class Growl
{
    private static readonly Dictionary<ulong, Panel> _panelDic = [];

    public static int MaxGrowlCount { get; set; } = 2;

    public static Panel? GrowlPanel { get; set; }

    public static void Register(ulong token, Panel? panel)
    {
        if (token is 0 || panel is null)
            return;
        _panelDic[token] = panel;
    }

    public static void Unregister(Panel? panel)
    {
        if (panel is null)
            return;
        var first = _panelDic.FirstOrDefault(item => ReferenceEquals(panel, item.Value));
        if (first.Key is not 0)
        {
            _ = _panelDic.Remove(first.Key);
        }
    }

    private static void SetDefaultPanelTransition(Panel panel)
    {
        if (panel.ChildrenTransitions != null && panel.ChildrenTransitions.Count == 0)
        {
            var growlTransition = GetGrowlEnterTransition(panel);
            SetPanelTransition(growlTransition, panel);
        }
    }

    private static void SetPanelTransition(GrowlTransition growlTransition, Panel? panel)
    {
        var transitions = new TransitionCollection();
        switch (growlTransition)
        {
            case GrowlTransition.AddDeleteThemeTransition:
                transitions.Add(new AddDeleteThemeTransition());
                break;
            case GrowlTransition.ContentThemeTransition:
                transitions.Add(new ContentThemeTransition());
                break;
            case GrowlTransition.EdgeUIThemeTransition:
                transitions.Add(new EdgeUIThemeTransition());
                break;
            case GrowlTransition.EntranceThemeTransition:
                transitions.Add(new EntranceThemeTransition());
                break;
            case GrowlTransition.NavigationThemeTransition:
                transitions.Add(new NavigationThemeTransition());
                break;
            case GrowlTransition.PaneThemeTransition:
                transitions.Add(new PaneThemeTransition());
                break;
            case GrowlTransition.PopupThemeTransition:
                transitions.Add(new PopupThemeTransition());
                break;
            case GrowlTransition.ReorderThemeTransition:
                transitions.Add(new ReorderThemeTransition());
                break;
            case GrowlTransition.RepositionThemeTransition:
                transitions.Add(new RepositionThemeTransition());
                break;
        }

        if (panel is not null)
        {
            panel.ChildrenTransitions = transitions;
        }
    }

    private static InfoBar? InitGrowl(GrowlInfo growlInfo)
    {
        var ib = new InfoBar
        {
            Title = growlInfo.Title,
            Message = growlInfo.Message,
            IsIconVisible = growlInfo.IsIconVisible,
            IconSource = growlInfo.IconSource,
            IsClosable = growlInfo.IsClosable,
            Severity = growlInfo.Severity,
            IsOpen = true,
            Width = 340,
            ActionButton = growlInfo.ActionButton
        };

        ib.CloseButtonClick += growlInfo.CloseButtonClicked;

        if (growlInfo.Token is 0 || !_panelDic.TryGetValue(growlInfo.Token, out var panel))
            if (GrowlPanel is not null)
                panel = GrowlPanel;
            else
                return null;

        if (panel.Children.Count >= MaxGrowlCount)
        {
            panel.Children.RemoveAt(0);
        }
        panel.Children.Add(ib);
        if (!growlInfo.StaysOpen)
        {
            _ = Task.Delay(growlInfo.WaitTime).ContinueWith(_ => panel.Children.Remove(ib), TaskScheduler.FromCurrentSynchronizationContext());
        }
        return ib;
    }

    private static void Clear(Panel? panel) => panel?.Children.Clear();

    private static void RemoveGrowl(ulong token, InfoBar growl)
    {
        if (token is 0 || !_panelDic.TryGetValue(token, out var panel))
            if (GrowlPanel is not null)
                panel = GrowlPanel;
            else
                return;

        _ = panel.Children.Remove(growl);
    }

    public static void Clear(ulong token = 0)
    {
        if (token is 0)
            Clear(GrowlPanel);
        else if (_panelDic.TryGetValue(token, out var panel))
            Clear(panel);
    }
}
