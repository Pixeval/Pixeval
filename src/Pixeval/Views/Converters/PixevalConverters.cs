// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using AutoSettingsPage.Avalonia;
using AutoSettingsPage.Models;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using FluentIcons.Common;
using Mako.Model;
using Misaki;
using Pixeval.Controls;

namespace Pixeval.Views.Converters;

public static partial class PixevalConverters
{
    public static readonly NumberEllipsisConverter NumberEllipsis = NumberEllipsisConverter.Instance;

    public static readonly DateTimeShortDateConverter DateTimeShortDate = DateTimeShortDateConverter.Instance;

    public static readonly FuncValueConverter<int, int> PlusOne = new(i => i + 1);

    public static readonly FuncValueConverter<int, string> IntToString = new(value => value.ToString());

    public static readonly FuncValueConverter<HeartButtonState, IconVariant> StateToIconVariant = new(value => value switch
    {
        _ when (value & HeartButtonState.Pending) is not 0 => IconVariant.Filled,
        HeartButtonState.Checked => IconVariant.Color,
        _ => IconVariant.Regular
    });

    public static readonly FuncValueConverter<double, AspectRatio> ToAspectRatio = new(value => value);

    public static readonly EitherConverter EitherConverter = EitherConverter.Instance;

    public static readonly FuncValueConverter<ISettingsEntry, Control?> SettingsEntryConverter = new(value => value is null ? null : SettingsEntryHelper.GetControl(value));

    public static readonly FuncValueConverter<SafeRating, BadgeMode> SafeBadgeModeConverter = new(value =>
        value switch
        {
            { IsR18G: true } => BadgeMode.R18G,
            { IsR18: true } => BadgeMode.R18,
            _ => BadgeMode.None
        });

    public static readonly FuncValueConverter<XRestrict, BadgeMode> RestrictBadgeModeConverter = new(value =>
        value switch
        {
            XRestrict.R18G => BadgeMode.R18G,
            XRestrict.R18 => BadgeMode.R18,
            _ => BadgeMode.None
        });

    public static readonly StringFormatConverter StringFormatConverter = StringFormatConverter.Instance;

    public static readonly FuncValueConverter<IReadOnlyCollection<IImageFrame>, string?> ImagePickClosestConverter = new(value => value?.PickClosest(50, 50)?.ImageUri.OriginalString);

    public static readonly FuncValueConverter<string?, string?> HtmlToPlainText = new(value =>
        value is null
            ? null
            : WebUtility.HtmlDecode(HtmlTagRegex().Replace(LineBreakRegex().Replace(value, "\n"), "")));
    
    [GeneratedRegex("<br\\s*/?>", RegexOptions.IgnoreCase)]
    private static partial Regex LineBreakRegex();

    [GeneratedRegex("<[^>]+>")]
    private static partial Regex HtmlTagRegex();
}
