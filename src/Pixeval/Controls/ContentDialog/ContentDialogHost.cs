// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Pixeval.Controls;

/// <summary>
/// Hosts one or more <see cref="ContentDialog"/> instances in a <see cref="TopLevel"/> adorner layer.
/// </summary>
public class ContentDialogHost : TemplatedControl
{
    private const string pcOpen = ":open";
    private const string pcCompact = ":compact";
    private const string partRoot = "PART_Root";
    private const string partDialogPanel = "PART_DialogPanel";

    private readonly ObservableCollection<ContentDialog> _dialogs = [];
    private TopLevel? _host;
    private AdornerLayer? _adornerLayer;
    private Control? _root;
    private Panel? _dialogPanel;

    /// <summary>
    /// Defines the <see cref="IsOpen"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<ContentDialogHost, bool>(nameof(IsOpen));

    /// <summary>
    /// Defines the <see cref="CompactBreakpoint"/> property.
    /// </summary>
    public static readonly StyledProperty<double> CompactBreakpointProperty =
        AvaloniaProperty.Register<ContentDialogHost, double>(nameof(CompactBreakpoint), 560);

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentDialogHost"/> class.
    /// </summary>
    public ContentDialogHost()
    {
        SizeChanged += OnSizeChanged;
    }

    /// <summary>
    /// Initializes and installs a new instance of the <see cref="ContentDialogHost"/> class.
    /// </summary>
    /// <param name="host">The top-level control that owns the dialog host.</param>
    public ContentDialogHost(TopLevel host)
        : this()
    {
        InstallFromTopLevel(host);
    }

    /// <summary>
    /// Gets the dialogs currently shown by this host, ordered from bottom to top.
    /// </summary>
    public IReadOnlyList<ContentDialog> Dialogs => _dialogs;

