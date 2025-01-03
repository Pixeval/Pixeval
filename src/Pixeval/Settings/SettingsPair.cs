// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Windows.Foundation.Collections;

namespace Pixeval.Settings;

public record SettingsPair<TSettings>(TSettings Settings, IPropertySet Values);
