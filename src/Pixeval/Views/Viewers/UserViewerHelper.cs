// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Threading.Tasks;
using Mako.Net.Response;
using Pixeval.ViewModels.Viewers;
using Pixeval.Views.ViewContainers;

namespace Pixeval.Views.Viewers;

public static class UserViewerHelper
{
    /// <param name="control"></param>
    extension(ViewContainerBase control)
    {
        public async Task CreateUserPageAsync(long userId)
        {
            var userDetail = await App.AppViewModel.MakoClient.GetUserFromIdAsync(userId);
            control.CreateUserPage(userDetail);
        }

        public void CreateUserPage(SingleUserResponse userDetail)
        {
            var viewModel = new UserViewerPageViewModel(userDetail);
            control.NavigateTo(new UserViewerPage(viewModel));
        }
    }

    public static UserViewerPageViewModel GetViewModelFromParameter(object? param)
    {
        return param switch
        {
            SingleUserResponse userDetail => new UserViewerPageViewModel(userDetail),
            _ => throw new ArgumentException("Invalid parameter type.", nameof(param))
        };
    }
}
