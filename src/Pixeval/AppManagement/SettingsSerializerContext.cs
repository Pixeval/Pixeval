// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.ObjectModel;
using Pixeval.Models.Home;
using SharpYaml.Serialization;

namespace Pixeval.AppManagement;

[YamlSerializable(typeof(AppSettings))]
[YamlSerializable(typeof(LoginContext))]
[YamlSerializable(typeof(ObservableCollection<HomePageCardLayout>))]
public partial class SettingsSerializerContext : YamlSerializerContext;
