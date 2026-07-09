// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq;
using FluentIcons.Common;
using Pixeval.Utilities;

namespace Pixeval.Models.Navigation;

public abstract record NavigationMenuItem(
    string Header,
    string? HeaderSource,
    Symbol Icon,
    bool NeedLogin,
    IReadOnlyList<NavigationMenuItem> Children)
{
    public NavigationYamlItem ToYamlItem()
    {
        return this switch
        {
            NavigationPageItem page => ToYamlPageItem(page),
            NavigationFolderItem folder => new()
            {
                Folder = folder.HeaderSource ?? folder.Header,
                Icon = folder.Icon.ToString(),
                Children = folder.Children.Select(child => child.ToYamlItem()).ToArray()
            },
            _ => throw new InvalidOperationException("Unknown navigation menu item type")
        };

        static NavigationYamlItem ToYamlPageItem(NavigationPageItem item)
        {
            var result = new NavigationYamlItem { Page = item.PageKey };
            if (!NavigationPageRegistry.TryGetPage(item.PageKey, out var definition))
                return result;

            if (item.HeaderSource is { } headerSource)
                result.Title = headerSource;
            else if (item.Header != definition.Header)
                result.Title = item.Header;
            if (item.Icon != definition.Icon)
                result.Icon = item.Icon.ToString();

            return result;
        }
    }
}

public sealed record NavigationPageItem(
    Type PageType,
    string PageKey,
    string Header,
    string? HeaderSource,
    Symbol Icon,
    bool NeedLogin) : NavigationMenuItem(Header, HeaderSource, Icon, NeedLogin, []);

public sealed record NavigationFolderItem(
    string Header,
    string? HeaderSource,
    Symbol Icon,
    bool NeedLogin,
    IReadOnlyList<NavigationMenuItem> Children) : NavigationMenuItem(Header, HeaderSource, Icon, NeedLogin, Children);
