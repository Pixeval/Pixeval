using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.UI;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Model;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.UserControls;
using Pixeval.Util.IO;
using Pixeval.Utilities;
using AppContext = Pixeval.AppManagement.AppContext;

namespace Pixeval.Controls.IllustratorView;

public partial class IllustratorViewModel : ObservableObject, IIllustrationVisualizer
{
    public string Name { get; set; }

    public string AvatarUrl { get; set; }

    [ObservableProperty]
    private ImageSource? _avatarSource;

    public IFetchEngine<Illustration?> FetchEngine => App.AppViewModel.MakoClient.Posts(Id.ToString());

    public long Id { get; set; }

    public string? Account { get; set; }

    public string? Comment { get; set; }

    public ObservableCollection<IllustrationViewModel> Illustrations { get; }

    public IllustrationVisualizationController VisualizationController { get; internal set; }

    public IllustratorViewModel(UserInfo info)
    {
        Name = info.Name!;
        AvatarUrl = info.ProfileImageUrls?.Medium!;
        Id = info.Id;
        Account = info.Account;
        Comment = info.Comment;
        Illustrations = new ObservableCollection<IllustrationViewModel>();
        VisualizationController = new IllustrationVisualizationController(this);
        _ = LoadAvatar();
    }

    public async Task LoadAvatar()
    {
        if (AvatarSource != null) return;
        AvatarSource = (await App.AppViewModel.MakoClient.DownloadBitmapImageResultAsync(AvatarUrl)
            .GetOrElseAsync(await AppContext.GetPixivNoProfileImageAsync())!)!;
    }

    public async Task LoadThumbnail()
    {
        await VisualizationController.ResetAndFillAsync(FetchEngine, 3);
        foreach (var model in Illustrations)
        {
            await model.LoadThumbnailIfRequired();
        }
    }

    public void DisposeCurrent()
    {
        Illustrations.Clear();
    }

    public void AddIllustrationViewModel(IllustrationViewModel viewModel)
    {
        Illustrations.Add(viewModel);
    }
}