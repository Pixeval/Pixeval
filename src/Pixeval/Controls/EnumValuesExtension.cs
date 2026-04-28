// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using AutoSettingsPage;
using Mako.Global.Enum;
using Microsoft.UI.Xaml.Markup;
using Pixeval.Util;
using WinUI3Utilities;

namespace Pixeval.Controls;

[MarkupExtensionReturnType(ReturnType = typeof(IReadOnlyList<IReadOnlyStringPair<object>>))]
public sealed partial class EnumValuesExtension : MarkupExtension
{
    public EnumValuesEnum Type { get; set; }

    protected override object ProvideValue()
    {
        return Type switch
        {
            EnumValuesEnum.WorkType => WorkType.Pairs,
            EnumValuesEnum.SimpleWorkType => SimpleWorkType.Pairs,
            EnumValuesEnum.WorkSortOption => WorkSortOption.Pairs,
            EnumValuesEnum.PrivacyPolicy => PrivacyPolicy.Pairs,
            EnumValuesEnum.DownloadListOption => DownloadListOption.Pairs,
            _ => ThrowHelper.ArgumentOutOfRange<EnumValuesEnum, object>(Type)
        };
    }
}

public enum EnumValuesEnum
{
    WorkType,
    SimpleWorkType,
    WorkSortOption,
    PrivacyPolicy,
    DownloadListOption
}
