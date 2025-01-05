// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using Pixeval.Extensions.Models;
using Pixeval.Settings;

namespace Pixeval.Extensions;

public partial class ExtensionSettingsGroup(ExtensionsHostModel model) : List<IExtensionSettingEntry>, ISettingsGroup
{
    public ExtensionsHostModel Model { get; } = model;

    public string Header { get; } = model.Name;

    IEnumerator<ISettingsEntry> IEnumerable<ISettingsEntry>.GetEnumerator() => ((IEnumerable<IExtensionSettingEntry>)this).GetEnumerator();
}
