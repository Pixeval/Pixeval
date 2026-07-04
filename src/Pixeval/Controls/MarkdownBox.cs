// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Markdown.Avalonia;
using Markdown.Avalonia.Html;
using Markdown.Avalonia.StyleCollections;
using Markdown.Avalonia.Svg;
using Markdown.Avalonia.Utils;
using Misaki;
using Pixeval.AppManagement;
using Pixeval.Utilities;
using Pixeval.Utilities.GitHub;
using Pixeval.Utilities.IO.Caching;
using Pixeval.Views.Viewers;

namespace Pixeval.Controls;

public class MarkdownBox : MarkdownScrollViewer
{
    public MarkdownBox()
    {
        SelectionEnabled = true;
        // 或 MarkdownStyleFluentAvalonia
        MarkdownStyle = new MarkdownStyleFluentTheme();
        Plugins.Plugins.Add(new HtmlPlugin());
        Plugins.Plugins.Add(new SvgFormat());
        Plugins.PathResolver = new PixevalPathResolver();
        Plugins.HyperlinkCommand = new RelayCommand<string>(OnHyperlinkClicked);
    }

    private async void OnHyperlinkClicked(string? url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return;
        if (TopLevel.GetTopLevel(this) is not
            {
                ViewContainer: { } viewContainer,
                Launcher: { } launcher
            })
            return;
        if (uri.Scheme is not AppInfo.AppProtocol)
            await launcher.LaunchUriAsync(uri);
        else if (uri.Host is "illust" && uri.AbsolutePath.Trim('/') is { } id)
            viewContainer.CreateIllustrationPage(id, IPlatformInfo.Pixiv);
    }

    private sealed class PixevalPathResolver : IPathResolver
    {
        private readonly DefaultPathResolver _defaultPathResolver = new();

        /// <inheritdoc />
        public string? AssetPathRoot
        {
            get;
            set
            {
                field = value;
                _defaultPathResolver.AssetPathRoot = value;
            }
        }

        /// <inheritdoc />
        public IEnumerable<string>? CallerAssemblyNames
        {
            get;
            set
            {
                field = value;
                _defaultPathResolver.CallerAssemblyNames = value;
            }
        }

        /// <inheritdoc />
        public async Task<Stream?> ResolveImageResource(string relativeOrAbsolutePath)
        {
            if (Uri.TryCreate(relativeOrAbsolutePath, UriKind.Absolute, out var uri)
                && uri.Scheme is "http" or "https")
            {
                return await CacheHelper.GetImageStreamAsync(ResolvePlatform(uri), uri.OriginalString).ConfigureAwait(false);
            }

            return await (_defaultPathResolver.ResolveImageResource(relativeOrAbsolutePath) ?? Task.FromResult<Stream?>(null)).ConfigureAwait(false);
        }

        private static string ResolvePlatform(Uri uri)
        {
            var host = uri.Host;
            if (GitHubHttpOptions.IsGitHubHost(host))
                return GitHubHttpClientProvider.PlatformKey;
            if (IsHostOrSubdomain(host, "pixiv.net") || IsHostOrSubdomain(host, "pximg.net"))
                return IPlatformInfo.Pixiv;
            if (IsHostOrSubdomain(host, "donmai.us"))
                return IPlatformInfo.Danbooru;
            return IPlatformInfo.All;
        }

        private static bool IsHostOrSubdomain(string host, string domain) =>
            host.Equals(domain, StringComparison.OrdinalIgnoreCase)
            || host.EndsWith("." + domain, StringComparison.OrdinalIgnoreCase);
    }
}
