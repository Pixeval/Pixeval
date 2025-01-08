// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using System;
using Microsoft.UI.Xaml.Controls;

namespace Pixeval.Controls;

public partial class NotifyOnLoadedComboBox : ComboBox
{
    public NotifyOnLoadedComboBox()
    {
        DefaultStyleKey = typeof(NotifyOnLoadedComboBox);
        SelectionChanged += (sender, args) =>
        {
            if (IsDropDownOpen)
            {
                SelectionChangedWhenLoaded?.Invoke(sender, args);
            }
        };
    }

    public event EventHandler<SelectionChangedEventArgs>? SelectionChangedWhenLoaded;
}
