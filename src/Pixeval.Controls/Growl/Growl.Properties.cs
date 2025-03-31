// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Pixeval.Controls;

public static partial class Growl
{
    public static ulong GetToken(DependencyObject obj)
    {
        return (ulong) obj.GetValue(TokenProperty);
    }

    public static void SetToken(DependencyObject obj, ulong value)
    {
        obj.SetValue(TokenProperty, value);
    }

    public static readonly DependencyProperty TokenProperty =
        DependencyProperty.RegisterAttached("Token", typeof(ulong), typeof(Growl), new PropertyMetadata(null, OnTokenChanged));

    private static void OnTokenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Panel panel && e.NewValue is ulong i)
        {
            Unregister(panel);
            Register(i, panel);
        }
    }

    public static void SetGrowlParent(DependencyObject element, bool value) => element.SetValue(GrowlParentProperty, value);

    public static bool GetGrowlParent(DependencyObject element) => (bool) element.GetValue(GrowlParentProperty);

    public static readonly DependencyProperty GrowlParentProperty = DependencyProperty.RegisterAttached(
        "GrowlParent", typeof(bool), typeof(Growl), new PropertyMetadata(false, (o, args) =>
        {
            if ((bool) args.NewValue && o is Panel panel)
            {
                panel.Loaded += Panel_Loaded;
            }
        }));

    private static void Panel_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is Panel panel)
        {
            // Panel is now fully loaded, you can perform initialization here
            if (GetGrowlParent(panel))
            {
                if (GetToken(panel) is var token and not 0)
                {
                    // If Token is set, use the dictionary
                    _PanelDic[token] = panel;
                }
                else
                {
                    // If Token is not set, handle things as before
                    GrowlPanel = panel;
                }

                SetDefaultPanelTransition(panel);
            }

            // Remove the event handler to avoid multiple subscriptions
            panel.Loaded -= Panel_Loaded;
        }
    }

    public static void SetGrowlEnterTransition(DependencyObject element, GrowlTransition value) => element.SetValue(GrowlEnterTransitionProperty, value);

    public static GrowlTransition GetGrowlEnterTransition(DependencyObject element) => (GrowlTransition) element.GetValue(GrowlEnterTransitionProperty);

    public static readonly DependencyProperty GrowlEnterTransitionProperty = DependencyProperty.RegisterAttached(
        "GrowlEnterTransition", typeof(GrowlTransition), typeof(Growl), new PropertyMetadata(GrowlTransition.AddDeleteThemeTransition, (o, args) =>
        {
            if (args.NewValue is GrowlTransition growlTransition && o is Panel panel)
            {
                SetPanelTransition(growlTransition, panel);
            }
        }));

    public static void SetGrowlExitTransition(DependencyObject element, GrowlTransition value) => element.SetValue(GrowlExitTransitionProperty, value);

    public static GrowlTransition GetGrowlExitTransition(DependencyObject element) => (GrowlTransition) element.GetValue(GrowlExitTransitionProperty);

    public static readonly DependencyProperty GrowlExitTransitionProperty = DependencyProperty.RegisterAttached(
        "GrowlExitTransition", typeof(GrowlTransition), typeof(Growl), new PropertyMetadata(GrowlTransition.AddDeleteThemeTransition));
}
