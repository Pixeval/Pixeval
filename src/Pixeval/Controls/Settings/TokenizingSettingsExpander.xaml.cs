// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using CommunityToolkit.WinUI.Controls;
using Pixeval.Settings;

namespace Pixeval.Controls.Settings;

public sealed partial class TokenizingSettingsExpander
{
    public IMultiStringsAppSettingsEntry Entry { get; set; } = null!;

    public TokenizingSettingsExpander() => InitializeComponent();

    private void TokenizingTextBox_OnTokenItemAdding(TokenizingTextBox sender, TokenItemAddingEventArgs e)
    {
        if (Entry.Value.Contains(e.TokenText))
            e.Cancel = true;
    }
}
