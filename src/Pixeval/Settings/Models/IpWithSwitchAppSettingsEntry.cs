using System.Collections.ObjectModel;
using System.Linq;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;
using Pixeval.Utilities;

namespace Pixeval.Settings.Models;

public class IpWithSwitchAppSettingsEntry(
    AppSettings appSettings)
    : BoolAppSettingsEntry(appSettings, t => t.EnableDomainFronting)
{
    public override IpWithSwitchSettingsExpander Element => new() { Entry = this };

    public override void ValueReset()
    {
        base.ValueReset();
        PixivAppApiNameResolver = [.. Settings.PixivAppApiNameResolver];
        PixivImageNameResolver = [.. Settings.PixivImageNameResolver];
        PixivImageNameResolver2 = [.. Settings.PixivImageNameResolver2];
        PixivOAuthNameResolver = [.. Settings.PixivOAuthNameResolver];
        PixivAccountNameResolver = [.. Settings.PixivAccountNameResolver];
        PixivWebApiNameResolver = [.. Settings.PixivWebApiNameResolver];

        OnPropertyChanged(nameof(PixivAppApiNameResolver));
        OnPropertyChanged(nameof(PixivImageNameResolver));
        OnPropertyChanged(nameof(PixivImageNameResolver2));
        OnPropertyChanged(nameof(PixivOAuthNameResolver));
        OnPropertyChanged(nameof(PixivAccountNameResolver));
        OnPropertyChanged(nameof(PixivWebApiNameResolver));
    }

    public override void ValueSaving()
    {
        var appApiNameSame = Settings.PixivAppApiNameResolver.SequenceEquals(PixivAppApiNameResolver);
        var imageNameSame = Settings.PixivImageNameResolver.SequenceEqual(PixivImageNameResolver);
        var imageName2Same = Settings.PixivImageNameResolver2.SequenceEqual(PixivImageNameResolver2);
        var oAuthNameSame = Settings.PixivOAuthNameResolver.SequenceEqual(PixivOAuthNameResolver);
        var accountNameSame = Settings.PixivAccountNameResolver.SequenceEqual(PixivAccountNameResolver);
        var webApiNameSame = Settings.PixivWebApiNameResolver.SequenceEqual(PixivWebApiNameResolver);

        Settings.PixivAppApiNameResolver = [.. PixivAppApiNameResolver];
        Settings.PixivImageNameResolver = [.. PixivImageNameResolver];
        Settings.PixivImageNameResolver2 = [.. PixivImageNameResolver2];
        Settings.PixivOAuthNameResolver = [.. PixivOAuthNameResolver];
        Settings.PixivAccountNameResolver = [.. PixivAccountNameResolver];
        Settings.PixivWebApiNameResolver = [.. PixivWebApiNameResolver];

        if (appApiNameSame || imageNameSame || imageName2Same || oAuthNameSame || accountNameSame || webApiNameSame)
            AppInfo.SetNameResolvers(Settings);
    }

    public ObservableCollection<string> PixivAppApiNameResolver { get; set; } = [.. appSettings.PixivAppApiNameResolver];

    public ObservableCollection<string> PixivImageNameResolver { get; set; } = [.. appSettings.PixivImageNameResolver];

    public ObservableCollection<string> PixivImageNameResolver2 { get; set; } = [.. appSettings.PixivImageNameResolver2];

    public ObservableCollection<string> PixivOAuthNameResolver { get; set; } = [.. appSettings.PixivOAuthNameResolver];

    public ObservableCollection<string> PixivAccountNameResolver { get; set; } = [.. appSettings.PixivAccountNameResolver];

    public ObservableCollection<string> PixivWebApiNameResolver { get; set; } = [.. appSettings.PixivWebApiNameResolver];
}
