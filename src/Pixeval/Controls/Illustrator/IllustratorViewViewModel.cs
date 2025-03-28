// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Mako.Model;
using IllustratorViewDataProvider = Pixeval.Controls.SimpleViewDataProvider<
    Mako.Model.User,
    Pixeval.Controls.IllustratorItemViewModel>;

namespace Pixeval.Controls;

public sealed partial class IllustratorViewViewModel : EntryViewViewModel<User, IllustratorItemViewModel>
{
    public override IllustratorViewDataProvider DataProvider { get; } = new();

    public IllustratorViewViewModel() => DataProvider.View.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasNoItem));
}
