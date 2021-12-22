using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Pixeval.CoreApi.Model;
using Pixeval.Util.IO;
using Pixeval.Utilities;
using AppContext = Pixeval.AppManagement.AppContext;

namespace Pixeval.Controls.IllustratorView;

public partial class IllustratorViewModel : ObservableObject
{
    public string Name { get; set; }

    public string AvatarUrl { get; set; }

    [ObservableProperty] private ImageSource? _avatarSource;
    
    public long Id { get; set; }
    
    public string? Account { get; set; }
    
    public string? Comment { get; set; }

    public IllustratorViewModel(UserInfo info)
    {
        Name = info.Name!;
        AvatarUrl = info.ProfileImageUrls?.Medium!;
        Id = info.Id;
        Account = info.Account;
        Comment = info.Comment;
    }

    public async Task LoadAvatarSource()
    {
        if (AvatarSource != null) return;
        AvatarSource = (await App.AppViewModel.MakoClient.DownloadBitmapImageResultAsync(AvatarUrl)
            .GetOrElseAsync(await AppContext.GetPixivNoProfileImageAsync())!)!;
    }
}