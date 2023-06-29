#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/EnhancedWindowPage.cs
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

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Util.UI.Windowing;
using WinUI3Utilities;

namespace Pixeval.Controls;

public static class EnhancedWindowPageExtension
{
    public static void Navigate<T>(this Frame frame, EnhancedWindow window, object parameter, NavigationTransitionInfo? info) where T : EnhancedWindowPage
    {
        _ = frame.Navigate(typeof(T), new NavigateParameter(parameter, window, frame), info);
    }
}

public record NavigateParameter(object Parameter, EnhancedWindow Window, Frame Frame);

public class EnhancedWindowPage : Page, IEnhancedPage
{
    protected EnhancedWindow Window { get; private set; } = null!;

    protected Frame ParentFrame { get; private set; } = null!;

    public int ActivationCount { get; private set; }

    public EnhancedWindowPage()
    {
        Loaded += (_, _) =>
        {
            Initialized = true;
            (this as ISupportCustomTitleBarDragRegion)?.SetTitleBarDragRegion();
        };
        SizeChanged += (_, _) => (this as ISupportCustomTitleBarDragRegion)?.SetTitleBarDragRegion();
    }

    public bool ClearCacheAfterNavigation { get; set; }

    public bool Initialized { get; private set; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ++ActivationCount;
        var parameter = e.Parameter.To<NavigateParameter>();
        Window = parameter.Window;
        ParentFrame = parameter.Frame;
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
        _ = frame.Navigate(typeof(TPage), new NavigateParameter(parameter, Window, frame), info);
    }

    protected void NavigateParent<TPage>(object parameter, NavigationTransitionInfo? info = null) where TPage : EnhancedWindowPage
    {
        _ = ParentFrame.Navigate(typeof(TPage), new NavigateParameter(parameter, Window, ParentFrame), info);
    }

    protected void NavigateSelf(object parameter, NavigationTransitionInfo? info = null)
    {
        _ = ParentFrame.Navigate(typeof(EnhancedWindowPage), new NavigateParameter(parameter, Window, ParentFrame), info);
    }

    public virtual void OnPageDeactivated(NavigatingCancelEventArgs e)
    {
    }

    public virtual void OnPageActivated(NavigationEventArgs e, object parameter)
    {
    }
}
