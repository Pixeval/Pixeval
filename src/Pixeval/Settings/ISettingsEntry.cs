using Microsoft.UI.Xaml;

namespace Pixeval.Settings;

public interface ISettingsEntry
{
    FrameworkElement Element { get; }

    void ValueReset();

    void ValueSaving();
}
