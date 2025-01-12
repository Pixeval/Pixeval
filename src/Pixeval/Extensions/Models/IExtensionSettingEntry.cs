// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Settings;

namespace Pixeval.Extensions.Models;

public interface IExtensionSettingEntry : ISettingsEntry
{
    void ValueReset();
}
