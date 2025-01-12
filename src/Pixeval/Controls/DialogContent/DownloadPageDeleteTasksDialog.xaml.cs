// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using WinUI3Utilities.Attributes;

namespace Pixeval.Controls.DialogContent;

[DependencyProperty<bool>("DeleteLocalFiles", "false")]
public sealed partial class DownloadPageDeleteTasksDialog
{
    public DownloadPageDeleteTasksDialog() => InitializeComponent();
}
