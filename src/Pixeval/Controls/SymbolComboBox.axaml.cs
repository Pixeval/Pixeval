// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoSettingsPage;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using FluentIcons.Common;
using Pixeval.Attributes;
using Pixeval.Models.Settings;

namespace Pixeval.Controls;

public record SymbolComboBoxItem(object Value, string Description, Symbol Symbol) : IReadOnlyStringPair<object>
{
    /// <inheritdoc />
    public override string ToString() => Description;

    public static IReadOnlyList<SymbolComboBoxItem> GetValues<TEnum>()
        where TEnum : struct, Enum
    {
        if (LocalSettingsEntryHelper.RegisteredAttach.TryGetValue(typeof(TEnum), out var value))
            return value;

        var list = new List<SymbolComboBoxItem>();
        var fieldInfos = typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static);
        foreach (var fieldInfo in fieldInfos)
            if (fieldInfo.GetCustomAttribute<LocalizedResourceAttribute>() is { } attribute)
                list.Add(new SymbolComboBoxItem(fieldInfo.GetValue(null)!, attribute.Resource, attribute.Symbol));
        LocalSettingsEntryHelper.RegisteredAttach[typeof(TEnum)] = list;
        return list;
    }

    public static IReadOnlyList<SymbolComboBoxItem> GetValues(object key) => LocalSettingsEntryHelper.RegisteredAttach[key];

    public static string GetResource<TEnum>(TEnum e)
        where TEnum : struct, Enum =>
        GetValues<TEnum>().First(t => t.Value.Equals(e)).Description;
}

/// <summary>
/// 一个自定义 ComboBox。<br/>
/// <see cref="IconMode"/> 为 <c>true</c> 时：收起显示 SymbolIcon + 下拉箭头，展开每项显示 SymbolIcon + Description。<br/>
/// <see cref="IconMode"/> 为 <c>false</c> 时：收起显示 Description + 下拉箭头，展开每项只显示 Description。<br/>
/// 外部可通过 <see cref="SelectedValue"/> 直接拿到选中项的 Value。
/// </summary>
public class SymbolComboBox : TemplatedControl
{
    public static readonly StyledProperty<IReadOnlyList<SymbolComboBoxItem>?> ItemsSourceProperty =
        AvaloniaProperty.Register<SymbolComboBox, IReadOnlyList<SymbolComboBoxItem>?>(nameof(ItemsSource));

    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<SymbolComboBox, int>(nameof(SelectedIndex));

    public static readonly StyledProperty<bool> IconModeProperty =
        AvaloniaProperty.Register<SymbolComboBox, bool>(nameof(IconMode), true);

    public static readonly DirectProperty<SymbolComboBox, object?> SelectedValueProperty =
        AvaloniaProperty.RegisterDirect<SymbolComboBox, object?>(
            nameof(SelectedValue),
            o => o.SelectedValue,
            (o, v) => o.SelectedValue = v,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<IDataTemplate?> IconItemTemplateProperty =
        AvaloniaProperty.Register<SymbolComboBox, IDataTemplate?>(nameof(IconItemTemplate));

    public static readonly StyledProperty<IDataTemplate?> IconSelectionBoxTemplateProperty =
        AvaloniaProperty.Register<SymbolComboBox, IDataTemplate?>(nameof(IconSelectionBoxTemplate));

    public static readonly StyledProperty<IDataTemplate?> TextItemTemplateProperty =
        AvaloniaProperty.Register<SymbolComboBox, IDataTemplate?>(nameof(TextItemTemplate));

    private ComboBox? _innerComboBox;
    private ContentControl? _selectionBoxPresenter;
    private bool _isSynchronizingSelection;
    private bool _preferSelectedValue;

    /// <summary>
    /// 也需要继承<see cref="IList"/>才能正常使用
    /// </summary>
    public IReadOnlyList<SymbolComboBoxItem>? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    /// <summary>
    /// 为 <c>true</c> 时显示图标模式，为 <c>false</c> 时显示文字模式
    /// </summary>
    public bool IconMode
    {
        get => GetValue(IconModeProperty);
        set => SetValue(IconModeProperty, value);
    }

    /// <summary>
    /// Icon 模式下拉项模板：左侧 SymbolIcon + 右侧 Description
    /// </summary>
    public IDataTemplate? IconItemTemplate
    {
        get => GetValue(IconItemTemplateProperty);
        set => SetValue(IconItemTemplateProperty, value);
    }

    /// <summary>
    /// Icon 模式收起状态模板：只显示 SymbolIcon
    /// </summary>
    public IDataTemplate? IconSelectionBoxTemplate
    {
        get => GetValue(IconSelectionBoxTemplateProperty);
        set => SetValue(IconSelectionBoxTemplateProperty, value);
    }

    /// <summary>
    /// Text 模式模板（下拉项和收起状态共用）：只显示 Description
    /// </summary>
    public IDataTemplate? TextItemTemplate
    {
        get => GetValue(TextItemTemplateProperty);
        set => SetValue(TextItemTemplateProperty, value);
    }

    public object? SelectedValue
    {
        get;
        set => SetAndRaise(SelectedValueProperty, ref field, value);
    }

    public T GetSelectedValue<T>()
    {
        if (SelectedValue is T t)
            return t;
        throw new InvalidOperationException();
    }

    /// <summary>
    /// 选中项变更时触发
    /// </summary>
    public event EventHandler<SymbolComboBox, EventArgs>? SelectionChanged;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_innerComboBox is not null)
        {
            _innerComboBox.SelectionChanged -= OnInnerSelectionChanged;
            _innerComboBox.TemplateApplied -= OnInnerComboBoxTemplateApplied;
        }

