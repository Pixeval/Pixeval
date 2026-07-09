// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Diagnostics.CodeAnalysis;
using FluentIcons.Common;

namespace Pixeval.Models.Navigation;

public sealed record NavigationPageDefinition(
    string Key,
    [property: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    [param: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    Type PageType,
    Symbol Icon,
    string HeaderResource,
    string Header,
    bool NeedLogin);
