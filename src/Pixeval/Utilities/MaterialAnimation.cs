// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using ComposeAnimation;

namespace Pixeval.Utilities;

public static class MaterialAnimation
{
    public static readonly AttachedProperty<bool> InitializeScopeProperty =
        AvaloniaProperty.RegisterAttached<TopLevel, bool>(
            "InitializeScope",
            typeof(MaterialAnimation));

    private static readonly ConditionalWeakTable<TopLevel, AnimationScope> _Scopes = new();

    static MaterialAnimation()
    {
        InitializeScopeProperty.Changed.AddClassHandler<TopLevel>(static (host, args) =>
        {
            if (args.GetNewValue<bool>())
                Initialize(host);
        });
    }

    public static bool GetInitializeScope(TopLevel host) => host.GetValue(InitializeScopeProperty);

    public static void SetInitializeScope(TopLevel host, bool value) => host.SetValue(InitializeScopeProperty, value);

    internal static void Initialize(TopLevel host)
    {
        // TopLevel styling also reaches platform-owned mobile/browser roots before child templates run.
        // Popup roots inherit the owner's animation environment and must not create a second one.
        if (host is PopupRoot)
            return;

        _ = _Scopes.GetValue(host, static topLevel =>
        {
            var scope = AnimationScope.Initialize(topLevel);
            scope.AnimationFailed += static (_, args) =>
            {
                if (Application.Current is App app)
                    app.LogAnimationFailure(args);
            };
            return scope;
        });
    }
}
