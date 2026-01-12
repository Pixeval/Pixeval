// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using AutoSettingsPage.Models;
using FluentIcons.Common;

namespace Pixeval.Extensions;

public partial class ExtensionSettingsGroup(ExtensionsHostModel model) : List<IExtensionSettingEntry>, ISettingsGroup
{
    public ExtensionsHostModel Model { get; } = model;

    /// <inheritdoc />
    public string Token { get; } = model.Name;

    /// <inheritdoc />
    public string Header { get; } = model.Name;

    /// <inheritdoc />
    public string Description { get; } = model.Description;

    /// <inheritdoc />
    public Symbol Icon => default;

    /// <inheritdoc />
    public Uri? DescriptionUri { get; } = model.Link;

    IEnumerator<ISettingsEntry> IEnumerable<ISettingsEntry>.GetEnumerator() => ((IEnumerable<IExtensionSettingEntry>) this).GetEnumerator();

    /// <inheritdoc />
    ISettingsEntry IReadOnlyList<ISettingsEntry>.this[int index] => this[index];
}
