// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Pixeval.Controls;

/// <summary>
/// Displays modal content with optional primary, secondary, and close actions.
/// </summary>
[PseudoClasses(PcCompact)]
public class ContentDialog : ContentControl
{
    private const string PcCompact = ":compact";
    private const string DefaultButtonClass = "default";

    private const string PartTitlePresenter = "PART_TitlePresenter";
    private const string PartPrimaryButton = "PART_PrimaryButton";
    private const string PartSecondaryButton = "PART_SecondaryButton";
    private const string PartCloseButton = "PART_CloseButton";

    private Button? _primaryButton;
    private Button? _secondaryButton;
    private Button? _closeButton;
    private ContentPresenter? _titlePresenter;
    private ContentDialogHost? _host;
    private TaskCompletionSource<ContentDialogResult>? _completion;
    private int _isClosing;

    /// <summary>
    /// Defines the <see cref="Title"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> TitleProperty =
        AvaloniaProperty.Register<ContentDialog, object?>(nameof(Title));

    /// <summary>
    /// Defines the <see cref="TitleTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> TitleTemplateProperty =
        AvaloniaProperty.Register<ContentDialog, IDataTemplate?>(nameof(TitleTemplate));

    /// <summary>
    /// Defines the <see cref="PrimaryButtonText"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> PrimaryButtonTextProperty =
        AvaloniaProperty.Register<ContentDialog, string?>(nameof(PrimaryButtonText));

    /// <summary>
    /// Defines the <see cref="SecondaryButtonText"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> SecondaryButtonTextProperty =
        AvaloniaProperty.Register<ContentDialog, string?>(nameof(SecondaryButtonText));

    /// <summary>
    /// Defines the <see cref="CloseButtonText"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> CloseButtonTextProperty =
        AvaloniaProperty.Register<ContentDialog, string?>(nameof(CloseButtonText));

    /// <summary>
    /// Defines the <see cref="IsPrimaryButtonEnabled"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsPrimaryButtonEnabledProperty =
        AvaloniaProperty.Register<ContentDialog, bool>(nameof(IsPrimaryButtonEnabled), true);

    /// <summary>
    /// Defines the <see cref="IsSecondaryButtonEnabled"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsSecondaryButtonEnabledProperty =
        AvaloniaProperty.Register<ContentDialog, bool>(nameof(IsSecondaryButtonEnabled), true);

    /// <summary>
    /// Defines the <see cref="IsCloseButtonEnabled"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsCloseButtonEnabledProperty =
        AvaloniaProperty.Register<ContentDialog, bool>(nameof(IsCloseButtonEnabled), true);

    /// <summary>
    /// Defines the <see cref="DefaultButton"/> property.
    /// </summary>
    public static readonly StyledProperty<ContentDialogButton> DefaultButtonProperty =
        AvaloniaProperty.Register<ContentDialog, ContentDialogButton>(nameof(DefaultButton));

    /// <summary>
    /// Defines the <see cref="IsLightDismissEnabled"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsLightDismissEnabledProperty =
        AvaloniaProperty.Register<ContentDialog, bool>(nameof(IsLightDismissEnabled), true);

