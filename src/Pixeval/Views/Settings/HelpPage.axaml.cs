// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.AppManagement;
using Pixeval.I18N;
using Pixeval.Mcp;
using Pixeval.Models.McpServer;

namespace Pixeval.Views.Settings;

public partial class HelpPage : ContentPage
{
    public static readonly DirectProperty<HelpPage, string> McpToolsStatusProperty =
        AvaloniaProperty.RegisterDirect<HelpPage, string>(nameof(McpToolsStatus), static page => page.McpToolsStatus);

    private bool _isLoadingMcpTools;

    public HelpPage()
    {
        InitializeComponent();
        McpToolsStatus = I18NManager.GetResource(HelpPageResources.McpToolsStatusDisabled);
    }

    public ObservableCollection<McpToolItemViewModel> McpTools { get; } = [];

    public string McpToolsStatus
    {
        get;
        private set => SetAndRaise(McpToolsStatusProperty, ref field, value);
    } = "";

    /// <inheritdoc />
    protected override async void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        await ReloadMcpToolsAsync();
    }

    private async void RefreshMcpToolsButton_OnClicked(object? sender, RoutedEventArgs e) =>
        await ReloadMcpToolsAsync();

    private async Task ReloadMcpToolsAsync()
    {
        if (_isLoadingMcpTools)
            return;

        var settings = App.AppViewModel.AppSettings.McpSettings;
        if (!settings.EnableServer)
        {
            McpTools.Clear();
            McpToolsStatus = I18NManager.GetResource(HelpPageResources.McpToolsStatusDisabled);
            return;
        }

        if (App.AppViewModel.AppServiceProvider.GetService<IPixevalMcpService>() is not { } service)
        {
            McpTools.Clear();
            McpToolsStatus = I18NManager.GetResource(HelpPageResources.McpToolsStatusUnavailable);
            return;
        }

        var configuredEndpoint = new Uri($"http://127.0.0.1:{settings.Port}{PixevalMcpHttpServer.DefaultPath}");
        _isLoadingMcpTools = true;
        RefreshMcpToolsButton.IsEnabled = false;
        McpToolsStatus = I18NManager.GetResource(HelpPageResources.McpToolsStatusLoading, configuredEndpoint);

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await service.ApplySettingsAsync(cts.Token);
            if (service.Endpoint is not { } endpoint)
            {
                McpTools.Clear();
                McpToolsStatus = I18NManager.GetResource(HelpPageResources.McpToolsStatusNotRunning);
                return;
            }

            var tools = await RequestMcpToolsAsync(endpoint, cts.Token);
            McpTools.Clear();
            foreach (var tool in tools)
                McpTools.Add(tool);

            McpToolsStatus = tools.Count is 0
                ? I18NManager.GetResource(HelpPageResources.McpToolsStatusEmpty)
                : I18NManager.GetResource(HelpPageResources.McpToolsStatusLoaded, endpoint, tools.Count);
        }
        catch (Exception e) when (e is HttpRequestException or JsonException or InvalidOperationException or OperationCanceledException)
        {
            McpTools.Clear();
            McpToolsStatus = I18NManager.GetResource(HelpPageResources.McpToolsStatusFailed, TrimErrorMessage(e.Message));
        }
        finally
        {
            RefreshMcpToolsButton.IsEnabled = true;
            _isLoadingMcpTools = false;
        }
    }
}
