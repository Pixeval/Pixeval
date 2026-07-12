// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Text.Json.Serialization;
using SharpYaml;
using SharpYaml.Serialization;

namespace Pixeval.Models.Navigation;

[YamlSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = YamlIgnoreCondition.WhenWritingNull,
    WriteIndented = true,
    IndentSize = 2,
    BlockSequenceMappingStyle = YamlSequenceItemStyle.Compact,
    BlockSequenceSequenceStyle = YamlSequenceItemStyle.Expanded)]
[YamlSerializable(typeof(NavigationYamlSettings))]
[YamlSerializable(typeof(NavigationYamlItem))]
public partial class NavigationYamlSerializerContext : YamlSerializerContext;
