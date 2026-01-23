// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Text.Json;
using AutoSettingsPage.Models;

namespace Pixeval.Models.Extensions;

public interface IExtensionSettingEntry : ISettingsEntry
{
    void ValueReset();

    void ValueSaving(Dictionary<string, JsonElement> values);
}