        _innerComboBox = e.NameScope.Find<ComboBox>("PART_ComboBox");
        if (_innerComboBox is not null)
        {
            _innerComboBox.SelectionChanged += OnInnerSelectionChanged;
            _innerComboBox.TemplateApplied += OnInnerComboBoxTemplateApplied;
            _innerComboBox.Classes.Set("text", !IconMode);
        }

        SyncSelection();
    }

    private void OnInnerComboBoxTemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
        _selectionBoxPresenter = e.NameScope.Find<ContentControl>("ContentPresenter");
        UpdateSelectionBoxTemplate();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SelectedValueProperty)
        {
            if (_isSynchronizingSelection)
                return;

            _preferSelectedValue = true;
            SyncSelectionFromValue();
            return;
        }

        if (change.Property == SelectedIndexProperty)
        {
            if (_isSynchronizingSelection || _preferSelectedValue)
                return;

            SyncSelectionFromIndex();
            return;
        }

        if (change.Property == ItemsSourceProperty)
        {
            if (change.NewValue is null)
                return;
            SyncSelection();
            return;
        }

        if (change.Property == IconModeProperty)
        {
            PseudoClasses.Set(":text", !IconMode);
            _innerComboBox?.Classes.Set("text", !IconMode);
            UpdateSelectionBoxTemplate();
        }

        if (change.Property == IconSelectionBoxTemplateProperty || change.Property == TextItemTemplateProperty)
        {
            UpdateSelectionBoxTemplate();
        }
    }

    private void OnInnerSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_innerComboBox is null)
            return;

        SyncSelectionFromInnerComboBox();

        // ComboBox 选中项变化时会以 LocalValue 优先级重设 ContentTemplate 为 ItemTemplate，
        // 需要在之后重新设置为我们期望的模板
        UpdateSelectionBoxTemplate();

        if (IsLoaded)
            SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 将内部 ComboBox 的选择框 ContentPresenter 的 ContentTemplate
    /// 设为当前模式对应的模板（Icon 模式用 IconSelectionBoxTemplate，Text 模式用 TextItemTemplate）。
    /// 必须以 LocalValue 优先级设置，因为 ComboBox 内部也是用 LocalValue 赋值的。
    /// </summary>
    private void UpdateSelectionBoxTemplate()
    {
        _selectionBoxPresenter?.ContentTemplate = IconMode ? IconSelectionBoxTemplate : TextItemTemplate;
    }

    private void SyncSelection()
    {
        if (_preferSelectedValue)
            SyncSelectionFromValue();
        else 
            SyncSelectionFromIndex();
    }

    private void SyncSelectionFromValue()
    {
        var index = FindIndexByValue(SelectedValue);
        SetSelectedIndexCore(index);
    }

    private void SyncSelectionFromIndex()
    {
        if (ItemsSource is null || SelectedIndex < 0 || SelectedIndex >= ItemsSource.Count)
        {
            SetSelectedValueCore(null);
            return;
        }

        var item = ItemsSource[SelectedIndex];
        SetSelectedValueCore(item.Value);
    }

    private void SyncSelectionFromInnerComboBox()
    {
        if (_innerComboBox is null)
            return;

        SetSelectedIndexCore(_innerComboBox.SelectedIndex);
        SetSelectedValueCore((_innerComboBox.SelectedItem as SymbolComboBoxItem)?.Value);
    }

    private int FindIndexByValue(object? value)
    {
        if (ItemsSource is null)
            return -1;

        for (var i = 0; i < ItemsSource.Count; i++)
            if (Equals(ItemsSource[i].Value, value))
                return i;

        return -1;
    }

    private void SetSelectedIndexCore(int value)
    {
        if (SelectedIndex == value)
            return;

        _isSynchronizingSelection = true;
        try
        {
            SetCurrentValue(SelectedIndexProperty, value);
        }
        finally
        {
            _isSynchronizingSelection = false;
        }
    }

    private void SetSelectedValueCore(object? value)
    {
        if (Equals(SelectedValue, value))
            return;

        _isSynchronizingSelection = true;
        try
        {
            SelectedValue = value;
        }
        finally
        {
            _isSynchronizingSelection = false;
        }
    }
}
