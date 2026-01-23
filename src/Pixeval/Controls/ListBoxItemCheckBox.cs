// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia.Controls;

namespace Pixeval.Controls;

public class ListBoxItemCheckBox : CheckBox
{
    public ListBoxItemCheckBox()
    {
        Tapped += (sender, e) => e.Handled = true;
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(CheckBox);
}
