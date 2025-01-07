// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;

namespace Pixeval.Settings;

public interface ISettingsGroup : IEnumerable<ISettingsEntry>
{
    string Header { get; }
}