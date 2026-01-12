// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Windows.Foundation.Collections;
using AutoSettingsPage.Models;

namespace Pixeval.Extensions;

public interface IExtensionSettingEntry : ISettingsEntry
{
    void ValueReset();

    void ValueSaving(IPropertySet values);
}
