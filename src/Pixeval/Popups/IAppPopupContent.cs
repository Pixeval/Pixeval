using System;
using Microsoft.UI.Xaml;

namespace Pixeval.Popups
{
    public interface IAppPopupContent
    {
        Guid UniqueId { get; }

        FrameworkElement UIContent { get; }
    }
}