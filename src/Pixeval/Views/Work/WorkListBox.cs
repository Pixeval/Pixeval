// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace Pixeval.Views.Work;

public sealed class WorkListBox : ListBox
{
    private static readonly AttachedProperty<WorkItem?> ItemProperty =
        AvaloniaProperty.RegisterAttached<WorkListBox, ListBoxItem, WorkItem?>("Item");

    protected override Type StyleKeyOverride => typeof(ListBox);

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        var needsContainer = base.NeedsContainerOverride(item, index, out recycleKey);
        if (needsContainer)
            recycleKey = this.FindDataTemplate(item);
        return needsContainer;
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        if (ReferenceEquals(container, item))
        {
            base.PrepareContainerForItemOverride(container, item, index);
            return;
        }

        var card = container.GetValue(ItemProperty);
        if (card is null)
        {
            card = this.FindDataTemplate(item)?.Build(item) as WorkItem
                ?? throw new InvalidOperationException("Work list templates must create a WorkItem.");
            container.SetValue(ItemProperty, card);
        }

        card.DataContext = item;
        ((ListBoxItem) container).Content = card;
        base.PrepareContainerForItemOverride(container, item, index);
    }

    protected override void ClearContainerForItemOverride(Control container)
    {
        container.GetValue(ItemProperty)?.Recycle();
        base.ClearContainerForItemOverride(container);
        container.DataContext = null;
    }
}
