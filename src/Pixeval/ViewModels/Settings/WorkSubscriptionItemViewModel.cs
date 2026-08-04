// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using Mako.Model;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.Models.Database;

namespace Pixeval.ViewModels.Settings;

public sealed partial class WorkSubscriptionItemViewModel(WorkSubscriptionEntry entry) : ViewModelBase
{
    public WorkSubscriptionEntry Entry { get; } = entry;

    [ObservableProperty] public partial UserBasicInfo User { get; private set; } = new WorkSubscriptionShimmer(entry);

    public string SubscriptionTypeText => SymbolComboBoxItem.GetResource(Entry.SubscriptionType);

    public string WorkKindText => SymbolComboBoxItem.GetResource(Entry.WorkKind);

    internal void UpdateSubscription(WorkSubscriptionEntry subscription)
    {
        Entry.UpdateFrom(subscription);
        User = new WorkSubscriptionShimmer(Entry);
    }

    private sealed record WorkSubscriptionShimmer : UserBasicInfo
    {
        [SetsRequiredMembers]
        public WorkSubscriptionShimmer(WorkSubscriptionEntry entry)
        {
            Id = entry.Id;
            Name = entry.DisplayName;
            Account = entry.Account;
            AvatarUrl = string.IsNullOrWhiteSpace(entry.AvatarUrl)
                ? AppInfo.ImageNotAvailablePath
                : entry.AvatarUrl;
        }

        public override string AvatarUrl { get; }
    }
}
