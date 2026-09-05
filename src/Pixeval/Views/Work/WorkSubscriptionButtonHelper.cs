// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace Pixeval.Views.Work;

internal static class WorkSubscriptionButtonHelper
{
    public static void UpdateVisibility(
        CommandBarButton addButton,
        CommandBarButton removeButton,
        bool isSubscribed)
    {
        addButton.IsVisible = !isSubscribed;
        removeButton.IsVisible = isSubscribed;
    }

    public static async Task<TResult> RunAsync<TResult>(
        CommandBarButton addButton,
        CommandBarButton removeButton,
        Func<Task<TResult>> operation,
        Action updateButtons)
    {
        addButton.IsVisible = false;
        removeButton.IsVisible = false;
        try
        {
            return await operation();
        }
        finally
        {
            updateButtons();
        }
    }
}
