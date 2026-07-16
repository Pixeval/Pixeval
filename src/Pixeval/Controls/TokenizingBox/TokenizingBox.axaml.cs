// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Pixeval.Controls;

[TemplatePart(PartRoot, typeof(Border))]
[TemplatePart(PartItemsHost, typeof(TokenizingBoxPanel))]
[TemplatePart(PartAutoCompleteBox, typeof(AutoCompleteBox))]
[TemplatePart(PartCountTextBlock, typeof(TextBlock))]
[PseudoClasses(PseudoClassEmpty, PseudoClassMaxReached)]
public class TokenizingBox : TemplatedControl
{
    private const string PartRoot = "PART_Root";
    private const string PartItemsHost = "PART_ItemsHost";
    private const string PartAutoCompleteBox = "PART_AutoCompleteBox";
    private const string PartCountTextBlock = "PART_CountTextBlock";
    private const string PartSuggestionItemsControl = "PART_SelectingItemsControl";

    private const string TokenRemoveClass = "tokenizing-token-remove";
    private const string PseudoClassEmpty = ":empty";
    private const string PseudoClassMaxReached = ":maxreached";

    public static readonly StyledProperty<int> MaxCountProperty =
        AvaloniaProperty.Register<TokenizingBox, int>(nameof(MaxCount), -1, validate: value => value >= -1);

    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        ItemsControl.ItemsSourceProperty.AddOwner<TokenizingBox>();

    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        ItemsControl.ItemTemplateProperty.AddOwner<TokenizingBox>();

    public static readonly StyledProperty<IEnumerable?> SuggestionItemsSourceProperty =
        AvaloniaProperty.Register<TokenizingBox, IEnumerable?>(nameof(SuggestionItemsSource));

    public static readonly StyledProperty<IDataTemplate?> SuggestionItemTemplateProperty =
        AvaloniaProperty.Register<TokenizingBox, IDataTemplate?>(nameof(SuggestionItemTemplate));

    public static readonly StyledProperty<string?> TokenSeparatorProperty =
        AvaloniaProperty.Register<TokenizingBox, string?>(nameof(TokenSeparator), ",");

    public static readonly StyledProperty<bool> AllowDuplicateTokensProperty =
        AvaloniaProperty.Register<TokenizingBox, bool>(nameof(AllowDuplicateTokens), true);

    public static readonly StyledProperty<string?> TextProperty =
        AutoCompleteBox.TextProperty.AddOwner<TokenizingBox>(new(defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true));

    public static readonly StyledProperty<string?> PlaceholderTextProperty =
        AutoCompleteBox.PlaceholderTextProperty.AddOwner<TokenizingBox>();

    public static readonly StyledProperty<IBrush?> PlaceholderForegroundProperty =
        AutoCompleteBox.PlaceholderForegroundProperty.AddOwner<TokenizingBox>();

    public static readonly StyledProperty<object?> InnerLeftContentProperty =
        AutoCompleteBox.InnerLeftContentProperty.AddOwner<TokenizingBox>();

    public static readonly StyledProperty<object?> InnerRightContentProperty =
        AutoCompleteBox.InnerRightContentProperty.AddOwner<TokenizingBox>();

    public static readonly StyledProperty<int> MinimumPrefixLengthProperty =
        AutoCompleteBox.MinimumPrefixLengthProperty.AddOwner<TokenizingBox>();

    public static readonly StyledProperty<TimeSpan> MinimumPopulateDelayProperty =
        AutoCompleteBox.MinimumPopulateDelayProperty.AddOwner<TokenizingBox>();

    public static readonly StyledProperty<double> MaxDropDownHeightProperty =
        AutoCompleteBox.MaxDropDownHeightProperty.AddOwner<TokenizingBox>();

    public static readonly StyledProperty<bool> IsTextCompletionEnabledProperty =
        AutoCompleteBox.IsTextCompletionEnabledProperty.AddOwner<TokenizingBox>();

