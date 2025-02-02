// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using CommunityToolkit.WinUI;

namespace Pixeval.Controls.DialogContent;

public sealed partial class DownloadPageDeleteTasksDialog
{
    [GeneratedDependencyProperty(DefaultValue = false)]
    public partial bool DeleteLocalFiles { get; set; }

    public DownloadPageDeleteTasksDialog() => InitializeComponent();
}
