// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;
using System.Text.Json;
using AutoSettingsPage.Models;

namespace Pixeval.Models.Extensions;

public interface IExtensionSettingEntry : ISettingsEntry
{
    void ValueReset();

    void ValueSaving(Dictionary<string, JsonElement> values);
}
