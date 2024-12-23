#region Copyright (c) Pixeval/Pixeval.Controls
// GPL v3 License
// 
// Pixeval/Pixeval.Controls
// Copyright (c) 2023 Pixeval.Controls/EnhancedPage.cs
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

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Pixeval.Controls;

public partial class EnhancedPage : Page
{
    public int ActivationCount { get; private set; }

    public bool ClearCacheAfterNavigation { get; set; } = true;

    protected sealed override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ++ActivationCount;
        OnPageActivated(e);
    }

    protected sealed override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        base.OnNavigatingFrom(e);
        OnPageDeactivated(e);

        if (!ClearCacheAfterNavigation)
            return;
        NavigationCacheMode = NavigationCacheMode.Disabled;
        if (Parent is not Frame frame)
            return;
        var cacheSize = frame.CacheSize;
        frame.CacheSize = 0;
        frame.CacheSize = cacheSize;
    }

    /// <inheritdoc cref="OnPageActivated"/>
    /// <remarks>
    /// 只有被导航时才会调用此方法，直接关闭窗口等情况下不会调用此方法
    /// </remarks>
    public virtual void OnPageDeactivated(NavigatingCancelEventArgs e)
    {
    }

    /// <summary>
    /// <see cref="OnNavigatedTo"/>-><br/>
    /// <see cref="OnPageActivated"/>-><br/>
    /// <see cref="FrameworkElement.Loaded"/>-><br/>
    /// <see cref="OnPageDeactivated"/>-><br/>
    /// <see cref="OnNavigatingFrom"/>-><br/>
    /// <see cref="FrameworkElement.Unloaded"/>
    /// </summary>
    public virtual void OnPageActivated(NavigationEventArgs e)
    {
    }
}
