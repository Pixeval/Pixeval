// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
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
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Pixeval.Controls;

public class TokenAddingEventArgs(object? item, string? tokenText = null) : CancelEventArgs
{
    public string? TokenText { get; } = tokenText;

    public object? Item { get; set; } = item;
}

public class TokenEventArgs(object? item) : EventArgs
{
    public object? Item { get; } = item;
}

public class TokenRemovingEventArgs(object? item) : CancelEventArgs
{
    public object? Item { get; } = item;
}

public sealed class TokenizingBoxToken(object? item, int index, IDataTemplate? itemTemplate, ICommand removeCommand)
{
    public object? Item { get; } = item;

    public int Index { get; } = index;

    public IDataTemplate? ItemTemplate { get; } = itemTemplate;

    public ICommand RemoveCommand { get; } = removeCommand;
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

public class TokenizingBoxPanel : Panel
{
    public static readonly StyledProperty<double> ItemSpacingProperty =
        AvaloniaProperty.Register<TokenizingBoxPanel, double>(nameof(ItemSpacing), 4);

    public static readonly StyledProperty<double> LineSpacingProperty =
        AvaloniaProperty.Register<TokenizingBoxPanel, double>(nameof(LineSpacing), 2);

    public static readonly StyledProperty<double> MinInputWidthProperty =
        AvaloniaProperty.Register<TokenizingBoxPanel, double>(nameof(MinInputWidth), 120);

    static TokenizingBoxPanel()
    {
        AffectsMeasure<TokenizingBoxPanel>(ItemSpacingProperty, LineSpacingProperty, MinInputWidthProperty);
        AffectsArrange<TokenizingBoxPanel>(ItemSpacingProperty, LineSpacingProperty, MinInputWidthProperty);
    }

    public double ItemSpacing
    {
        get => GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }

    public double LineSpacing
    {
        get => GetValue(LineSpacingProperty);
        set => SetValue(LineSpacingProperty, value);
    }

