// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoSettingsPage.Models;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Extensions.Common;
using Pixeval.Extensions.Common.Commands.Transformers;
using Pixeval.Extensions.Common.Downloaders;
using Pixeval.Extensions.Common.FormatProviders;
using Pixeval.Extensions.Common.Settings;
using Pixeval.Mcp.Dtos;
using Pixeval.Models.Extensions;

namespace Pixeval.Utilities.McpServer;

public sealed partial class PixevalMcpService
{
    public PixevalExtensionListDto Extensions()
    {
        var service = ViewModel.AppServiceProvider.GetRequiredService<ExtensionService>();
        var hosts = service.HostModels
            .Select(model => ToExtensionHostDto(service, model))
            .ToArray();
        var typeStatistics = hosts
            .SelectMany(static host => host.ExtensionTypes)
            .GroupBy(static type => type, StringComparer.OrdinalIgnoreCase)
            .Select(static group => new PixevalExtensionTypeStatisticDto(group.Key, group.Count()))
            .OrderBy(static item => item.Type, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        return new(
            hosts.Length,
            ExtensionService.CurrentVersion,
            ExtensionService.NativeLibraryExtension,
            typeStatistics,
            hosts);
    }

    private static PixevalExtensionHostDto ToExtensionHostDto(
        ExtensionService service,
        ExtensionsHostModel model)
    {
        var settings = service.SettingsGroups
            .Where(group => ReferenceEquals(group.Model, model))
            .SelectMany(static group => group.Cast<IExtensionSettingEntry>())
            .Select(ToExtensionSettingsEntryDto)
            .ToArray();
        return new(
            model.Name,
            model.Description,
            model.Author,
            model.Version,
            model.IsActive,
            model.IsPendingUninstall,
            model.Priority,
            model.Link?.ToString(),
            model.HelpLink?.ToString(),
            Path.GetFileName(model.HostLibraryPath),
            model.UninstallTargetRelativePath,
            [.. model.Extensions.SelectMany(GetExtensionTypeNames).Distinct(StringComparer.OrdinalIgnoreCase)],
            settings);
    }

    private static PixevalExtensionSettingsEntryDto ToExtensionSettingsEntryDto(IExtensionSettingEntry entry)
    {
        var (min, max, step) = GetNumberRange(entry);
        return new(
            entry.Token,
            entry.Header,
            entry.Description,
            GetValueKind(entry),
            entry is IReadOnlySingleValueSettingsEntry single ? single.Placeholder : null,
            entry.DescriptionUri?.ToString(),
            min,
            max,
            step,
            GetEnumItems(entry));
    }

    private static IReadOnlyList<string> GetExtensionTypeNames(IExtension extension)
    {
        var result = new List<string>();
        if (extension is IEntryExtension)
            result.Add("entry");
        if (extension is IImageTransformerCommandExtension)
            result.Add("image_transformer");
        if (extension is ITextTransformerCommandExtension)
            result.Add("text_transformer");
        if (extension is IDownloaderExtension)
            result.Add("downloader");
        if (extension is IStaticImageFormatProviderExtension)
            result.Add("static_image_format_provider");
        if (extension is IAnimatedImageFormatProviderExtension)
            result.Add("animated_image_format_provider");
        if (extension is INovelFormatProviderExtension)
            result.Add("novel_format_provider");
        if (extension is ISettingsExtension)
            result.Add("settings");

        if (result.Count is 0)
            result.Add("extension");
        return result;
    }

    private static string GetValueKind(ISettingsEntry entry) =>
        entry switch
        {
            ISingleValueSettingsEntry<bool> => "bool",
            ISingleValueSettingsEntry<string> => "string",
            ISingleValueSettingsEntry<int> => "int",
            ISingleValueSettingsEntry<double> => "double",
            ISingleValueSettingsEntry<uint> => "color",
            ISingleValueSettingsEntry<DateTimeOffset> => "datetime",
            IEnumSettingsEntry<object> => "enum",
            IReadOnlySingleValueSettingsEntry => "single_value",
            _ => "settings_entry"
        };

    private static (double? Min, double? Max, double? Step) GetNumberRange(ISettingsEntry entry) =>
        entry switch
        {
            INumberSettingsEntry<int> number => (number.Min, number.Max, number.Step),
            INumberSettingsEntry<double> number => (number.Min, number.Max, number.Step),
            _ => (null, null, null)
        };

    private static IReadOnlyList<PixevalExtensionEnumItemDto> GetEnumItems(ISettingsEntry entry) =>
        entry is IEnumSettingsEntry<object> enumEntry
            ?
            [
                .. enumEntry.EnumItems.Select(static item => new PixevalExtensionEnumItemDto(
                    item.Description,
                    item.Value?.ToString() ?? ""))
            ]
            : [];
}
