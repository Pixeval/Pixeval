using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Pixeval.Controls.IllustratorView;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.Util.IO;
using Pixeval.Utilities;
using AppContext = Pixeval.AppManagement.AppContext;

namespace Pixeval.Pages.Capability;

public class FollowingsPageViewModel : ObservableObject
{
    private ObservableCollection<IllustratorViewModel> _illustrators;

    public ObservableCollection<IllustratorViewModel> Illustrators
    {
        get => _illustrators;
        set => SetProperty(ref _illustrators, value);
    }

    public FollowingsPageViewModel()
    {
        _illustrators = new ObservableCollection<IllustratorViewModel>();
    }

    public async Task LoadFollowings()
    {
        var fetchEngine = App.AppViewModel.MakoClient.Following(App.AppViewModel.PixivUid!, PrivacyPolicy.Public);
        await foreach (var user in fetchEngine)
        {
            var model = new IllustratorViewModel(user.UserInfo!);
            _illustrators.Add(model);
            _ = model.LoadAvatarSource();
        }
    }
}