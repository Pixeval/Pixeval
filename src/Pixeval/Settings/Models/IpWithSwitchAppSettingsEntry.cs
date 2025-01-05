using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using System.Reflection;
using Windows.Foundation.Collections;
using Windows.System;
using Pixeval.AppManagement;
using Pixeval.Attributes;
using Pixeval.Controls.Settings;
using Pixeval.Utilities;
using Symbol = FluentIcons.Common.Symbol;

namespace Pixeval.Settings.Models;

public partial class IpWithSwitchAppSettingsEntry : BoolAppSettingsEntry
{
    public IpWithSwitchAppSettingsEntry(AppSettings settings) : base(settings, t => t.EnableDomainFronting)
    {
        PixivAppApiNameResolver = [.. settings.PixivAppApiNameResolver];
        PixivImageNameResolver = [.. settings.PixivImageNameResolver];
        PixivImageNameResolver2 = [.. settings.PixivImageNameResolver2];
        PixivOAuthNameResolver = [.. settings.PixivOAuthNameResolver];
        PixivAccountNameResolver = [.. settings.PixivAccountNameResolver];
        PixivWebApiNameResolver = [.. settings.PixivWebApiNameResolver];

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

    public override void ValueReset(AppSettings defaultSettings)
    {
        base.ValueReset(defaultSettings);

        PixivAppApiNameResolver = [.. defaultSettings.PixivAppApiNameResolver];
        PixivImageNameResolver = [.. defaultSettings.PixivImageNameResolver];
        PixivImageNameResolver2 = [.. defaultSettings.PixivImageNameResolver2];
        PixivOAuthNameResolver = [.. defaultSettings.PixivOAuthNameResolver];
        PixivAccountNameResolver = [.. defaultSettings.PixivAccountNameResolver];
        PixivWebApiNameResolver = [.. defaultSettings.PixivWebApiNameResolver];

        OnPropertyChanged(nameof(PixivAppApiNameResolver));
        OnPropertyChanged(nameof(PixivImageNameResolver));
        OnPropertyChanged(nameof(PixivImageNameResolver2));
        OnPropertyChanged(nameof(PixivOAuthNameResolver));
        OnPropertyChanged(nameof(PixivAccountNameResolver));
        OnPropertyChanged(nameof(PixivWebApiNameResolver));
    }

    public override void ValueSaving(IPropertySet values)
    {
        base.ValueSaving(values);

        var appApiNameSame = Settings.PixivAppApiNameResolver.SequenceEquals(PixivAppApiNameResolver);
        var imageNameSame = Settings.PixivImageNameResolver.SequenceEqual(PixivImageNameResolver);
        var imageName2Same = Settings.PixivImageNameResolver2.SequenceEqual(PixivImageNameResolver2);
        var oAuthNameSame = Settings.PixivOAuthNameResolver.SequenceEqual(PixivOAuthNameResolver);
        var accountNameSame = Settings.PixivAccountNameResolver.SequenceEqual(PixivAccountNameResolver);
        var webApiNameSame = Settings.PixivWebApiNameResolver.SequenceEqual(PixivWebApiNameResolver);

        var appApi = Settings.PixivAppApiNameResolver = [.. PixivAppApiNameResolver];
        var imageName = Settings.PixivImageNameResolver = [.. PixivImageNameResolver];
        var imageName2 = Settings.PixivImageNameResolver2 = [.. PixivImageNameResolver2];
        var oAuthName = Settings.PixivOAuthNameResolver = [.. PixivOAuthNameResolver];
        var accountName = Settings.PixivAccountNameResolver = [.. PixivAccountNameResolver];
        var webApiName = Settings.PixivWebApiNameResolver = [.. PixivWebApiNameResolver];

        values[nameof(AppSettings.PixivAppApiNameResolver)] = Converter.Convert(appApi);
        values[nameof(AppSettings.PixivImageNameResolver)] = Converter.Convert(imageName);
        values[nameof(AppSettings.PixivImageNameResolver2)] = Converter.Convert(imageName2);
        values[nameof(AppSettings.PixivOAuthNameResolver)] = Converter.Convert(oAuthName);
        values[nameof(AppSettings.PixivAccountNameResolver)] = Converter.Convert(accountName);
        values[nameof(AppSettings.PixivWebApiNameResolver)] = Converter.Convert(webApiName);

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
