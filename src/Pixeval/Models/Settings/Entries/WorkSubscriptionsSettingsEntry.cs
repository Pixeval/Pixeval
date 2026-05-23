// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using AutoSettingsPage.Models;
using FluentIcons.Common;

namespace Pixeval.Models.Settings.Entries;

public class WorkSubscriptionsSettingsEntry()
    : ObservableSettingsEntry(
        nameof(WorkSubscriptionsSettingsEntry),
        new(Symbol.FolderSync,
            AppSettingsResources.WorkSubscriptionsSettingsEntryHeader,
            AppSettingsResources.WorkSubscriptionsSettingsEntryDescription));
