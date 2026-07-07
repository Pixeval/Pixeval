// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Markdown.Avalonia.Plugins;

namespace Pixeval.Controls;

internal sealed class PixevalMarkdownPlugin : IMdAvPlugin
{
    public void Setup(SetupInfo info)
    {
        info.Register(new PixevalFencedCodeBlockOverride());
        info.Register(new PixevalIndentedCodeBlockOverride());
        info.Register(new GitHubAlertBlockOverride());
        info.Register(new ExtraEscapedCharacterInlineParser());
    }
}
