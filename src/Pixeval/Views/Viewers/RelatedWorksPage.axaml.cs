// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Avalonia;
using Avalonia.Controls;

namespace Pixeval.Views.Viewers;

public partial class RelatedWorksPage : ContentPage
{
    public static readonly StyledProperty<long> IllustrationIdProperty =
        AvaloniaProperty.Register<RelatedWorksPage, long>(nameof(IllustrationId));

    public long IllustrationId
    {
        get => GetValue(IllustrationIdProperty);
        set => SetValue(IllustrationIdProperty, value);
    }

    public RelatedWorksPage() => InitializeComponent();

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IllustrationIdProperty)
        {
            var id = change.GetNewValue<long>();
            if (id is 0)
                return;
            RelatedWorksView.ResetEngine(App.AppViewModel.MakoClient.IllustrationRelated(id));
        }
    }
}
