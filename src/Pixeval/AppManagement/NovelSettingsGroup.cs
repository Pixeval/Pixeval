// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json.Serialization;
using AutoSettingsPage;
using Avalonia.Media;
using FluentIcons.Common;
using Pixeval.Models.Options;
using SharpYaml.Serialization;
using static Pixeval.AppSettingsResources;

namespace Pixeval.AppManagement;

public record NovelSettingsGroup
{
    [JsonIgnore]
    public Func<ApplicationTheme> ActualThemeProvider { get; set; } = static () => ApplicationTheme.Default;

    [YamlConverter(typeof(YamlColorConverter))]
    public uint NovelFontColorInDarkMode { get; set; } = 0xFFFFFFFF;

    [YamlConverter(typeof(YamlColorConverter))]
    public uint NovelFontColorInLightMode { get; set; } = 0xFF000000;

    [YamlConverter(typeof(YamlColorConverter))]
    public uint NovelBackgroundInDarkMode { get; set; }

    [YamlConverter(typeof(YamlColorConverter))]
    public uint NovelBackgroundInLightMode { get; set; }

    [SettingsEntry(Symbol.LineThickness, NovelSettingsFontWeightEntryHeader, NovelSettingsFontWeightEntryDescription, NovelSettingsFontWeightEntryPlaceholder)]
    public FontWeight NovelFontWeight { get; set; } = FontWeight.Normal;

    [SettingsEntry(Symbol.TextFont, NovelSettingsFontFamilyEntryHeader, AppFontFamilyEntryDescription, AppFontFamilyEntryPlaceholder)]
    public ObservableCollection<string> NovelFontFamily { get; set; } = [];

    [SettingsEntry(Symbol.TextFontSize, NovelSettingsFontSizeEntryHeader, NovelSettingsFontSizeEntryDescription)]
    public int NovelFontSize { get; set; } = 14;

    [SettingsEntry(Symbol.TextLineSpacing, NovelSettingsLineHeightEntryHeader, NovelSettingsLineHeightEntryDescription)]
    public int NovelLineHeight { get; set; } = 28;

    [SettingsEntry(Symbol.AutoFitWidth, NovelSettingsMaxWidthEntryHeader, NovelSettingsMaxWidthEntryDescription)]
    public int NovelMaxWidth { get; set; } = 1000;

    [JsonIgnore]
    [SettingsEntry(Symbol.ColorBackground, NovelSettingsBackgroundEntryHeader, NovelSettingsBackgroundEntryDescription)]
    public uint NovelBackground
    {
        get => ActualTheme is ApplicationTheme.Light ? NovelBackgroundInLightMode : NovelBackgroundInDarkMode;
        set
        {
            if (ActualTheme is ApplicationTheme.Light)
                NovelBackgroundInLightMode = value;
            else
                NovelBackgroundInDarkMode = value;
        }
    }

    [JsonIgnore]
    [SettingsEntry(Symbol.TextColor, NovelSettingsFontColorEntryHeader, NovelSettingsFontColorEntryDescription)]
    public uint NovelFontColor
    {
        get => ActualTheme is ApplicationTheme.Light ? NovelFontColorInLightMode : NovelFontColorInDarkMode;
        set
        {
            if (ActualTheme is ApplicationTheme.Light)
                NovelFontColorInLightMode = value;
            else
                NovelFontColorInDarkMode = value;
        }
    }

    private ApplicationTheme ActualTheme => ActualThemeProvider();
}

public class YamlColorConverter : YamlConverter<uint>
{
    /// <inheritdoc />
    public override uint Read(YamlReader reader)
    {
        var text = reader.ScalarValue!;
        reader.Read();
        if (text is not ['#', .. { Length: 8 } color])
            throw new FormatException("Invalid color format.");
        return uint.Parse(color, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
    }

    /// <inheritdoc />
    public override void Write(YamlWriter writer, uint value)
    {
        writer.WriteScalar($"#{value:X8}");
    }
}