    public static readonly StyledProperty<AutoCompleteFilterMode> FilterModeProperty =
        AutoCompleteBox.FilterModeProperty.AddOwner<TokenizingBox>();

    public static readonly StyledProperty<AutoCompleteFilterPredicate<object?>?> ItemFilterProperty =
        AutoCompleteBox.ItemFilterProperty.AddOwner<TokenizingBox>();

    public static readonly StyledProperty<AutoCompleteFilterPredicate<string?>?> TextFilterProperty =
        AutoCompleteBox.TextFilterProperty.AddOwner<TokenizingBox>();

    public static readonly StyledProperty<AutoCompleteSelector<object>?> ItemSelectorProperty =
        AutoCompleteBox.ItemSelectorProperty.AddOwner<TokenizingBox>();

    public static readonly StyledProperty<AutoCompleteSelector<string?>?> TextSelectorProperty =
        AutoCompleteBox.TextSelectorProperty.AddOwner<TokenizingBox>();

    public static readonly StyledProperty<Func<string?, CancellationToken, Task<IEnumerable<object>>>?> AsyncPopulatorProperty =
        AutoCompleteBox.AsyncPopulatorProperty.AddOwner<TokenizingBox>();

    public static readonly StyledProperty<BindingBase?> ValueMemberBindingProperty =
        AutoCompleteBox.ValueMemberBindingProperty.AddOwner<TokenizingBox>();

    public static readonly DirectProperty<TokenizingBox, int> TokenCountProperty =
        AvaloniaProperty.RegisterDirect<TokenizingBox, int>(nameof(TokenCount), box => box.TokenCount);

    public static readonly DirectProperty<TokenizingBox, IReadOnlyList<TokenizingBoxToken>> TokenItemsProperty =
        AvaloniaProperty.RegisterDirect<TokenizingBox, IReadOnlyList<TokenizingBoxToken>>(nameof(TokenItems), box => box.TokenItems);

    public static readonly FuncValueConverter<int, bool> HasMaxCountConverter = new(maxCount => maxCount >= 0);

    private readonly List<object?> _internalItems = [];
    private Border? _root;
    private TokenizingBoxPanel? _itemsHost;
    private AutoCompleteBox? _autoCompleteBox;
    private TextBox? _textBox;
    private SelectingItemsControl? _suggestionItemsControl;
    private INotifyCollectionChanged? _notifyCollectionChanged;

    static TokenizingBox()
    {
        FocusableProperty.OverrideDefaultValue<TokenizingBox>(true);

        ItemsSourceProperty.Changed.AddClassHandler<TokenizingBox>((box, _) => box.OnItemsSourceChanged());
        MaxCountProperty.Changed.AddClassHandler<TokenizingBox>((box, _) => box.OnMaxCountChanged());
    }

    public int MaxCount
    {
        get => GetValue(MaxCountProperty);
        set => SetValue(MaxCountProperty, value);
    }

    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public IEnumerable? SuggestionItemsSource
    {
        get => GetValue(SuggestionItemsSourceProperty);
        set => SetValue(SuggestionItemsSourceProperty, value);
    }

    [InheritDataTypeFromItems(nameof(SuggestionItemsSource))]
    public IDataTemplate? SuggestionItemTemplate
    {
        get => GetValue(SuggestionItemTemplateProperty);
        set => SetValue(SuggestionItemTemplateProperty, value);
    }

    public string? TokenSeparator
    {
        get => GetValue(TokenSeparatorProperty);
        set => SetValue(TokenSeparatorProperty, value);
    }

