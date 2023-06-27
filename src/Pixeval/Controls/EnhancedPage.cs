#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/EnhancedPage.cs
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
using System.Globalization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Util.UI.Windowing;
using WinUI3Utilities;

namespace Pixeval.Controls;

public static class EnhancedWindowPageExtension
{
    public static void Navigate<T>(this Frame frame, CustomizableWindow window, object parameter, NavigationTransitionInfo? info) where T : EnhancedWindowPage
    {
        _ = frame.Navigate(typeof(T), new NavigateParameter(parameter, window), info);
    }
}

public class EnhancedWindowPage : Page
{
    protected CustomizableWindow Window { get; private set; } = null!;

    protected Frame ParentFrame => Parent.To<Frame>();

    public int ActivationCount { get; private set; }

    public EnhancedWindowPage() => Loaded += (_, _) =>
    {
        Initialized = true;
        if (this as ISupportCustomTitleBarDragRegionTest is { } page)
        {
            Window.SetDragRegion(page.GetTitleBarDragRegion());
        }
    };

    public bool ClearCacheAfterNavigation { get; set; }

    public bool Initialized { get; private set; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ++ActivationCount;
        var parameter = e.Parameter.To<NavigateParameter>();
        Window = parameter.Window;
        OnPageActivated(e, parameter.Parameter);
    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        base.OnNavigatingFrom(e);
        OnPageDeactivated(e);

        if (ClearCacheAfterNavigation)
        {
            NavigationCacheMode = NavigationCacheMode.Disabled;
            if (Parent is Frame frame)
            {
                var cacheSize = frame.CacheSize;
                frame.CacheSize = 0;
                frame.CacheSize = cacheSize;
            }
        }
    }

    protected void Navigate<TPage>(Frame frame, object parameter, NavigationTransitionInfo? info = null) where TPage : EnhancedWindowPage
    {
        _ = frame.Navigate(typeof(TPage), new NavigateParameter(parameter, Window), info);
    }

    protected void NavigateParent<TPage>(object parameter, NavigationTransitionInfo? info = null) where TPage : EnhancedWindowPage
    {
        _ = ParentFrame.Navigate(typeof(TPage), new NavigateParameter(parameter, Window), info);
    }

    protected void NavigateSelf(object parameter, NavigationTransitionInfo? info = null)
    {
        _ = ParentFrame.Navigate(typeof(EnhancedWindowPage), new NavigateParameter(parameter, Window), info);
    }

    public virtual void OnPageDeactivated(NavigatingCancelEventArgs e)
    {
    }

    public virtual void OnPageActivated(NavigationEventArgs e, object parameter)
    {
    }
}

public record NavigateParameter(object Parameter, CustomizableWindow Window);

public class EnhancedPage : Page
{
    protected Frame ParentFrame => Parent.To<Frame>();

    public int ActivationCount { get; private set; }

    public EnhancedPage()
    {
        Loaded += (_, _) => Initialized = true;
    }

    public bool ClearCacheAfterNavigation { get; set; }

    public bool Initialized { get; private set; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ++ActivationCount;
        OnPageActivated(e);
    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        base.OnNavigatingFrom(e);
        OnPageDeactivated(e);

        if (ClearCacheAfterNavigation)
        {
            NavigationCacheMode = NavigationCacheMode.Disabled;
            if (Parent is Frame frame)
            {
                var cacheSize = frame.CacheSize;
                frame.CacheSize = 0;
                frame.CacheSize = cacheSize;
            }
        }
    }

    public virtual void OnPageDeactivated(NavigatingCancelEventArgs e)
    {
    }

    public virtual void OnPageActivated(NavigationEventArgs e)
    {
    }
}
public class IsEqualStateTrigger : StateTriggerBase
{
    private void UpdateTrigger() => SetActive(IsEqualStateTrigger.AreValuesEqual(Value, To, true));

    /// <summary>
    /// Gets or sets the value for comparison.
    /// </summary>
    public object Value
    {
        get { return (object)GetValue(ValueProperty); }
        set { SetValue(ValueProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="Value"/> DependencyProperty
    /// </summary>
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(object), typeof(IsEqualStateTrigger), new PropertyMetadata(null, OnValuePropertyChanged));

    private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var obj = (IsEqualStateTrigger)d;
        obj.UpdateTrigger();
    }

    /// <summary>
    /// Gets or sets the value to compare equality to.
    /// </summary>
    public object To
    {
        get { return (object)GetValue(ToProperty); }
        set { SetValue(ToProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="To"/> DependencyProperty
    /// </summary>
    public static readonly DependencyProperty ToProperty =
                DependencyProperty.Register(nameof(To), typeof(object), typeof(IsEqualStateTrigger), new PropertyMetadata(null, OnValuePropertyChanged));

    internal static bool AreValuesEqual(object? value1, object? value2, bool convertType)
    {
        if (Equals(value1, value2))
        {
            return true;
        }

        // If they are the same type but fail with Equals check, don't bother with conversion.
        if (value1 is not null && value2 is not null && convertType
            && value1.GetType() != value2.GetType())
        {
            // Try the conversion in both ways:
            return ConvertTypeEquals(value1, value2) || ConvertTypeEquals(value2, value1);
        }

        return false;
    }

    private static bool ConvertTypeEquals(object? value1, object value2)
    {
        // Let's see if we can convert:
        if (value2 is Enum)
        {
            value1 = ConvertToEnum(value2.GetType(), value1);
        }
        else
        {
            value1 = Convert.ChangeType(value1, value2.GetType(), CultureInfo.InvariantCulture);
        }

        return value2.Equals(value1);
    }

    private static object? ConvertToEnum(Type enumType, object? value)
    {
        // value cannot be the same type of enum now
        return value switch
        {
            string str => Enum.TryParse(enumType, str, out var e) ? e : null,
            int or uint or byte or sbyte or long or ulong or short or ushort
                => Enum.ToObject(enumType, value),
            _ => null
        };
    }
}
