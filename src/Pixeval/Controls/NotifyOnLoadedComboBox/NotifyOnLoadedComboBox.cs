using System;
using Microsoft.UI.Xaml.Controls;

namespace Pixeval.Controls.NotifyOnLoadedComboBox
{
    public class NotifyOnLoadedComboBox : ComboBox
    {
        public NotifyOnLoadedComboBox()
        {
            DefaultStyleKey = typeof(NotifyOnLoadedComboBox);
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