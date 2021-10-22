#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/WeakEventListener.cs
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
using System.ComponentModel;

namespace Pixeval.CommunityToolkit
{
    /// <summary>
    ///     Implements a weak event listener that allows the owner to be garbage
    ///     collected if its only remaining link is an event handler.
    ///     Note: Copied from Microsoft.Toolkit.Uwp.Helpers.WeakEventListener to avoid taking a
    ///     dependency on Microsoft.Toolkit.Uwp.dll and Microsoft.Toolkit.dll.
    /// </summary>
    /// <typeparam name="TInstance">Type of instance listening for the event.</typeparam>
    /// <typeparam name="TSource">Type of source for the event.</typeparam>
    /// <typeparam name="TEventArgs">Type of event arguments for the event.</typeparam>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal sealed class WeakEventListener<TInstance, TSource, TEventArgs>
        where TInstance : class
    {
        /// <summary>
        ///     WeakReference to the instance listening for the event.
        /// </summary>
        private readonly WeakReference weakInstance;

        /// <summary>
        ///     Initializes a new instance of the <see cref="WeakEventListener{TInstance, TSource, TEventArgs}" /> class.
        /// </summary>
        /// <param name="instance">Instance subscribing to the event.</param>
        public WeakEventListener(TInstance instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            weakInstance = new WeakReference(instance);
        }

        /// <summary>
        ///     Gets or sets the method to call when the event fires.
        /// </summary>
        public Action<TInstance, TSource?, TEventArgs>? OnEventAction { get; set; }

        /// <summary>
        ///     Gets or sets the method to call when detaching from the event.
        /// </summary>
        public Action<WeakEventListener<TInstance, TSource, TEventArgs>>? OnDetachAction { get; set; }

        /// <summary>
        ///     Handler for the subscribed event calls OnEventAction to handle it.
        /// </summary>
        /// <param name="source">Event source.</param>
        /// <param name="eventArgs">Event arguments.</param>
        public void OnEvent(TSource? source, TEventArgs eventArgs)
        {
            if (weakInstance.Target is TInstance target)
            {
                // Call registered action
                OnEventAction?.Invoke(target, source, eventArgs);
            }
            else
            {
                // Detach from event
                Detach();
            }
        }

        /// <summary>
        ///     Detaches from the subscribed event.
        /// </summary>
        public void Detach()
        {
            OnDetachAction?.Invoke(this);
            OnDetachAction = null;
        }
    }
}