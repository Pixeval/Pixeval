// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using AutoSettingsPage.Models;
using FluentIcons.Common;

namespace Pixeval.Models.Settings.Entries;

public class WorkSubscriptionsSettingsEntry()
    : ObservableSettingsEntry(
        nameof(WorkSubscriptionsSettingsEntry),
        new(Symbol.FolderSync,
            AppSettingsResources.WorkSubscriptionsSettingsEntryHeader,
            AppSettingsResources.WorkSubscriptionsSettingsEntryDescription));
