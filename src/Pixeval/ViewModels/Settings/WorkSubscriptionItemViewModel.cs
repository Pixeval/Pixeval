// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Diagnostics.CodeAnalysis;
using Mako.Model;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.Models.Database;

namespace Pixeval.ViewModels.Settings;

public class WorkSubscriptionItemViewModel(WorkSubscriptionEntry entry) : ViewModelBase
{
    public WorkSubscriptionEntry Entry { get; } = entry;

    public UserBasicInfo User { get; } = new WorkSubscriptionShimmer(entry);

    public string SubscriptionTypeText => SymbolComboBoxItem.GetResource(Entry.SubscriptionType);

    public string WorkKindText => SymbolComboBoxItem.GetResource(Entry.WorkKind);

    private sealed record WorkSubscriptionShimmer : UserBasicInfo
    {
        [SetsRequiredMembers]
        public WorkSubscriptionShimmer(WorkSubscriptionEntry entry)
        {
            Id = entry.UserId;
            Name = entry.DisplayName;
            Account = entry.Account;
            AvatarUrl = string.IsNullOrWhiteSpace(entry.AvatarUrl)
                ? AppInfo.PixivNoProfilePath
                : entry.AvatarUrl;
        }

        public override string AvatarUrl { get; }

        public override string Description { get; set; } = "";
    }
}
