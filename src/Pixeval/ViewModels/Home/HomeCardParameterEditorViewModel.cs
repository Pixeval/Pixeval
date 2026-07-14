// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Pixeval.Controls;
using Pixeval.Views.Home;

namespace Pixeval.ViewModels.Home;

public abstract class HomeCardParameterEditorViewModel(
    HomeCardParameterKinds kind,
    string header) : ObservableObject
{
    public HomeCardParameterKinds Kind { get; } = kind;

    public string Header { get; } = header;
}

public sealed partial class HomeCardChoiceParameterEditorViewModel(
    HomeCardParameterKinds kind,
    string header,
    IReadOnlyList<SymbolComboBoxItem> items,
    object value) : HomeCardParameterEditorViewModel(kind, header)
{
    private object _value = items.Any(item => Equals(item.Value, value))
        ? value
        : throw new ArgumentException("The initial value must be present in the supplied items.", nameof(value));

    public event EventHandler? ValueChanged;

    [ObservableProperty] public partial IReadOnlyList<SymbolComboBoxItem> Items { get; private set; } = items;

    public object Value
    {
        get => _value;
        set
        {
            // A two-way binding can briefly push null or the previous data context's value
            // while an ItemsControl replaces its data template. Keep the last valid choice.
            if (!Items.Any(item => Equals(item.Value, value)) || !SetProperty(ref _value, value))
                return;

            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public T GetValue<T>()
        where T : struct, Enum =>
        Value is T typedValue
            ? typedValue
            : throw new InvalidOperationException($"The {Kind} editor does not contain a {typeof(T).Name} value.");

    public void Reset(IReadOnlyList<SymbolComboBoxItem> newItems, object newValue)
    {
        if (!newItems.Any(item => Equals(item.Value, newValue)))
            throw new ArgumentException("The new value must be present in the supplied items.", nameof(newValue));

        Items = newItems;
        Value = newValue;
    }
}

public sealed partial class HomeCardTextParameterEditorViewModel(
    HomeCardParameterKinds kind,
    string header) : HomeCardParameterEditorViewModel(kind, header)
{
    [ObservableProperty] public partial string Text { get; set; } = "";
}

public sealed partial class HomeCardRankingDateParameterEditorViewModel(
    string header,
    DateTime selectedDate) : HomeCardParameterEditorViewModel(HomeCardParameterKinds.RankingDate, header)
{
    [ObservableProperty] public partial bool UseSpecifiedDate { get; set; }

    [ObservableProperty] public partial DateTime SelectedDate { get; set; } = selectedDate;

    public void Reset(DateTime date)
    {
        UseSpecifiedDate = false;
        SelectedDate = date;
    }
}
