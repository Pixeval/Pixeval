using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Pixeval.Controls.Card
{
    [TemplatePart(Name = "CardContainer", Type = typeof(Grid))]
    public sealed class CardControl : ContentControl
    {
        public CardControl()
        {
            DefaultStyleKey = typeof(CardControl);
        }
    }
}
