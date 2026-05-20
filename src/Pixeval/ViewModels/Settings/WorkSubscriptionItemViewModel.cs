// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using CommunityToolkit.Mvvm.ComponentModel;
using Pixeval.Controls;
using Pixeval.Models.Database;

namespace Pixeval.ViewModels.Settings;

public class WorkSubscriptionItemViewModel(WorkSubscriptionEntry entry) : ObservableObject
{
    public WorkSubscriptionEntry Entry { get; } = entry;

    public int Id => Entry.HistoryEntryId;

    public long UserId => Entry.UserId;

    public string UserName => Entry.DisplayName;

    public string SubscriptionTypeText => SymbolComboBoxItem.GetResource(Entry.SubscriptionType);

    public string WorkKindText => SymbolComboBoxItem.GetResource(Entry.WorkKind);
}
