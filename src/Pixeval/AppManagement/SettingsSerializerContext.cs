// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using SharpYaml.Serialization;

namespace Pixeval.AppManagement;

[YamlSerializable(typeof(AppSettings))]
[YamlSerializable(typeof(LoginContext))]
[YamlSerializable(typeof(HomePageCardsSettings))]
public partial class SettingsSerializerContext : YamlSerializerContext;
