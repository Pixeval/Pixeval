// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Mako.Model;
using UserViewDataProvider = Pixeval.ViewModels.SharableViewDataProvider<
    Mako.Model.User,
    Pixeval.ViewModels.UserItemViewModel>;

namespace Pixeval.ViewModels;

public sealed class UserViewViewModel : EntryViewViewModel<User, UserItemViewModel>
{
    public UserViewViewModel(UserViewViewModel viewModel) : this(viewModel.DataProvider.CloneRef())
    {
    }

    public UserViewViewModel() : this(new UserViewDataProvider())
    {
    }

    private UserViewViewModel(UserViewDataProvider dataProvider)
    {
        DataProvider = dataProvider;
    }

    public override UserViewDataProvider DataProvider { get; }
}
