#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/ThemeListener.cs
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
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using WindowActivatedEventArgs = Windows.UI.Core.WindowActivatedEventArgs;
#if DEBUG
using System.Diagnostics;
#endif

namespace Pixeval.CommunityToolkit
{
    /// <summary>
    ///     The Delegate for a ThemeChanged Event.
    /// </summary>
    /// <param name="sender">Sender ThemeListener</param>
    public delegate void ThemeChangedEvent(ThemeListener sender);

    /// <summary>
    ///     Class which listens for changes to Application Theme or High Contrast Modes
    ///     and Signals an Event when they occur.
    /// </summary>
    [AllowForWeb]
    public sealed class ThemeListener : IDisposable
    {
        private readonly AccessibilitySettings _accessible = new();
        private readonly UISettings _settings = new();

        /// <summary>
        ///     Initializes a new instance of the <see cref="ThemeListener" /> class.
        /// </summary>
        /// <param name="dispatcherQueue">
        ///     The DispatcherQueue that should be used to dispatch UI updates, or null if this is being
        ///     called from the UI thread.
        /// </param>
        public ThemeListener(DispatcherQueue? dispatcherQueue = null)
        {
            CurrentTheme = Application.Current.RequestedTheme;
            IsHighContrast = _accessible.HighContrast;

            DispatcherQueue = dispatcherQueue ?? DispatcherQueue.GetForCurrentThread();

            if (Window.Current != null)
            {
                _accessible.HighContrastChanged += Accessible_HighContrastChanged;
                _settings.ColorValuesChanged += Settings_ColorValuesChanged;

                Window.Current.CoreWindow.Activated += CoreWindow_Activated;
            }
        }

        /// <summary>
        ///     Gets the Name of the Current Theme.
        /// </summary>
        public string CurrentThemeName => CurrentTheme.ToString();

        /// <summary>
        ///     Gets or sets the Current Theme.
        /// </summary>
        public ApplicationTheme CurrentTheme { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the current theme is high contrast.
        /// </summary>
        public bool IsHighContrast { get; set; }

        /// <summary>
        ///     Gets or sets which DispatcherQueue is used to dispatch UI updates.
        /// </summary>
        public DispatcherQueue DispatcherQueue { get; set; }

        /// <inheritdoc />
        public void Dispose()
        {
            _accessible.HighContrastChanged -= Accessible_HighContrastChanged;
            _settings.ColorValuesChanged -= Settings_ColorValuesChanged;
            if (Window.Current != null)
            {
                Window.Current.CoreWindow.Activated -= CoreWindow_Activated;
            }
        }

        /// <summary>
        ///     An event that fires if the Theme changes.
        /// </summary>
        public event ThemeChangedEvent? ThemeChanged;

        private async void Accessible_HighContrastChanged(AccessibilitySettings sender, object args)
        {
#if DEBUG
            Debug.WriteLine("HighContrast Changed");
#endif

            await OnThemePropertyChangedAsync();
        }

        // Note: This can get called multiple times during HighContrast switch, do we care?
        private async void Settings_ColorValuesChanged(UISettings sender, object args)
        {
            await OnThemePropertyChangedAsync();
        }

        /// <summary>
        ///     Dispatches an update for the public properties and the firing of <see cref="ThemeChanged" /> on
        ///     <see cref="DispatcherQueue" />.
        /// </summary>
        /// <returns>A <see cref="Task" /> that indicates when the dispatching has completed.</returns>
        internal Task OnThemePropertyChangedAsync()
        {
            // Getting called off thread, so we need to dispatch to request value.
            return DispatcherQueue.EnqueueAsync(
                () =>
                {
                    // TODO: This doesn't stop the multiple calls if we're in our faked 'White' HighContrast Mode below.
                    if (CurrentTheme != Application.Current.RequestedTheme ||
                        IsHighContrast != _accessible.HighContrast)
                    {
#if DEBUG
                        Debug.WriteLine("Color Values Changed");
#endif

                        UpdateProperties();
                    }
                });
        }

        private void CoreWindow_Activated(CoreWindow sender, WindowActivatedEventArgs args)
        {
            if (CurrentTheme != Application.Current.RequestedTheme ||
                IsHighContrast != _accessible.HighContrast)
            {
#if DEBUG
                Debug.WriteLine("CoreWindow Activated Changed");
#endif
                UpdateProperties();
            }
        }

        /// <summary>
        ///     Set our current properties and fire a change notification.
        /// </summary>
        private void UpdateProperties()
        {
            // TODO: Not sure if HighContrastScheme names are localized?
            if (_accessible.HighContrast && _accessible.HighContrastScheme.IndexOf("white", StringComparison.OrdinalIgnoreCase) != -1)
            {
                // If our HighContrastScheme is ON & a lighter one, then we should remain in 'Light' theme mode for Monaco Themes Perspective
                IsHighContrast = false;
                CurrentTheme = ApplicationTheme.Light;
            }
            else
            {
                // Otherwise, we just set to what's in the system as we'd expect.
                IsHighContrast = _accessible.HighContrast;
                CurrentTheme = Application.Current.RequestedTheme;
            }

            ThemeChanged?.Invoke(this);
        }
    }
}