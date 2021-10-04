using System;
using Microsoft.UI.Xaml;
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
            CornerRadius = new CornerRadius(2);
        }

        private EventHandler<SelectionChangedEventArgs>? _selectionChangedWhenLoaded;

        public event EventHandler<SelectionChangedEventArgs> SelectionChangedWhenLoaded
        {
            add => _selectionChangedWhenLoaded += value;
            remove => _selectionChangedWhenLoaded -= value;
        }
    }
}