    /// <summary>
    /// Gets a value indicating whether at least one dialog is open.
    /// </summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        private set => SetValue(IsOpenProperty, value);
    }

    /// <summary>
    /// Gets or sets the width at which dialogs switch to the compact bottom-sheet layout.
    /// </summary>
    public double CompactBreakpoint
    {
        get => GetValue(CompactBreakpointProperty);
        set => SetValue(CompactBreakpointProperty, value);
    }

    /// <summary>
    /// Installs this host into the adorner layer of the specified top-level control.
    /// </summary>
    /// <param name="host">The top-level control that owns the dialog host.</param>
    public void InstallFromTopLevel(TopLevel host)
    {
        if (_host == host)
            return;

        Uninstall();

        _host = host;
        _host.TemplateApplied += HostOnTemplateApplied;
        InstallCore();
    }

    /// <summary>
    /// Shows a dialog on this host and completes when that dialog is closed.
    /// </summary>
    /// <param name="dialog">The dialog to show.</param>
    /// <param name="cancellationToken">A token that closes the dialog with <see cref="ContentDialogResult.None"/> when canceled.</param>
    /// <returns>The result selected by the user.</returns>
    public async Task<ContentDialogResult> ShowAsync(ContentDialog dialog, CancellationToken cancellationToken = default)
    {
        Dispatcher.UIThread.VerifyAccess();

        if (_host is null)
            throw new InvalidOperationException("ContentDialogHost has not been installed.");

        dialog.PrepareForShow(this);
        _dialogs.Add(dialog);
        AddDialogToPanel(dialog);
        IsOpen = true;
        UpdateOpenState();
        UpdateDialogState();

        var result = ContentDialogResult.None;
        try
        {
            await using var registration = cancellationToken.Register(() =>
                Dispatcher.UIThread.Post(() => _ = dialog.HideAsync()));

            await Dispatcher.UIThread.InvokeAsync(dialog.FocusInitialElement, DispatcherPriority.Loaded);
            result = await dialog.WaitForCloseAsync();
            return result;
        }
        finally
        {
            var wasTopmost = _dialogs.Count > 0 && ReferenceEquals(_dialogs[^1], dialog);
            _dialogs.Remove(dialog);
            _dialogPanel?.Children.Remove(dialog);
            IsOpen = _dialogs.Count > 0;
            UpdateOpenState();
            UpdateDialogState();

            if (wasTopmost && _dialogs.Count > 0)
                await Dispatcher.UIThread.InvokeAsync(_dialogs[^1].FocusInitialElement, DispatcherPriority.Loaded);

            dialog.CompleteClose(result);
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var size = GetLayoutSize(availableSize);
        base.MeasureOverride(size);
        return size;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = GetLayoutSize(finalSize);
        base.ArrangeOverride(size);
        return size;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_root is not null)
            _root.PointerPressed -= RootOnPointerPressed;

        _root = e.NameScope.Find<Control>(partRoot);
        _dialogPanel = e.NameScope.Find<Panel>(partDialogPanel);

        if (_root is not null)
            _root.PointerPressed += RootOnPointerPressed;

        SyncPanelChildren();
        UpdateOpenState();
        UpdateDialogState();
    }

    private void HostOnTemplateApplied(object? sender, TemplateAppliedEventArgs e) => InstallCore();

    private void InstallCore()
    {
        if (_host is null)
            return;

        var layer = AdornerLayer.GetAdornerLayer(_host);
        if (layer is null || ReferenceEquals(_adornerLayer, layer))
            return;

        _adornerLayer?.Children.Remove(this);

        _adornerLayer = layer;
        AdornerLayer.SetAdornedElement(this, _host);
        AdornerLayer.SetIsClipEnabled(this, false);

        if (!_adornerLayer.Children.Contains(this))
            _adornerLayer.Children.Add(this);
    }

    private void Uninstall()
    {
        _host?.TemplateApplied -= HostOnTemplateApplied;

        _adornerLayer?.Children.Remove(this);

        _host = null;
        _adornerLayer = null;
    }

    private void AddDialogToPanel(ContentDialog dialog)
    {
        if (_dialogPanel is not null && !_dialogPanel.Children.Contains(dialog))
            _dialogPanel.Children.Add(dialog);
    }

    private void SyncPanelChildren()
    {
        if (_dialogPanel is null)
            return;

        _dialogPanel.Children.Clear();
        foreach (var dialog in _dialogs)
            AddDialogToPanel(dialog);
    }

    private void RootOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_dialogs.Count is 0)
            return;

        if (IsFromDialog(e.Source as Visual))
            return;

        var topmost = _dialogs[^1];
        if (topmost.IsLightDismissEnabled)
            _ = topmost.HideAsync();
    }

    private bool IsFromDialog(Visual? source)
    {
        while (source is not null)
        {
            if (source is ContentDialog dialog && _dialogs.Contains(dialog))
                return true;

            source = source.GetVisualParent();
        }

        return false;
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e) => UpdateDialogState();

    private Size GetLayoutSize(Size size)
    {
        var hostSize = _host?.ClientSize ?? default;
        var width = double.IsInfinity(size.Width) || double.IsNaN(size.Width) || size.Width <= 0
            ? hostSize.Width
            : size.Width;
        var height = double.IsInfinity(size.Height) || double.IsNaN(size.Height) || size.Height <= 0
            ? hostSize.Height
            : size.Height;

        return new(Math.Max(0, width), Math.Max(0, height));
    }

    private void UpdateOpenState()
    {
        PseudoClasses.Set(pcOpen, IsOpen);
        IsHitTestVisible = IsOpen;
        IsVisible = IsOpen;
    }

    private void UpdateDialogState()
    {
        var width = _host?.ClientSize.Width ?? Bounds.Width;
        var compact = width > 0 && width <= CompactBreakpoint;
        PseudoClasses.Set(pcCompact, compact);

        var topmost = _dialogs.Count > 0 ? _dialogs[^1] : null;
        foreach (var dialog in _dialogs)
        {
            dialog.SetCompact(compact);
            dialog.IsHitTestVisible = ReferenceEquals(dialog, topmost);
        }
    }
}
