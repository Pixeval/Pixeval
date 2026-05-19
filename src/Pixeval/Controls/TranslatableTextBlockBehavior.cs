// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Extensions.Common.Commands.Transformers;
using Pixeval.Models.Extensions;

namespace Pixeval.Controls;

public static class TranslatableTextBlockBehavior
{
    private static ControlTheme CompactFlyoutPresenterTheme { get; } = new ControlTheme(typeof(FlyoutPresenter))
    {
        Setters =
        {
            new Setter(Layoutable.MinWidthProperty, 0d),
            new Setter(Layoutable.MinHeightProperty, 0d),
            new Setter(TemplatedControl.PaddingProperty, new Thickness(0)),
            new Setter(TemplatedControl.TemplateProperty, new FuncControlTemplate<FlyoutPresenter>((presenter, _) =>
            {
                var contentPresenter = new ContentPresenter
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };
                contentPresenter.Bind(ContentPresenter.ContentProperty,
                    presenter.GetObservable(ContentControl.ContentProperty));
                contentPresenter.Bind(ContentPresenter.ContentTemplateProperty,
                    presenter.GetObservable(ContentControl.ContentTemplateProperty));

                return new Border
                {
                    Padding = new Thickness(0),
                    [!Border.BackgroundProperty] =
                        presenter.GetResourceObservable("LayerFillColorDefaultBrush").ToBinding(),
                    [!Border.BorderBrushProperty] =
                        presenter.GetResourceObservable("SurfaceStrokeColorFlyoutBrush").ToBinding(),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    Child = contentPresenter
                };
            }))
        }
    };

    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<TextBlock, bool>(
            "IsEnabled",
            typeof(TranslatableTextBlockBehavior));

    public static readonly AttachedProperty<string?> PretranslatedTextProperty =
        AvaloniaProperty.RegisterAttached<TextBlock, string?>(
            "PretranslatedText",
            typeof(TranslatableTextBlockBehavior));

    public static readonly AttachedProperty<TextTransformerType> TextTypeProperty =
        AvaloniaProperty.RegisterAttached<TextBlock, TextTransformerType>(
            "TextType",
            typeof(TranslatableTextBlockBehavior));

    public static readonly AttachedProperty<bool> CanTranslateProperty =
        AvaloniaProperty.RegisterAttached<TextBlock, bool>(
            "CanTranslate",
            typeof(TranslatableTextBlockBehavior));

    public static readonly AttachedProperty<bool> IsTranslatingProperty =
        AvaloniaProperty.RegisterAttached<TextBlock, bool>(
            "IsTranslating",
            typeof(TranslatableTextBlockBehavior));

    public static readonly AttachedProperty<bool> IsTranslatedProperty =
        AvaloniaProperty.RegisterAttached<TextBlock, bool>(
            "IsTranslated",
            typeof(TranslatableTextBlockBehavior));

    public static readonly AttachedProperty<string?> TranslatedTextProperty =
        AvaloniaProperty.RegisterAttached<TextBlock, string?>(
            "TranslatedText",
            typeof(TranslatableTextBlockBehavior));

    private static readonly AttachedProperty<TranslationState?> StateProperty =
        AvaloniaProperty.RegisterAttached<TextBlock, TranslationState?>(
            "State",
            typeof(TranslatableTextBlockBehavior));

    static TranslatableTextBlockBehavior()
    {
        IsEnabledProperty.Changed.AddClassHandler<TextBlock>(OnIsEnabledChanged);
        PretranslatedTextProperty.Changed.AddClassHandler<TextBlock>(OnPretranslatedTextChanged);
        TextTypeProperty.Changed.AddClassHandler<TextBlock>(OnTextTypeChanged);
        TextBlock.TextProperty.Changed.AddClassHandler<TextBlock>(OnTextChanged);
    }

    public static bool GetIsEnabled(TextBlock element) => element.GetValue(IsEnabledProperty);

    public static void SetIsEnabled(TextBlock element, bool value) => element.SetValue(IsEnabledProperty, value);

    public static string? GetPretranslatedText(TextBlock element) => element.GetValue(PretranslatedTextProperty);

    public static void SetPretranslatedText(TextBlock element, string? value) => element.SetValue(PretranslatedTextProperty, value);

    public static TextTransformerType GetTextType(TextBlock element) => element.GetValue(TextTypeProperty);

    public static void SetTextType(TextBlock element, TextTransformerType value) => element.SetValue(TextTypeProperty, value);

    public static bool GetCanTranslate(TextBlock element) => element.GetValue(CanTranslateProperty);

    public static bool GetIsTranslating(TextBlock element) => element.GetValue(IsTranslatingProperty);

    public static bool GetIsTranslated(TextBlock element) => element.GetValue(IsTranslatedProperty);

    public static string? GetTranslatedText(TextBlock element) => element.GetValue(TranslatedTextProperty);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetCanTranslate(TextBlock element, bool value) => element.SetValue(CanTranslateProperty, value);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetIsTranslating(TextBlock element, bool value) => element.SetValue(IsTranslatingProperty, value);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetIsTranslated(TextBlock element, bool value) => element.SetValue(IsTranslatedProperty, value);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetTranslatedText(TextBlock element, string? value) => element.SetValue(TranslatedTextProperty, value);

    private static TranslationState? GetState(TextBlock element) => element.GetValue(StateProperty);

    private static void SetState(TextBlock element, TranslationState? value) => element.SetValue(StateProperty, value);

    private static void OnIsEnabledChanged(TextBlock textBlock, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.GetNewValue<bool>())
            Attach(textBlock);
        else
            Detach(textBlock);
    }

    private static void Attach(TextBlock textBlock)
    {
        if (GetState(textBlock) is not null)
            return;

        var state = new TranslationState { OriginalText = textBlock.Text };
        SetState(textBlock, state);

        textBlock.Loaded += OnLoaded;
        textBlock.Unloaded += OnUnloaded;
        textBlock.PointerEntered += OnPointerEntered;

        RefreshTranslatedTextProperty(textBlock, state);
        UpdateCanTranslate(textBlock, state);

        if (!string.IsNullOrWhiteSpace(GetPretranslatedText(textBlock)))
            ShowTranslatedText(textBlock, state, GetPretranslatedText(textBlock)!);
    }

    private static void Detach(TextBlock textBlock)
    {
        if (GetState(textBlock) is not { } state)
            return;

        state.Flyout?.Hide();
        if (GetIsTranslated(textBlock))
            SetDisplayText(textBlock, state, state.OriginalText);

        textBlock.Loaded -= OnLoaded;
        textBlock.Unloaded -= OnUnloaded;
        textBlock.PointerEntered -= OnPointerEntered;

        SetCanTranslate(textBlock, false);
        SetIsTranslating(textBlock, false);
        SetIsTranslated(textBlock, false);
        SetTranslatedText(textBlock, null);
        SetState(textBlock, null);
    }

    private static void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is TextBlock textBlock && GetState(textBlock) is { } state)
        {
            state.OriginalText ??= textBlock.Text;
            RefreshTranslatedTextProperty(textBlock, state);
            UpdateCanTranslate(textBlock, state);
        }
    }

    private static void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        if (sender is TextBlock textBlock && GetState(textBlock) is { Flyout: { } flyout })
            flyout.Hide();
    }

    private static void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        if (sender is not TextBlock textBlock || GetState(textBlock) is not { } state)
            return;

        UpdateCanTranslate(textBlock, state);
        if (!GetCanTranslate(textBlock))
            return;

        EnsureFlyout(textBlock, state);
        UpdateFlyoutButton(textBlock, state);

        if (state.Flyout is { IsOpen: false } flyout && textBlock.IsLoaded)
            flyout.ShowAt(textBlock);
    }

    private static void OnTextChanged(TextBlock textBlock, AvaloniaPropertyChangedEventArgs e)
    {
        if (GetState(textBlock) is not { } state || state.IsUpdatingDisplayText)
            return;

        ++state.Version;
        state.OriginalText = e.GetNewValue<string?>();
        state.TranslatedText = null;
        SetIsTranslating(textBlock, false);
        RefreshTranslatedTextProperty(textBlock, state);
        UpdateCanTranslate(textBlock, state);

        if (GetPretranslatedText(textBlock) is { } pretranslatedText && !string.IsNullOrWhiteSpace(pretranslatedText))
            ShowTranslatedText(textBlock, state, pretranslatedText);
        else
            SetIsTranslated(textBlock, false);
    }

    private static void OnPretranslatedTextChanged(TextBlock textBlock, AvaloniaPropertyChangedEventArgs e)
    {
        if (GetState(textBlock) is not { } state)
            return;

        ++state.Version;
        state.TranslatedText = null;
        RefreshTranslatedTextProperty(textBlock, state);
        UpdateCanTranslate(textBlock, state);

        if (e.GetNewValue<string?>() is { } pretranslatedText && !string.IsNullOrWhiteSpace(pretranslatedText))
        {
            ShowTranslatedText(textBlock, state, pretranslatedText);
        }
        else if (GetIsTranslated(textBlock))
        {
            ShowOriginalText(textBlock, state);
        }
    }

    private static void OnTextTypeChanged(TextBlock textBlock, AvaloniaPropertyChangedEventArgs e)
    {
        if (GetState(textBlock) is not { } state)
            return;

        ++state.Version;
        state.TranslatedText = null;
        RefreshTranslatedTextProperty(textBlock, state);
        UpdateCanTranslate(textBlock, state);

        if (GetIsTranslated(textBlock) && string.IsNullOrWhiteSpace(GetPretranslatedText(textBlock)))
            ShowOriginalText(textBlock, state);
    }

    private static async Task ToggleTranslationAsync(TextBlock textBlock)
    {
        if (GetState(textBlock) is not { } state)
            return;

        UpdateCanTranslate(textBlock, state);
        if (!GetCanTranslate(textBlock) || GetIsTranslating(textBlock))
            return;

        if (GetIsTranslated(textBlock))
        {
            ShowOriginalText(textBlock, state);
            UpdateFlyoutButton(textBlock, state);
            return;
        }

        if (GetEffectiveTranslatedText(textBlock, state) is { } translatedText && !string.IsNullOrWhiteSpace(translatedText))
        {
            ShowTranslatedText(textBlock, state, translatedText);
            UpdateFlyoutButton(textBlock, state);
            return;
        }

        if (string.IsNullOrWhiteSpace(state.OriginalText) ||
            ExtensionService.ActiveTextTransformerCommands.FirstOrDefault() is not { } translator)
        {
            UpdateCanTranslate(textBlock, state);
            UpdateFlyoutButton(textBlock, state);
            return;
        }

        var version = state.Version;
        SetIsTranslating(textBlock, true);
        UpdateFlyoutButton(textBlock, state);

        try
        {
            var result = await translator.TransformAsync(state.OriginalText, GetTextType(textBlock));
            if (version != state.Version || string.IsNullOrWhiteSpace(result))
                return;

            state.TranslatedText = result;
            RefreshTranslatedTextProperty(textBlock, state);
            ShowTranslatedText(textBlock, state, result);
        }
        finally
        {
            if (version == state.Version)
            {
                SetIsTranslating(textBlock, false);
                UpdateCanTranslate(textBlock, state);
                UpdateFlyoutButton(textBlock, state);
            }
        }
    }

    private static void EnsureFlyout(TextBlock textBlock, TranslationState state)
    {
        if (state.Flyout is not null)
            return;

        var icon = new SymbolIcon
        {
            FontSize = 15,
            Symbol = Symbol.TranslateAuto
        };

        var progress = new ProgressRing
        {
            Width = 16,
            Height = 16,
            IsActive = true
        };

        var button = new Button
        {
            Padding = new Thickness(0),
            MinWidth = 0,
            MinHeight = 0,
            Width = 28,
            Height = 28,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Content = icon
        };
        button.SetCurrentValue(TemplatedControl.BackgroundProperty, Brushes.Transparent);
        button.SetCurrentValue(TemplatedControl.BorderBrushProperty, Brushes.Transparent);
        button.SetCurrentValue(TemplatedControl.BorderThicknessProperty, new Thickness(0));

        button.Click += async (_, e) =>
        {
            e.Handled = true;
            await ToggleTranslationAsync(textBlock);
        };

        state.Icon = icon;
        state.Progress = progress;
        state.Button = button;
        state.Flyout = new Flyout
        {
            Content = button,
            Placement = PlacementMode.TopEdgeAlignedRight,
            ShowMode = FlyoutShowMode.TransientWithDismissOnPointerMoveAway,
            OverlayDismissEventPassThrough = true,
            FlyoutPresenterTheme = CompactFlyoutPresenterTheme
        };
    }

    private static void UpdateFlyoutButton(TextBlock textBlock, TranslationState state)
    {
        if (state.Button is not { } button || state.Icon is not { } icon || state.Progress is not { } progress)
            return;

        var isTranslating = GetIsTranslating(textBlock);
        icon.Symbol = GetIsTranslated(textBlock) ? Symbol.TranslateOff : Symbol.TranslateAuto;
        button.IsEnabled = !isTranslating;
        progress.IsActive = isTranslating;
        progress.Foreground = button.Foreground;
        button.Content = isTranslating ? progress : icon;
    }

    private static void ShowOriginalText(TextBlock textBlock, TranslationState state)
    {
        SetDisplayText(textBlock, state, state.OriginalText);
        SetIsTranslated(textBlock, false);
        UpdateCanTranslate(textBlock, state);
    }

    private static void ShowTranslatedText(TextBlock textBlock, TranslationState state, string translatedText)
    {
        SetDisplayText(textBlock, state, translatedText);
        SetIsTranslated(textBlock, true);
        UpdateCanTranslate(textBlock, state);
    }

    private static void SetDisplayText(TextBlock textBlock, TranslationState state, string? text)
    {
        state.IsUpdatingDisplayText = true;
        try
        {
            textBlock.SetCurrentValue(TextBlock.TextProperty, text);
        }
        finally
        {
            state.IsUpdatingDisplayText = false;
        }
    }

    private static void RefreshTranslatedTextProperty(TextBlock textBlock, TranslationState state)
    {
        SetTranslatedText(textBlock, GetEffectiveTranslatedText(textBlock, state));
    }

    private static string? GetEffectiveTranslatedText(TextBlock textBlock, TranslationState state) =>
        !string.IsNullOrWhiteSpace(GetPretranslatedText(textBlock))
            ? GetPretranslatedText(textBlock)
            : state.TranslatedText;

    private static void UpdateCanTranslate(TextBlock textBlock, TranslationState state)
    {
        var canTranslate =
            !string.IsNullOrWhiteSpace(GetEffectiveTranslatedText(textBlock, state)) ||
            (!string.IsNullOrWhiteSpace(state.OriginalText) &&
             ExtensionService.ActiveTextTransformerCommands.Any());

        SetCanTranslate(textBlock, canTranslate);
    }

    private static ExtensionService ExtensionService => App.AppViewModel.AppServiceProvider.GetRequiredService<ExtensionService>();

    private sealed class TranslationState
    {
        public string? OriginalText { get; set; }

        public string? TranslatedText { get; set; }

        public int Version { get; set; }

        public bool IsUpdatingDisplayText { get; set; }

        public Flyout? Flyout { get; set; }

        public Button? Button { get; set; }

        public SymbolIcon? Icon { get; set; }

        public ProgressRing? Progress { get; set; }
    }
}
