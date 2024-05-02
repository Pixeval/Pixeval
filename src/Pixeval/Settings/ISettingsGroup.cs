using System.Collections.Generic;

namespace Pixeval.Settings;

public interface ISettingsGroup : IEnumerable<ISettingsEntry>
{
    string Header { get; }
}