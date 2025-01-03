// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using Pixeval.Settings;

namespace Pixeval.Extensions;

public partial class ExtensionSettingsGroup(ExtensionsHostModel model) : List<ISettingsEntry>, ISettingsGroup
{
    public string Header { get; } = model.Name;
}
