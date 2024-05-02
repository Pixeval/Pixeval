using System;
using System.Collections.Generic;
using Pixeval.Util;

namespace Pixeval.Settings;

public class SimpleSettingsGroup(Enum tag) : List<ISettingsEntry>, ISettingsGroup
{
    public string Header { get; } = tag.GetLocalizedResourceContent() ?? "";

    public Enum Tag { get; } = tag;
}
