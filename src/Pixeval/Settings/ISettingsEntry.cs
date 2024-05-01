using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.AppManagement;
using Pixeval.Utilities;
using WinUI3Utilities;
using WinUI3Utilities.Controls;
using Pixeval.Misc;
using Windows.Globalization;
using System.Drawing.Text;
using Pixeval.Controls;
using Pixeval.Download.MacroParser;
using Pixeval.Download.Macros;

namespace Pixeval.Settings;

public interface ISettingsEntry
{
    FrameworkElement Element { get; }

    void ValueReset();

    void ValueSaving();
}

public abstract class SettingsEntryBase<TSettings>(
    TSettings settings,
    string header,
    string description,
    IconGlyph headerIcon) : ISettingsEntry
{
    public abstract FrameworkElement Element { get; }

    public IconGlyph HeaderIcon { get; set; } = headerIcon;

    public string Header { get; set; } = header;

    public object DescriptionControl
    {
        get
        {
            if (DescriptionUri is not null)
            {
                var b = new HyperlinkButton { Content = Description };
                if (DescriptionUri.Scheme is "http" or "https")
                {
                    b.NavigateUri = DescriptionUri;
                    return b;
                }

                var uri = DescriptionUri;
                b.Click += (_, _) => _ = Launcher.LaunchUriAsync(uri);
                return b;
            }
            return Description;
        }
    }

    public string Description { get; set; } = description;

    public Uri? DescriptionUri { get; set; }

    public TSettings Settings { get; } = settings;

    public abstract void ValueReset();

    public virtual void ValueSaving()
    {
    }
}

public abstract class ClickableSettingsEntryBase<TSettings>(
    TSettings settings,
    string header,
    string description,
    IconGlyph headerIcon,
    Action clicked)
    : SettingsEntryBase<TSettings>(settings, header, description, headerIcon)
{
    public Action Clicked { get; set; } = clicked;

    public IconGlyph ActionIcon { get; set; } = IconGlyph.OpenInNewWindowE8A7;

    public override void ValueReset() { }
}


public class ClickableAppSettingsEntry(
    AppSettings settings,
    string header,
    string description,
    IconGlyph headerIcon,
    Action clicked)
    : ClickableSettingsEntryBase<AppSettings>(settings, header, description, headerIcon, clicked)
{
    public override ClickableSettingsCard Element => new() { Entry = this };
}

public abstract class ObservableSettingsEntryBase<TSettings>(
    TSettings settings,
    string header,
    string description,
    IconGlyph headerIcon)
    : SettingsEntryBase<TSettings>(settings, header, description, headerIcon), INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public static string SubHeader(WorkTypeEnum workType) => workType switch
    {
        WorkTypeEnum.Illustration => SettingsPageResources.IllustrationOptionEntryHeader,
        WorkTypeEnum.Manga => SettingsPageResources.MangaOptionEntryHeader,
        WorkTypeEnum.Ugoira => SettingsPageResources.UgoiraOptionEntryHeader,
        WorkTypeEnum.Novel => SettingsPageResources.NovelOptionEntryHeader,
        _ => throw new ArgumentOutOfRangeException()
    };

    public static IconGlyph SubHeaderIcon(WorkTypeEnum workType) => workType switch
    {
        WorkTypeEnum.Illustration => IconGlyph.PhotoE91B,
        WorkTypeEnum.Manga => IconGlyph.PictureE8B9,
        WorkTypeEnum.Ugoira => IconGlyph.GIFF4A9,
        WorkTypeEnum.Novel => IconGlyph.ReadingModeE736,
        _ => throw new ArgumentOutOfRangeException()
    };
}

public abstract class SingleValueSettingsEntryBase<TSettings>(
    TSettings settings,
    string header,
    string description,
    IconGlyph headerIcon,
    string propertyName)
    : ObservableSettingsEntryBase<TSettings>(settings, header, description, headerIcon)
{
    public PropertyInfo PropertyInfo { get; } = typeof(TSettings).GetProperty(propertyName) ?? ThrowHelper.Argument<string, PropertyInfo>(propertyName);

    public object? ValueBase
    {
        get => PropertyInfo.GetValue(Settings);
        set => PropertyInfo.SetValue(Settings, value);
    }

    public override void ValueReset() => OnPropertyChanged("Value");
}

public class BoolAppSettingsEntry(
    AppSettings appSettings,
    string header,
    string description,
    IconGlyph headerIcon,
    string propertyName)
    : SingleValueSettingsEntryBase<AppSettings>(appSettings, header, description, headerIcon, propertyName)
{
    public override BoolSettingsCard Element => new() { Entry = this };

    public bool Value
    {
        get => (bool)ValueBase!;
        set => ValueBase = value;
    }

    public Action<bool>? ValueChanged { get; set; }

    public BoolAppSettingsEntry(
        AppSettings appSettings,
        WorkTypeEnum workType,
        string propertyName)
        : this(appSettings, SubHeader(workType), "", SubHeaderIcon(workType), propertyName)
    {
    }
}

