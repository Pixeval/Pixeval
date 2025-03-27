// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Mako.Global.Enum;

[JsonConverter(typeof(SnakeCaseLowerEnumConverter<TargetFilter>))]
public enum TargetFilter
{
    [Description("for_android")]
    ForAndroid,

    [Description("for_ios")]
    [EnumMember(Value = "for_ios")]
    ForIos
}
