using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Pixeval.Utilities;

namespace Pixeval.Views.Work;

public class AddNewBookmarkTagItem : TemplatedControl
{
    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<AddNewBookmarkTagItem, bool>(nameof(IsExpanded));

    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public static readonly DirectProperty<AddNewBookmarkTagItem, AddNewBookmarkTag?> SourceProperty =
        AvaloniaProperty.RegisterDirect<AddNewBookmarkTagItem, AddNewBookmarkTag?>(
            nameof(Source), o => o.Source, (o, v) => o.Source = v);

    public AddNewBookmarkTag? Source
    {
        get;
        set => SetAndRaise(SourceProperty, ref field, value);
    }

    private Button? _addButton;
    private Button? _confirmButton;
    private Button? _cancelButton;
    private TextBox? _textBox;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _addButton?.Click -= OnAddClick;
        _confirmButton?.Click -= OnConfirmClick;
        _confirmButton?.Click -= OnCancelClick;
        _cancelButton?.Click -= OnCancelClick;

        _addButton = e.NameScope.Find<Button>("PART_AddButton");
        _confirmButton = e.NameScope.Find<Button>("PART_ConfirmButton");
        _cancelButton = e.NameScope.Find<Button>("PART_CancelButton");
        _textBox = e.NameScope.Find<TextBox>("PART_TextBox");

        _addButton?.Click += OnAddClick;
        _confirmButton?.Click += OnConfirmClick;
        _confirmButton?.Click += OnCancelClick;
        _cancelButton?.Click += OnCancelClick;
    }

    private void OnAddClick(object? sender, RoutedEventArgs e)
    {
        IsExpanded = true;
        _textBox?.Focus();
    }

    private void OnConfirmClick(object? sender, RoutedEventArgs e)
    {
        var name = _textBox?.Text?.Trim();
        if (!string.IsNullOrEmpty(name))
            Source?.TagAdded.Invoke(Source, name);
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        _textBox?.Text = "";
        IsExpanded = false;
    }
}
