using System;
using Microsoft.UI.Xaml.Controls;

namespace Pixeval.Controls
{
    public class NotifyOnLoadedComboBox : ComboBox
    {
        public NotifyOnLoadedComboBox()
        {
            SelectionChanged += (sender, args) =>
            {
                if (IsDropDownOpen)
                {
                    _selectionChangedWhenLoaded?.Invoke(sender, args);
                }
            };
        }

        private EventHandler<SelectionChangedEventArgs>? _selectionChangedWhenLoaded;

        public event EventHandler<SelectionChangedEventArgs> SelectionChangedWhenLoaded
        {
            add => _selectionChangedWhenLoaded += value;
            remove => _selectionChangedWhenLoaded -= value;
        }
    }
}