public class EnumAppSettingsEntry(
    AppSettings appSettings,
    string header,
    string description,
    IconGlyph headerIcon,
    string propertyName,
    Array array)
    : SingleValueSettingsEntryBase<AppSettings>(appSettings, header, description, headerIcon, propertyName)
{
    public override EnumSettingsCard Element => new() { Entry = this };

    public Enum Value
    {
        get => (Enum)ValueBase!;
        set => ValueBase = value;
    }

    public Action<Enum>? ValueChanged { get; set; }

    public Array EnumValues { get; set; } = array;

    public EnumAppSettingsEntry(
        AppSettings appSettings,
        WorkTypeEnum workType,
        string propertyName,
        Array array)
        : this(appSettings, SubHeader(workType), "", SubHeaderIcon(workType), propertyName, array)
    {
    }
}

public class EnumAppSettingsEntry<TEnum>(
    AppSettings appSettings,
    string header,
    string description,
    IconGlyph headerIcon,
    string propertyName)
    : EnumAppSettingsEntry(appSettings, header, description, headerIcon, propertyName, Enum.GetValues<TEnum>())
    where TEnum : struct, Enum
{
    public EnumAppSettingsEntry(
        AppSettings appSettings,
        WorkTypeEnum workType,
        string propertyName)
        : this(appSettings, SubHeader(workType), "", SubHeaderIcon(workType), propertyName)
    {
    }
}

public class StringAppSettingsEntry(
    AppSettings appSettings,
    string header,
    string description,
    IconGlyph headerIcon,
    string propertyName)
    : SingleValueSettingsEntryBase<AppSettings>(appSettings, header, description, headerIcon, propertyName)
{
    public override StringSettingsCard Element => new() { Entry = this };

    public string? Value
    {
        get => (string?)ValueBase;
        set => ValueBase = value;
    }

    public Action<string?>? ValueChanged { get; set; }

    public string? Placeholder { get; set; }

    public StringAppSettingsEntry(
        AppSettings appSettings,
        WorkTypeEnum workType,
        string propertyName)
        : this(appSettings, SubHeader(workType), "", SubHeaderIcon(workType), propertyName)
    {
    }
}

public class IntAppSettingsEntry(
    AppSettings appSettings,
    string header,
    string description,
    IconGlyph headerIcon,
    string propertyName)
    : SingleValueSettingsEntryBase<AppSettings>(appSettings, header, description, headerIcon, propertyName)
{
    public override IntSettingsCard Element => new() { Entry = this };

    public int Value
    {
        get => (int)ValueBase!;
        set => ValueBase = value;
    }

    public Action<int>? ValueChanged { get; set; }

    public string? Placeholder { get; set; }

    public double Min { get; set; } = double.NaN;

    public double Max { get; set; } = double.NaN;

    public IntAppSettingsEntry(
        AppSettings appSettings,
        WorkTypeEnum workType,
        string propertyName)
        : this(appSettings, SubHeader(workType), "", SubHeaderIcon(workType), propertyName)
    {
    }
}

public enum WorkTypeEnum
{
    Illustration,
    Manga,
    Ugoira,
    Novel
}

public abstract class MultiValuesSettingsEntryBase<TSettings>(
    TSettings settings,
    string header,
    string description,
    IconGlyph headerIcon,
    IReadOnlyList<SingleValueSettingsEntryBase<TSettings>> entries) : SettingsEntryBase<TSettings>(settings, header, description, headerIcon)
{
    public IReadOnlyList<SingleValueSettingsEntryBase<TSettings>> Entries { get; } = entries;

    public override void ValueReset()
    {
        foreach (var entry in Entries)
            entry.ValueReset();
    }
}

public class MultiValuesAppSettingsEntry(
    AppSettings appSettings,
    string header,
    string description,
    IconGlyph headerIcon,
    IReadOnlyList<SingleValueSettingsEntryBase<AppSettings>> entries)
    : MultiValuesSettingsEntryBase<AppSettings>(appSettings, header, description, headerIcon, entries)
{
    public override MultiValuesAppSettingsExpander Element => new() { Entry = this };
}

public interface ISettingsGroup : IEnumerable<ISettingsEntry>
{
    string Header { get; }
}

public class SimpleSettingsGroup(string header) : List<ISettingsEntry>, ISettingsGroup
{
    public string Header { get; } = header;
}

