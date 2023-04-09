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

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using WinUI3Utilities;

namespace Pixeval.Controls;

public class EnhancedPage : Page
{
    protected Frame ParentFrame => Parent.To<Frame>();

    public int ActivationCount { get; private set; }

    public EnhancedPage()
    {
        Loaded += (_, _) =>
        {
            if (!Initialized)
            {
                Initialized = true;
            }
        };
    }

    public bool ClearCacheAfterNavigation { get; set; }

    public bool Initialized { get; private set; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ActivationCount++;
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