    public bool AllowDuplicateTokens
    {
        get => GetValue(AllowDuplicateTokensProperty);
        set => SetValue(AllowDuplicateTokensProperty, value);
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string? PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    public IBrush? PlaceholderForeground
    {
        get => GetValue(PlaceholderForegroundProperty);
        set => SetValue(PlaceholderForegroundProperty, value);
    }

    public object? InnerLeftContent
    {
        get => GetValue(InnerLeftContentProperty);
        set => SetValue(InnerLeftContentProperty, value);
    }

    public object? InnerRightContent
    {
        get => GetValue(InnerRightContentProperty);
        set => SetValue(InnerRightContentProperty, value);
    }

    public int MinimumPrefixLength
    {
        get => GetValue(MinimumPrefixLengthProperty);
        set => SetValue(MinimumPrefixLengthProperty, value);
    }

    public TimeSpan MinimumPopulateDelay
    {
        get => GetValue(MinimumPopulateDelayProperty);
        set => SetValue(MinimumPopulateDelayProperty, value);
    }

    public double MaxDropDownHeight
    {
        get => GetValue(MaxDropDownHeightProperty);
        set => SetValue(MaxDropDownHeightProperty, value);
    }

    public bool IsTextCompletionEnabled
    {
        get => GetValue(IsTextCompletionEnabledProperty);
        set => SetValue(IsTextCompletionEnabledProperty, value);
    }

    public AutoCompleteFilterMode FilterMode
    {
        get => GetValue(FilterModeProperty);
        set => SetValue(FilterModeProperty, value);
    }

    public AutoCompleteFilterPredicate<object?>? ItemFilter
    {
        get => GetValue(ItemFilterProperty);
        set => SetValue(ItemFilterProperty, value);
    }

    public AutoCompleteFilterPredicate<string?>? TextFilter
    {
        get => GetValue(TextFilterProperty);
        set => SetValue(TextFilterProperty, value);
    }

    public AutoCompleteSelector<object>? ItemSelector
    {
        get => GetValue(ItemSelectorProperty);
        set => SetValue(ItemSelectorProperty, value);
    }

    public AutoCompleteSelector<string?>? TextSelector
    {
        get => GetValue(TextSelectorProperty);
        set => SetValue(TextSelectorProperty, value);
    }

    public Func<string?, CancellationToken, Task<IEnumerable<object>>>? AsyncPopulator
    {
        get => GetValue(AsyncPopulatorProperty);
        set => SetValue(AsyncPopulatorProperty, value);
    }

    [AssignBinding]
    [InheritDataTypeFromItems(nameof(SuggestionItemsSource))]
    public BindingBase? ValueMemberBinding
    {
        get => GetValue(ValueMemberBindingProperty);
        set => SetValue(ValueMemberBindingProperty, value);
    }

    public int TokenCount
    {
        get;
        private set => SetAndRaise(TokenCountProperty, ref field, value);
    }

    public IReadOnlyList<TokenizingBoxToken> TokenItems
    {
        get;
        private set => SetAndRaise(TokenItemsProperty, ref field, value);
    } = [];

    public event EventHandler<TokenizingBox, TokenAddingEventArgs>? TokenAdding;

    public event EventHandler<TokenizingBox, TokenEventArgs>? TokenAdded;

    public event EventHandler<TokenizingBox, TokenRemovingEventArgs>? TokenRemoving;

    public event EventHandler<TokenizingBox, TokenEventArgs>? TokenRemoved;

    public event EventHandler<TokenizingBox, TokenEventArgs>? TokenClick;

    public bool AddToken(object? item) => AddTokenCore(item, item as string);

    public bool RemoveToken(object? item)
    {
        var snapshot = GetTokenItemsSnapshot();
        var index = snapshot.FindIndex(token => Equals(token, item));
        return index >= 0 && RemoveTokenAt(index, snapshot[index]);
    }

    public void ClearTokens()
    {
        var snapshot = GetTokenItemsSnapshot();
        for (var i = snapshot.Count - 1; i >= 0; --i)
            RemoveTokenAt(i, snapshot[i]);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        DetachTemplateParts();

        base.OnApplyTemplate(e);

        _root = e.NameScope.Find<Border>(PartRoot);
        _itemsHost = e.NameScope.Find<TokenizingBoxPanel>(PartItemsHost);
        _autoCompleteBox = e.NameScope.Find<AutoCompleteBox>(PartAutoCompleteBox);

        _root?.Tapped += RootOnTapped;
        _itemsHost?.Tapped += ItemsHostOnTapped;

        if (_autoCompleteBox is not null)
        {
            _autoCompleteBox.TextChanged += AutoCompleteBoxOnTextChanged;
            _autoCompleteBox.TemplateApplied += AutoCompleteBoxOnTemplateApplied;
            _autoCompleteBox.AddHandler(KeyDownEvent, AutoCompleteBoxOnKeyDown, RoutingStrategies.Tunnel);
        }

        RebuildItemsHost();
    }

    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);