public class DateRangeWithSwitchAppSettingsEntry(
    AppSettings appSettings)
    : ObservableSettingsEntryBase<AppSettings>(appSettings, "", "", default)
{
    public override DateRangeWithSwitchSettingsExpander Element => new() { Entry = this };

    public override void ValueReset() => OnPropertyChanged(nameof(Settings));
}

public class IpWithSwitchAppSettingsEntry(
    AppSettings appSettings)
    : ObservableSettingsEntryBase<AppSettings>(appSettings, "", "", default)
{
    public override IpWithSwitchSettingsExpander Element => new() { Entry = this };

    public Action<bool>? ValueChanged { get; set; }

    public override void ValueReset()
    {
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

public class TokenizingAppSettingsEntry(
    AppSettings appSettings)
    : ObservableSettingsEntryBase<AppSettings>(appSettings, "", "", default)
{
    public override TokenizingSettingsExpander Element => new() { Entry = this };

    public override void ValueReset()
    {
        BlockedTags = [.. Settings.BlockedTags];
        OnPropertyChanged(nameof(BlockedTags));
    }

    public override void ValueSaving() => Settings.BlockedTags = [.. BlockedTags];

    public ObservableCollection<string> BlockedTags { get; set; } = [.. appSettings.BlockedTags];
}

public class DownloadMacroAppSettingsEntry(
    AppSettings appSettings)
    : ObservableSettingsEntryBase<AppSettings>(appSettings, "", "", default)
{
    public override DownloadMacroSettingsExpander Element => new() { Entry = this };

    public string DefaultDownloadPathMacro
    {
        get => Settings.DefaultDownloadPathMacro;
        set => Settings.DefaultDownloadPathMacro = value;
    }

    public override void ValueReset() => OnPropertyChanged(nameof(DefaultDownloadPathMacro));

    private static readonly IDictionary<string, string> _macroTooltips = new Dictionary<string, string>
    {
        ["ext"] = SettingsPageResources.ExtMacroTooltip,
        ["id"] = SettingsPageResources.IdMacroTooltip,
        ["title"] = SettingsPageResources.TitleMacroTooltip,
        ["artist_id"] = SettingsPageResources.ArtistIdMacroTooltip,
        ["artist_name"] = SettingsPageResources.ArtistNameMacroTooltip,
        ["if_r18"] = SettingsPageResources.IfR18MacroTooltip,
        ["if_r18g"] = SettingsPageResources.IfR18GMacroTooltip,
        ["if_ai"] = SettingsPageResources.IfAiMacroTooltip,
        ["if_illust"] = SettingsPageResources.IfIllustMacroTooltip,
        ["if_novel"] = SettingsPageResources.IfNovelMacroTooltip,
        ["if_manga"] = SettingsPageResources.IfMangaMacroTooltip,
        ["if_gif"] = SettingsPageResources.IfGifMacroTooltip,
        ["manga_index"] = SettingsPageResources.MangaIndexMacroTooltip
    };

    public static ICollection<StringRepresentableItem> AvailableMacros { get; } = MetaPathMacroAttributeHelper.GetAttachedTypeInstances()
        .Select(m => new StringRepresentableItem(_macroTooltips[m.Name], $"@{{{(m is IPredicate ? $"{m.Name}=" : m.Name)}}}"))
        .ToList();
}

public class FontAppSettingsEntry(
    AppSettings appSettings)
    : ObservableSettingsEntryBase<AppSettings>(appSettings, "", "", default)
{
    public static IEnumerable<string> AvailableFonts { get; }

    static FontAppSettingsEntry()
    {
        using var collection = new InstalledFontCollection();
        AvailableFonts = collection.Families.Select(t => t.Name);
    }

    public override FontSettingsCard Element => new() { Entry = this };

    public string AppFontFamilyName
    {
        get => Settings.AppFontFamilyName;
        set => Settings.AppFontFamilyName = value;
    }

    public override void ValueReset() => OnPropertyChanged(nameof(AppFontFamilyName));
}

public class LanguageAppSettingsEntry(
    AppSettings appSettings)
    : ObservableSettingsEntryBase<AppSettings>(appSettings, "", "", default)
{
    public override LanguageSettingsCard Element => new() { Entry = this };

    public static IEnumerable<LanguageModel> AvailableLanguages { get; } = [LanguageModel.DefaultLanguage, new("简体中文", "zh-Hans-CN"), new("Русский", "ru"), new("English (United States)", "en-US")];

    public LanguageModel AppLanguage
    {
        get => AvailableLanguages.FirstOrDefault(t => t.Name == ApplicationLanguages.PrimaryLanguageOverride) ?? LanguageModel.DefaultLanguage;
        set => ApplicationLanguages.PrimaryLanguageOverride = value.Name;
    }

    public override void ValueReset()
    {
        AppLanguage = LanguageModel.DefaultLanguage;
        OnPropertyChanged(nameof(AppLanguage));
    }
}
