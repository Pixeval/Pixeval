// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using System;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace Pixeval.Controls;

public class GrowlInfo
{
    public string? Title { get; set; }

    public string? Message { get; set; }

    public ulong Token { get; set; }

    public object? Content { get; set; }

    public InfoBarSeverity Severity { get; set; }

    public IconSource? IconSource { get; set; }

    public bool IsClosable { get; set; } = true;

    public bool StaysOpen { get; set; }

    public bool IsIconVisible { get; set; } = true;

    public TimeSpan WaitTime { get; set; } = TimeSpan.FromSeconds(3);

    public Button? ActionButton { get; set; }

    public TypedEventHandler<InfoBar, object>? CloseButtonClicked;
}
