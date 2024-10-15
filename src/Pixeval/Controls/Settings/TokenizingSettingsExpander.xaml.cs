using CommunityToolkit.WinUI.Controls;
using Pixeval.Settings.Models;

namespace Pixeval.Controls.Settings;

public sealed partial class TokenizingSettingsExpander 
{
    public TokenizingAppSettingsEntry Entry { get; set; } = null!;

    public TokenizingSettingsExpander() => InitializeComponent();

    private void TokenizingTextBox_OnTokenItemAdding(TokenizingTextBox sender, TokenItemAddingEventArgs e)
    {
        if (Entry.Settings.BlockedTags.Contains(e.TokenText))
            e.Cancel = true;
    }
}
