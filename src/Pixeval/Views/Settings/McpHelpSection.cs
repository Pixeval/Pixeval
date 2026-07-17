// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Avalonia.Controls;
#if PIXEVAL_MCP
using Pixeval.I18N;
#endif

namespace Pixeval.Views.Settings;

public partial class McpHelpSection : UserControl
{
    public McpHelpSection()
    {
#if PIXEVAL_MCP
        InitializeComponent();
        McpToolsStatus = I18NManager.GetResource(HelpPageResources.McpToolsStatusDisabled);
#endif
    }
}
