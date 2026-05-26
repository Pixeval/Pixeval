// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Mako.Model;
using UserViewDataProvider = Pixeval.ViewModels.SharableViewDataProvider<
    Mako.Model.User,
    Pixeval.ViewModels.UserItemViewModel>;

namespace Pixeval.ViewModels;

public sealed class UserViewViewModel : EntryViewViewModel<User, UserItemViewModel>
{
    protected override UserViewDataProvider DataProvider { get; } = new();
}