    /// <summary>
    /// Gets or sets the title content shown above the dialog body.
    /// </summary>
    public object? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Gets or sets the data template used to display <see cref="Title"/>.
    /// </summary>
    public IDataTemplate? TitleTemplate
    {
        get => GetValue(TitleTemplateProperty);
        set => SetValue(TitleTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the text for the primary action button. An empty value hides the button.
    /// </summary>
    public string? PrimaryButtonText
    {
        get => GetValue(PrimaryButtonTextProperty);
        set => SetValue(PrimaryButtonTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the text for the secondary action button. An empty value hides the button.
    /// </summary>
    public string? SecondaryButtonText
    {
        get => GetValue(SecondaryButtonTextProperty);
        set => SetValue(SecondaryButtonTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the text for the close action button. An empty value hides the button.
    /// </summary>
    public string? CloseButtonText
    {
        get => GetValue(CloseButtonTextProperty);
        set => SetValue(CloseButtonTextProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the primary action button can be invoked.
    /// </summary>
    public bool IsPrimaryButtonEnabled
    {
        get => GetValue(IsPrimaryButtonEnabledProperty);
        set => SetValue(IsPrimaryButtonEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the secondary action button can be invoked.
    /// </summary>
    public bool IsSecondaryButtonEnabled
    {
        get => GetValue(IsSecondaryButtonEnabledProperty);
        set => SetValue(IsSecondaryButtonEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the close action button can be invoked.
    /// </summary>
    public bool IsCloseButtonEnabled
    {
        get => GetValue(IsCloseButtonEnabledProperty);
        set => SetValue(IsCloseButtonEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets the button invoked by the Enter key and visually treated as the default action.
    /// </summary>
    public ContentDialogButton DefaultButton
    {
        get => GetValue(DefaultButtonProperty);
        set => SetValue(DefaultButtonProperty, value);
    }

    /// <summary>
    /// Gets or sets whether clicking outside the topmost dialog closes it with <see cref="ContentDialogResult.None"/>.
    /// </summary>
    public bool IsLightDismissEnabled
    {
        get => GetValue(IsLightDismissEnabledProperty);
        set => SetValue(IsLightDismissEnabledProperty, value);
    }

    /// <summary>
    /// Occurs when the primary action button is clicked, before the dialog starts closing.
    /// </summary>
    public event EventHandler<ContentDialog, ContentDialogButtonClickEventArgs>? PrimaryButtonClick;

    /// <summary>
    /// Occurs when the secondary action button is clicked, before the dialog starts closing.
    /// </summary>
    public event EventHandler<ContentDialog, ContentDialogButtonClickEventArgs>? SecondaryButtonClick;

    /// <summary>
    /// Occurs when the close action button is clicked, before the dialog starts closing.
    /// </summary>
    public event EventHandler<ContentDialog, ContentDialogButtonClickEventArgs>? CloseButtonClick;

    /// <summary>
    /// Occurs when the dialog is closing. Handlers can cancel or defer the close.
    /// </summary>
    public event EventHandler<ContentDialog, ContentDialogClosingEventArgs>? Closing;

    /// <summary>
    /// Occurs after the dialog has closed.
    /// </summary>
    public event EventHandler<ContentDialog, ContentDialogClosedEventArgs>? Closed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentDialog"/> class.
    /// </summary>
    public ContentDialog()
    {
        AddHandler(KeyDownEvent, OnDialogKeyDown, RoutingStrategies.Tunnel);
    }

    static ContentDialog()
    {
        DefaultButtonProperty.Changed.AddClassHandler<ContentDialog>((x, _) => x.UpdateDefaultButton());
    }

    /// <summary>
    /// Shows this dialog on the specified host.
    /// </summary>
    /// <param name="host">The host that will display the dialog.</param>
    /// <param name="cancellationToken">A token that closes the dialog with <see cref="ContentDialogResult.None"/> when canceled.</param>
    /// <returns>The result selected by the user.</returns>
    public Task<ContentDialogResult> ShowAsync(ContentDialogHost host, CancellationToken cancellationToken = default) =>
        host.ShowAsync(this, cancellationToken);

    /// <summary>
    /// Requests that this dialog close with the specified result.
    /// </summary>
    /// <param name="result">The result to report to callers awaiting <see cref="ShowAsync(ContentDialogHost, CancellationToken)"/>.</param>
    /// <returns>The result passed to the close request.</returns>
    public async Task<ContentDialogResult> HideAsync(ContentDialogResult result = ContentDialogResult.None)
    {
        await RequestCloseAsync(result, ToButton(result));
        return result;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _primaryButton?.Click -= PrimaryButtonOnClick;
        _secondaryButton?.Click -= SecondaryButtonOnClick;
        _closeButton?.Click -= CloseButtonOnClick;

        _titlePresenter = e.NameScope.Find<ContentPresenter>(PartTitlePresenter);
        _primaryButton = e.NameScope.Find<Button>(PartPrimaryButton);
        _secondaryButton = e.NameScope.Find<Button>(PartSecondaryButton);
        _closeButton = e.NameScope.Find<Button>(PartCloseButton);

        _primaryButton?.Click += PrimaryButtonOnClick;
        _secondaryButton?.Click += SecondaryButtonOnClick;
        _closeButton?.Click += CloseButtonOnClick;
        UpdateDefaultButton();
    }

    internal void PrepareForShow(ContentDialogHost host)
    {
        if (_completion is not null)
            throw new InvalidOperationException($"This {nameof(ContentDialog)} is already shown.");

        _host = host;
        _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        _isClosing = 0;
    }

    internal Task<ContentDialogResult> WaitForCloseAsync()
    {
        if (_completion is null)
            throw new InvalidOperationException($"This {nameof(ContentDialog)} has not been shown.");

        return _completion.Task;
    }

    internal void CompleteClose(ContentDialogResult result)
    {
        Closed?.Invoke(this, new(result));
        _host = null;
        _completion = null;
        Interlocked.Exchange(ref _isClosing, 0);
    }

    internal void SetCompact(bool compact) => PseudoClasses.Set(PcCompact, compact);

    internal void FocusInitialElement()
    {
        Focus(NavigationMethod.Directional);
    }

    private async void PrimaryButtonOnClick(object? sender, RoutedEventArgs e) =>
        await RequestCloseAsync(ContentDialogResult.Primary, ContentDialogButton.Primary);

    private async void SecondaryButtonOnClick(object? sender, RoutedEventArgs e) =>
        await RequestCloseAsync(ContentDialogResult.Secondary, ContentDialogButton.Secondary);

    private async void CloseButtonOnClick(object? sender, RoutedEventArgs e) =>
        await RequestCloseAsync(ContentDialogResult.Close, ContentDialogButton.Close);

    private async void OnDialogKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is Key.Enter)
        {
            if (await InvokeDefaultButtonAsync())
                e.Handled = true;
            return;
        }

        if (e.Key is Key.Escape && IsButtonAvailable(ContentDialogButton.Close))
        {
            await RequestCloseAsync(ContentDialogResult.Close, ContentDialogButton.Close);
            e.Handled = true;
        }
    }

    private async Task<bool> InvokeDefaultButtonAsync()
    {
        if (!IsButtonAvailable(DefaultButton))
            return false;

        await RequestCloseAsync(ToResult(DefaultButton), DefaultButton);
        return true;
    }

    private async Task RequestCloseAsync(ContentDialogResult result, ContentDialogButton button)
    {
        if (_completion is null || _host is null)
            return;

        if (Interlocked.Exchange(ref _isClosing, 1) is 1)
            return;

        var buttonArgs = new ContentDialogButtonClickEventArgs(button, result);
        switch (button)
        {
            case ContentDialogButton.Primary:
                PrimaryButtonClick?.Invoke(this, buttonArgs);
                break;
            case ContentDialogButton.Secondary:
                SecondaryButtonClick?.Invoke(this, buttonArgs);
                break;
            case ContentDialogButton.Close:
                CloseButtonClick?.Invoke(this, buttonArgs);
                break;
        }

        await buttonArgs.WaitForDeferralsAsync();

        if (buttonArgs.Cancel)
        {
            Interlocked.Exchange(ref _isClosing, 0);
            return;
        }

        var closingArgs = new ContentDialogClosingEventArgs(result);
        Closing?.Invoke(this, closingArgs);
        await closingArgs.WaitForDeferralsAsync();

        if (closingArgs.Cancel)
        {
            Interlocked.Exchange(ref _isClosing, 0);
            return;
        }

        _completion.TrySetResult(result);
    }

    private bool IsButtonAvailable(ContentDialogButton button) =>
        button switch
        {
            ContentDialogButton.Primary => _primaryButton is { IsVisible: true, IsEnabled: true },
            ContentDialogButton.Secondary => _secondaryButton is { IsVisible: true, IsEnabled: true },
            ContentDialogButton.Close => _closeButton is { IsVisible: true, IsEnabled: true },
            _ => false
        };

    private void UpdateDefaultButton()
    {
        _primaryButton?.Classes.Set(DefaultButtonClass, DefaultButton is ContentDialogButton.Primary);
        _secondaryButton?.Classes.Set(DefaultButtonClass, DefaultButton is ContentDialogButton.Secondary);
        _closeButton?.Classes.Set(DefaultButtonClass, DefaultButton is ContentDialogButton.Close);
    }

    private static ContentDialogResult ToResult(ContentDialogButton button) =>
        button switch
        {
            ContentDialogButton.Primary => ContentDialogResult.Primary,
            ContentDialogButton.Secondary => ContentDialogResult.Secondary,
            ContentDialogButton.Close => ContentDialogResult.Close,
            _ => ContentDialogResult.None
        };

    private static ContentDialogButton ToButton(ContentDialogResult result) =>
        result switch
        {
            ContentDialogResult.Primary => ContentDialogButton.Primary,
            ContentDialogResult.Secondary => ContentDialogButton.Secondary,
            ContentDialogResult.Close => ContentDialogButton.Close,
            _ => ContentDialogButton.None
        };
}
