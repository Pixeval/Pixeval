// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using UserViewDataProvider = Pixeval.ViewModels.SimpleViewDataProvider<
    Mako.Model.User,
    Pixeval.ViewModels.UserItemViewModel>;

namespace Pixeval.ViewModels;

public sealed class UserViewViewModel : EntryViewViewModel<Mako.Model.User, UserItemViewModel>
{
    public override UserViewDataProvider DataProvider { get; } = new();
}