        if (ReferenceEquals(e.Source, this))
            Dispatcher.UIThread.Post(FocusTextBox);
    }

    private void DetachTemplateParts()
    {
        _root?.Tapped -= RootOnTapped;
        _itemsHost?.Tapped -= ItemsHostOnTapped;

        if (_autoCompleteBox is not null)
        {
            _autoCompleteBox.TextChanged -= AutoCompleteBoxOnTextChanged;
            _autoCompleteBox.TemplateApplied -= AutoCompleteBoxOnTemplateApplied;
            _autoCompleteBox.RemoveHandler(KeyDownEvent, AutoCompleteBoxOnKeyDown);
        }

        DetachSuggestionItemsControl();

        _root = null;
        _itemsHost = null;
        _autoCompleteBox = null;
        _textBox = null;
    }

    private void OnItemsSourceChanged()
    {
        _notifyCollectionChanged?.CollectionChanged -= ItemsSourceOnCollectionChanged;

        _notifyCollectionChanged = ItemsSource as INotifyCollectionChanged;

        _notifyCollectionChanged?.CollectionChanged += ItemsSourceOnCollectionChanged;

        TrimToMaxCount();
        RebuildItemsHost();
    }

    private void OnMaxCountChanged()
    {
        TrimToMaxCount();
        RebuildItemsHost();
    }

    private void ItemsSourceOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        TrimToMaxCount();
        RebuildItemsHost();
    }

    private void RebuildItemsHost()
    {
        var snapshot = GetTokenItemsSnapshot();
        var shouldRestoreFocus = _itemsHost is not null && IsKeyboardFocusWithin;
        var tokenItems = new TokenizingBoxToken[snapshot.Count];

        for (var i = 0; i < snapshot.Count; ++i)
        {
            var index = i;
            var item = snapshot[index];
            tokenItems[index] = new TokenizingBoxToken(item, new TokenCommand(() => RemoveTokenAt(index, item)));
        }

        TokenItems = tokenItems;

        UpdateVisualState(snapshot);

        if (shouldRestoreFocus)
            QueueFocusTextBox();
    }

    private void ItemsHostOnTapped(object? sender, TappedEventArgs e)
    {
        if (!IsRemoveButtonEvent(e) && GetEventToken(e) is { } token)
            TokenClick?.Invoke(this, new TokenEventArgs(token.Item));
    }

    private static bool IsRemoveButtonEvent(RoutedEventArgs e)
    {
        return e.Source is Visual visual &&
               visual.GetSelfAndVisualAncestors()
                   .OfType<Control>()
                   .Any(control => control.Classes.Contains(TokenRemoveClass));
    }

    private static TokenizingBoxToken? GetEventToken(RoutedEventArgs e)
    {
        return e.Source is Visual visual
            ? visual.GetSelfAndVisualAncestors()
                .OfType<Control>()
                .Select(control => control.DataContext)
                .OfType<TokenizingBoxToken>()
                .FirstOrDefault()
            : null;
    }

    private void RootOnTapped(object? sender, TappedEventArgs e)
    {
        if (!IsRemoveButtonEvent(e) && GetEventToken(e) is null)
            FocusTextBox();
    }

    private void AutoCompleteBoxOnTemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
        DetachSuggestionItemsControl();

        _textBox = e.NameScope.Find<TextBox>("PART_TextBox")
                   ?? _autoCompleteBox?.GetVisualDescendants().OfType<TextBox>().FirstOrDefault();
        _suggestionItemsControl = e.NameScope.Find<SelectingItemsControl>(PartSuggestionItemsControl);
        _suggestionItemsControl?.PointerReleased += SuggestionItemsControlOnPointerReleased;
    }

    private void DetachSuggestionItemsControl()
    {
        _suggestionItemsControl?.PointerReleased -= SuggestionItemsControlOnPointerReleased;
        _suggestionItemsControl = null;
    }

    private void SuggestionItemsControlOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_autoCompleteBox is not null)
                CommitSuggestion(_autoCompleteBox.SelectedItem ?? _suggestionItemsControl?.SelectedItem);
        });
    }

    private void AutoCompleteBoxOnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Handled)
            return;

        switch (e.Key)
        {
            case Key.Enter:
                e.Handled = CommitSuggestion(_autoCompleteBox?.SelectedItem) || CommitText();
                break;

            case Key.Back:
                if (string.IsNullOrEmpty(_autoCompleteBox?.Text))
                {
                    var snapshot = GetTokenItemsSnapshot();
                    if (snapshot.Count > 0)
                    {
                        e.Handled = RemoveTokenAt(snapshot.Count - 1, snapshot[^1]);
                    }
                }

                break;
        }
    }

    private void AutoCompleteBoxOnTextChanged(object? sender, TextChangedEventArgs e)
    {
        ProcessTokenSeparators(_autoCompleteBox?.Text ?? "");
    }

    private bool CommitSuggestion(object? item)
    {
        if (item is null)
            return false;

        if (!AddTokenCore(item, item as string))
            return false;

        SetCurrentValue(TextProperty, "");
        if (_autoCompleteBox is not null)
        {
            _autoCompleteBox.Text = "";
            _autoCompleteBox.SelectedItem = null;
            _autoCompleteBox.IsDropDownOpen = false;
            QueueFocusTextBox();
        }

        return true;
    }

    private bool CommitText()
    {
        var text = (_autoCompleteBox?.Text ?? Text)?.Trim();
        if (string.IsNullOrEmpty(text))
            return false;

        if (!AddTokenCore(text, text))
            return false;

        SetCurrentValue(TextProperty, "");
        if (_autoCompleteBox is not null)
        {
            _autoCompleteBox.Text = "";
            QueueFocusTextBox();
        }

        return true;
    }

    private void ProcessTokenSeparators(string text)
    {
        var separator = TokenSeparator;
        if (string.IsNullOrEmpty(separator) || string.IsNullOrEmpty(text) || !text.Contains(separator, StringComparison.Ordinal))
            return;

        var parts = text.Split([separator], StringSplitOptions.None);
        var endsWithSeparator = text.EndsWith(separator, StringComparison.Ordinal);
        var tokenCount = endsWithSeparator ? parts.Length : parts.Length - 1;
        var remainder = endsWithSeparator ? "" : parts[^1].Trim();
        var processedToken = false;

        for (var i = 0; i < tokenCount; ++i)
        {
            var token = parts[i].Trim();
            if (token.Length == 0)
                continue;

            if (!AddTokenCore(token, token))
            {
                remainder = string.Join(separator, parts, i, parts.Length - i);
                break;
            }

            processedToken = true;
        }

        SetCurrentValue(TextProperty, remainder);
        _autoCompleteBox?.Text = remainder;

        if (processedToken)
            QueueFocusTextBox();
    }

    private bool AddTokenCore(object? item, string? tokenText)
    {
        if (IsMaxCountReached())
            return false;

        var args = new TokenAddingEventArgs(item, tokenText);
        TokenAdding?.Invoke(this, args);

        if (args.Cancel)
            return false;

        item = args.Item;

        if (item is null)
            return true;

        if (!AllowDuplicateTokens && ContainsToken(item))
            return true;

        if (!AddItemToSource(item))
            return false;

        TokenAdded?.Invoke(this, new TokenEventArgs(item));

        if (_notifyCollectionChanged is null)
            RebuildItemsHost();

        return true;
    }

    private bool RemoveTokenAt(int index, object? item, bool ignoreCancel = false)
    {
        if (!ignoreCancel)
        {
            var args = new TokenRemovingEventArgs(item);
            TokenRemoving?.Invoke(this, args);

            if (args.Cancel)
                return false;
        }

        if (!RemoveItemFromSource(index, item))
            return false;

        TokenRemoved?.Invoke(this, new TokenEventArgs(item));

        if (_notifyCollectionChanged is null)
            RebuildItemsHost();

        return true;
    }

    private void TrimToMaxCount()
    {
        if (MaxCount < 0)
            return;

        var snapshot = GetTokenItemsSnapshot();
        for (var i = snapshot.Count - 1; i >= MaxCount; --i)
            RemoveTokenAt(i, snapshot[i], true);
    }

    private bool AddItemToSource(object? item)
    {
        switch (ItemsSource)
        {
            case null:
                _internalItems.Add(item);
                return true;
            case IList list when list.IsReadOnly || list.IsFixedSize:
                return false;
            case IList list:
                try
                {
                    list.Add(item);
                    return true;
                }
                catch (ArgumentException)
                {
                    return false;
                }
                catch (InvalidCastException)
                {
                    return false;
                }
            default:
                return false;
        }
    }

    private bool RemoveItemFromSource(int index, object? item)
    {
        switch (ItemsSource)
        {
            case null when index < 0 || index >= _internalItems.Count:
                return false;
            case null:
                _internalItems.RemoveAt(index);
                return true;
            case IList list when list.IsReadOnly || list.IsFixedSize:
                return false;
            case IList list when index >= 0 && index < list.Count && Equals(list[index], item):
                list.RemoveAt(index);
                return true;
            case IList list when list.Contains(item):
                list.Remove(item);
                return true;
            default:
                return false;
        }
    }

    private bool ContainsToken(object? item)
    {
        return GetTokenItemsSnapshot().Any(token => Equals(token, item));
    }

    private bool IsMaxCountReached()
    {
        return MaxCount >= 0 && TokenCount >= MaxCount;
    }

    private void QueueFocusTextBox()
    {
        Dispatcher.UIThread.Post(FocusTextBox, DispatcherPriority.Input);
    }

    private void FocusTextBox()
    {
        var textBox = EnsureTextBox();
        if (textBox?.Focus() == true)
        {
            textBox.CaretIndex = textBox.Text?.Length ?? 0;
            return;
        }

        _autoCompleteBox?.Focus();
    }

    private TextBox? EnsureTextBox()
    {
        if (_textBox is not null)
            return _textBox;

        if (_autoCompleteBox is null)
            return null;

        _autoCompleteBox.ApplyTemplate();
        _textBox = _autoCompleteBox.GetVisualDescendants().OfType<TextBox>().FirstOrDefault();
        return _textBox;
    }

    private List<object?> GetTokenItemsSnapshot()
    {
        return [.. (ItemsSource ?? _internalItems).Cast<object?>()];
    }

    private void UpdateVisualState(IReadOnlyCollection<object?> snapshot)
    {
        TokenCount = snapshot.Count;

        var isEmpty = snapshot.Count is 0;
        var isMaxReached = IsMaxCountReached();

        PseudoClasses.Set(PseudoClassEmpty, isEmpty);
        PseudoClasses.Set(PseudoClassMaxReached, isMaxReached);
    }
}

file sealed class TokenCommand(Action execute) : ICommand
{
    public event EventHandler? CanExecuteChanged
    {
        add { }
        remove { }
    }

    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter) => execute();
}
