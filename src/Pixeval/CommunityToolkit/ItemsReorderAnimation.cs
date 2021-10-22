#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/ItemsReorderAnimation.cs
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
using Windows.ApplicationModel;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;

namespace Pixeval.CommunityToolkit
{
    /// <summary>
    ///     Provides the ability to assign a reorder animation to a <see cref="ListViewBase" />.
    /// </summary>
    public static class ItemsReorderAnimation
    {
        /// <summary>
        ///     Identifies the attached "Duration" <see cref="DependencyProperty" />.
        /// </summary>
        public static readonly DependencyProperty DurationProperty = DependencyProperty.RegisterAttached(
            "Duration",
            typeof(TimeSpan),
            typeof(ItemsReorderAnimation),
            new PropertyMetadata(TimeSpan.Zero, OnDurationChanged));

        /// <summary>
        ///     Identifies the attached "ReorderAnimation" <see cref="DependencyProperty" />.
        /// </summary>
        private static readonly DependencyProperty ReorderAnimationProperty = DependencyProperty.RegisterAttached(
            "ReorderAnimation",
            typeof(ImplicitAnimationCollection),
            typeof(ItemsReorderAnimation),
            new PropertyMetadata(null));

        /// <summary>
        ///     Gets the value of the <see cref="DurationProperty" /> property.
        /// </summary>
        /// <param name="listView">The <see cref="ListViewBase" /> to get the value for.</param>
        /// <returns>The retrieved <see cref="TimeSpan" /> value.</returns>
        public static TimeSpan GetDuration(ListViewBase listView)
        {
            return (TimeSpan) listView.GetValue(DurationProperty);
        }

        /// <summary>
        ///     Sets a value for the duration, in milliseconds, the animation should take.
        /// </summary>
        /// <param name="listView">the object to set the value on.</param>
        /// <param name="value">The duration.</param>
        public static void SetDuration(ListViewBase listView, TimeSpan value)
        {
            listView.SetValue(DurationProperty, value);
        }

        /// <summary>
        ///     Callback to apply the reorder animation when <see cref="DurationProperty" /> changes.
        /// </summary>
        /// <param name="d">The target object the property was changed for.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance for the current event.</param>
        private static void OnDurationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (DesignMode.DesignModeEnabled)
            {
                return;
            }

            if (d is ListViewBase listView && e.NewValue is TimeSpan duration)
            {
                AssignReorderAnimation(listView, duration);

                listView.ContainerContentChanging -= OnContainerContentChanging;
                listView.ContainerContentChanging += OnContainerContentChanging;

                listView.ChoosingItemContainer -= OnChoosingItemContainer;
                listView.ChoosingItemContainer += OnChoosingItemContainer;
            }
        }

        /// <summary>
        ///     Updates the reorder animation for a target <see cref="ListViewBase" /> instance.
        /// </summary>
        /// <param name="listView">The target <see cref="ListViewBase" /> instance.</param>
        /// <param name="duration">The duration of the animation.</param>
        private static void AssignReorderAnimation(UIElement listView, TimeSpan duration)
        {
            var compositor = ElementCompositionPreview.GetElementVisual(listView).Compositor;
            var animationCollection = listView.GetValue(ReorderAnimationProperty) as ImplicitAnimationCollection;

            if (animationCollection is null)
            {
                animationCollection = compositor.CreateImplicitAnimationCollection();

                listView.SetValue(ReorderAnimationProperty, animationCollection);
            }

            if (duration == TimeSpan.Zero)
            {
                animationCollection.Remove(nameof(Visual.Offset));
            }
            else
            {
                var offsetAnimation = compositor.CreateVector3KeyFrameAnimation();

                offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
                offsetAnimation.Duration = duration;
                offsetAnimation.Target = nameof(Visual.Offset);

                var animationGroup = compositor.CreateAnimationGroup();

                animationGroup.Add(offsetAnimation);

                animationCollection[nameof(Visual.Offset)] = animationGroup;
            }
        }

        /// <summary>
        ///     Updates the reorder animation to each container, whenever one changes.
        /// </summary>
        /// <param name="sender">The sender <see cref="ListViewBase" /> instance.</param>
        /// <param name="args">The <see cref="ContainerContentChangingEventArgs" /> instance for the current container change.</param>
        private static void OnContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.InRecycleQueue)
            {
                PokeUIElementZIndex(args.ItemContainer);
            }
            else
            {
                var visual = ElementCompositionPreview.GetElementVisual(args.ItemContainer);
                var animationCollection = sender.GetValue(ReorderAnimationProperty) as ImplicitAnimationCollection;

                visual.ImplicitAnimations = animationCollection;
            }
        }

        /// <summary>
        ///     Pokes the Z index of each container when one is chosen, to ensure animations are displayed correctly.
        /// </summary>
        /// <param name="sender">The sender <see cref="ListViewBase" /> instance.</param>
        /// <param name="args">The <see cref="ContainerContentChangingEventArgs" /> instance for the current container event.</param>
        private static void OnChoosingItemContainer(ListViewBase sender, ChoosingItemContainerEventArgs args)
        {
            if (args.ItemContainer is not null)
            {
                PokeUIElementZIndex(args.ItemContainer);
            }
        }

        /// <summary>
        ///     Pokes the Z index of a target <see cref="UIElement" />.
        /// </summary>
        /// <param name="element">The target <see cref="UIElement" /> to poke the Z index for.</param>
        private static void PokeUIElementZIndex(UIElement element)
        {
            var oldZIndex = Canvas.GetZIndex(element);

            Canvas.SetZIndex(element, oldZIndex + 1);
            Canvas.SetZIndex(element, oldZIndex);
        }
    }
}