    public double MinInputWidth
    {
        get => GetValue(MinInputWidthProperty);
        set => SetValue(MinInputWidthProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var width = double.IsInfinity(availableSize.Width) ? double.PositiveInfinity : Math.Max(0, availableSize.Width);
        var input = FindNamedChild("PART_AutoCompleteBox");
        var count = FindNamedChild("PART_CountTextBlock");

        foreach (var child in Children)
        {
            if (ReferenceEquals(child, input))
            {
                child.Measure(new Size(double.IsInfinity(width) ? MinInputWidth : width, availableSize.Height));
            }
            else
            {
                child.Measure(new Size(double.PositiveInfinity, availableSize.Height));
            }
        }

        var desiredWidth = double.IsInfinity(width) ? 0 : width;
        var x = 0d;
        var lineHeight = 0d;
        var totalHeight = 0d;
        var maxLineWidth = 0d;

        foreach (var child in Children)
        {
            if (!child.IsVisible || ReferenceEquals(child, input) || ReferenceEquals(child, count))
                continue;

            AddMeasuredChild(child.DesiredSize.Width, child.DesiredSize.Height);
        }

        if (input?.IsVisible == true)
        {
            var countWidth = count?.IsVisible == true ? count.DesiredSize.Width : 0;
            var countSpacing = countWidth > 0 ? ItemSpacing : 0;
            var neededWidth = MinInputWidth + countSpacing + countWidth;

            if (!double.IsInfinity(width) && x > 0 && x + ItemSpacing + neededWidth > width)
                CommitLine();

            var prefixSpacing = x > 0 ? ItemSpacing : 0;
            var inputWidth = double.IsInfinity(width)
                ? MinInputWidth
                : Math.Max(MinInputWidth, width - x - prefixSpacing - countSpacing - countWidth);

            AddMeasuredChild(inputWidth, input.DesiredSize.Height);

            if (count?.IsVisible == true)
                AddMeasuredChild(countWidth, count.DesiredSize.Height);
        }
        else if (count?.IsVisible == true)
        {
            AddMeasuredChild(count.DesiredSize.Width, count.DesiredSize.Height);
        }

        CommitLine();

        return new Size(double.IsInfinity(width) ? maxLineWidth : desiredWidth, totalHeight);

        void AddMeasuredChild(double childWidth, double childHeight)
        {
            if (!double.IsInfinity(width) && x > 0 && x + ItemSpacing + childWidth > width)
                CommitLine();

            x += x > 0 ? ItemSpacing + childWidth : childWidth;
            lineHeight = Math.Max(lineHeight, childHeight);
        }

        void CommitLine()
        {
            if (lineHeight <= 0)
                return;

            maxLineWidth = Math.Max(maxLineWidth, x);
            totalHeight += totalHeight > 0 ? LineSpacing + lineHeight : lineHeight;
            x = 0;
            lineHeight = 0;
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var input = FindNamedChild("PART_AutoCompleteBox");
        var count = FindNamedChild("PART_CountTextBlock");
        var width = Math.Max(0, finalSize.Width);
        var x = 0d;
        var y = 0d;
        var lineHeight = 0d;

        foreach (var child in Children)
        {
            if (!child.IsVisible || ReferenceEquals(child, input) || ReferenceEquals(child, count))
                continue;

            ArrangeFixedChild(child);
        }

        if (input?.IsVisible == true)
        {
            var countWidth = count?.IsVisible == true ? count.DesiredSize.Width : 0;
            var countSpacing = countWidth > 0 ? ItemSpacing : 0;
            var neededWidth = MinInputWidth + countSpacing + countWidth;

            if (x > 0 && x + ItemSpacing + neededWidth > width)
                MoveToNextLine();

            var inputX = x > 0 ? x + ItemSpacing : 0;
            var inputWidth = Math.Max(0, width - inputX - countSpacing - countWidth);
            var rowHeight = Math.Max(input.DesiredSize.Height, count?.IsVisible == true ? count.DesiredSize.Height : 0);

            input.Arrange(new Rect(inputX, y, inputWidth, rowHeight));
            x = inputX + inputWidth;
            lineHeight = Math.Max(lineHeight, rowHeight);

            if (count?.IsVisible == true)
            {
                var countX = x + countSpacing;
                count.Arrange(new Rect(countX, y, countWidth, rowHeight));
                x = countX + countWidth;
            }
        }
        else if (count?.IsVisible == true)
        {
            ArrangeFixedChild(count);
        }

        return finalSize;

        void ArrangeFixedChild(Control child)
        {
            var childSize = child.DesiredSize;
            if (x > 0 && x + ItemSpacing + childSize.Width > width)
                MoveToNextLine();

            var childX = x > 0 ? x + ItemSpacing : 0;
            child.Arrange(new Rect(childX, y, childSize.Width, childSize.Height));
            x = childX + childSize.Width;
            lineHeight = Math.Max(lineHeight, childSize.Height);
        }

        void MoveToNextLine()
        {
            y += lineHeight + LineSpacing;
            x = 0;
            lineHeight = 0;
        }
    }

    private Control? FindNamedChild(string name)
    {
        return Children.FirstOrDefault(child => child.Name == name);
    }
}

[TemplatePart(PartRoot, typeof(Border))]
[TemplatePart(PartItemsHost, typeof(Panel))]
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
    public const string TokenTemplateResourceKey = "TokenizingBoxTokenTemplate";

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

    private readonly List<object?> _internalItems = [];
    private Border? _root;
    private Panel? _itemsHost;
    private AutoCompleteBox? _autoCompleteBox;
    private TextBox? _textBox;
    private TextBlock? _countTextBlock;
    private SelectingItemsControl? _suggestionItemsControl;
    private INotifyCollectionChanged? _notifyCollectionChanged;

    static TokenizingBox()
    {
        FocusableProperty.OverrideDefaultValue<TokenizingBox>(true);

        ItemsSourceProperty.Changed.AddClassHandler<TokenizingBox>((box, _) => box.OnItemsSourceChanged());
        MaxCountProperty.Changed.AddClassHandler<TokenizingBox>((box, _) => box.OnMaxCountChanged());
        ItemTemplateProperty.Changed.AddClassHandler<TokenizingBox>((box, _) => box.RebuildItemsHost());
        PlaceholderTextProperty.Changed.AddClassHandler<TokenizingBox>((box, _) => box.UpdateVisualState());
        IsEnabledProperty.Changed.AddClassHandler<TokenizingBox>((box, _) => box.UpdateVisualState());
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
        _itemsHost = e.NameScope.Find<Panel>(PartItemsHost);
        _autoCompleteBox = e.NameScope.Find<AutoCompleteBox>(PartAutoCompleteBox);
        _countTextBlock = e.NameScope.Find<TextBlock>(PartCountTextBlock);

        if (_root is not null)
            _root.Tapped += RootOnTapped;

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
        if (_root is not null)
            _root.Tapped -= RootOnTapped;

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
        _countTextBlock = null;
    }

    private void OnItemsSourceChanged()
    {
        if (_notifyCollectionChanged is not null)
            _notifyCollectionChanged.CollectionChanged -= ItemsSourceOnCollectionChanged;

        _notifyCollectionChanged = ItemsSource as INotifyCollectionChanged;

        if (_notifyCollectionChanged is not null)
            _notifyCollectionChanged.CollectionChanged += ItemsSourceOnCollectionChanged;

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
        if (_itemsHost is null)
            return;

        var snapshot = GetTokenItemsSnapshot();
        var shouldRestoreFocus = IsKeyboardFocusWithin;

        _itemsHost.Children.Clear();

        for (var i = 0; i < snapshot.Count; ++i)
        {
            _itemsHost.Children.Add(CreateTokenControl(snapshot[i], i));
        }

        if (_autoCompleteBox is not null)
            _itemsHost.Children.Add(_autoCompleteBox);

        if (_countTextBlock is not null)
            _itemsHost.Children.Add(_countTextBlock);

        UpdateVisualState(snapshot);

        if (shouldRestoreFocus)
            QueueFocusTextBox();
    }

    private Control CreateTokenControl(object? item, int index)
    {
        var token = new TokenizingBoxToken(
            item,
            index,
            ItemTemplate,
            new TokenCommand(() => RemoveTokenAt(index, item)));

        var control = new ContentControl
        {
            Content = token,
            VerticalAlignment = VerticalAlignment.Center,
            [!ContentControl.ContentTemplateProperty] = new DynamicResourceExtension(TokenTemplateResourceKey)
        };

        control.Tapped += (_, e) =>
        {
            if (!IsRemoveButtonEvent(e, token))
                TokenClick?.Invoke(this, new TokenEventArgs(item));
        };

        return control;
    }

    private static bool IsRemoveButtonEvent(RoutedEventArgs e, TokenizingBoxToken? token = null)
    {
        return e.Source is Visual visual &&
               visual.GetSelfAndVisualAncestors()
                   .OfType<Control>()
                   .Any(control =>
                       control.Classes.Contains(TokenRemoveClass)
                       || (control is Button { Command: { } command }
                           && ReferenceEquals(command, token?.RemoveCommand)));
    }

    private void RootOnTapped(object? sender, TappedEventArgs e)
    {
        if (!IsRemoveButtonEvent(e))
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
        ProcessTokenSeparators(_autoCompleteBox?.Text ?? string.Empty);
    }

    private bool CommitSuggestion(object? item)
    {
        if (item is null)
            return false;

        if (!AddTokenCore(item, item as string))
            return false;

        SetCurrentValue(TextProperty, string.Empty);
        if (_autoCompleteBox is not null)
        {
            _autoCompleteBox.Text = string.Empty;
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

        SetCurrentValue(TextProperty, string.Empty);
        if (_autoCompleteBox is not null)
        {
            _autoCompleteBox.Text = string.Empty;
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
        var remainder = endsWithSeparator ? string.Empty : parts[^1].Trim();
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
        return (ItemsSource ?? _internalItems).Cast<object?>().ToList();
    }

    private void UpdateVisualState()
    {
        UpdateVisualState(GetTokenItemsSnapshot());
    }

    private void UpdateVisualState(IReadOnlyCollection<object?> snapshot)
    {
        TokenCount = snapshot.Count;

        var isEmpty = snapshot.Count is 0;
        var isMaxReached = IsMaxCountReached();

        PseudoClasses.Set(PseudoClassEmpty, isEmpty);
        PseudoClasses.Set(PseudoClassMaxReached, isMaxReached);

        if (_autoCompleteBox is not null)
        {
            _autoCompleteBox.IsVisible = !isMaxReached;
            _autoCompleteBox.IsEnabled = IsEnabled && !isMaxReached;
            _autoCompleteBox.PlaceholderText = PlaceholderText;
        }

        if (_countTextBlock is not null)
        {
            _countTextBlock.IsVisible = MaxCount >= 0;
            _countTextBlock.Text = MaxCount >= 0 ? $"{snapshot.Count}/{MaxCount}" : string.Empty;
        }
    }
}
