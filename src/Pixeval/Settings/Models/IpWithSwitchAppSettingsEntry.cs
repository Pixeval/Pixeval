using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using System.Reflection;
using Windows.System;
using Pixeval.AppManagement;
using Pixeval.Attributes;
using Pixeval.Controls.Settings;
using Pixeval.Utilities;
using Symbol = FluentIcons.Common.Symbol;

namespace Pixeval.Settings.Models;

public partial class IpWithSwitchAppSettingsEntry : BoolAppSettingsEntry
{
    public IpWithSwitchAppSettingsEntry(AppSettings appSettings) : base(appSettings, t => t.EnableDomainFronting)
    {
        PixivAppApiNameResolver = [.. appSettings.PixivAppApiNameResolver];
        PixivImageNameResolver = [.. appSettings.PixivImageNameResolver];
        PixivImageNameResolver2 = [.. appSettings.PixivImageNameResolver2];
        PixivOAuthNameResolver = [.. appSettings.PixivOAuthNameResolver];
        PixivAccountNameResolver = [.. appSettings.PixivAccountNameResolver];
        PixivWebApiNameResolver = [.. appSettings.PixivWebApiNameResolver];

        var member = typeof(AppSettings).GetProperty(nameof(AppSettings.PixivAppApiNameResolver));
        Attribute2 = member?.GetCustomAttribute<SettingsEntryAttribute>();

        if (Attribute2 is { } attribute)
        {
            Header2 = attribute.LocalizedResourceHeader;
            Description2 = attribute.LocalizedResourceDescription;
            HeaderIcon2 = attribute.Symbol;
        }
    }

    #region Entry2

    public Symbol HeaderIcon2 { get; set; }

    public string Header2 { get; set; } = "";

    public object DescriptionControl2
    {
        get
        {
            if (DescriptionUri2 is not null)
            {
                var b = new HyperlinkButton { Content = Description2 };
                if (DescriptionUri2.Scheme is "http" or "https")
                {
                    b.NavigateUri = DescriptionUri2;
                    return b;
                }

                var uri = DescriptionUri2;
                b.Click += (_, _) => _ = Launcher.LaunchUriAsync(uri);
                return b;
            }
            return Description2;
        }
    }

    public string Description2 { get; set; } = "";

    public Uri? DescriptionUri2 { get; set; }

    public SettingsEntryAttribute? Attribute2 { get; }

    #endregion

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

    public ObservableCollection<string> PixivAppApiNameResolver { get; set; }

    public ObservableCollection<string> PixivImageNameResolver { get; set; }

    public ObservableCollection<string> PixivImageNameResolver2 { get; set; }

    public ObservableCollection<string> PixivOAuthNameResolver { get; set; }

    public ObservableCollection<string> PixivAccountNameResolver { get; set; }

    public ObservableCollection<string> PixivWebApiNameResolver { get; set; }
}
