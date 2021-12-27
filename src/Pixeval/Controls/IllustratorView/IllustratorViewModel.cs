using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Global.Enum;
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
    private bool _isFollowed;

    /// <summary>
    /// disable the follow button while follow request is being sent
    /// </summary>
    [ObservableProperty]
    private bool _isFollowButtonEnabled;

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
        IsFollowed = info.IsFollowed;
        Illustrations = new ObservableCollection<IllustrationViewModel>();
        VisualizationController = new IllustrationVisualizationController(this);
        IsFollowButtonEnabled = true;
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

    public async Task Follow()
    {
        IsFollowButtonEnabled = false;
        await App.AppViewModel.MakoClient.PostFollowUserAsync(Id.ToString(), PrivacyPolicy.Public);
        IsFollowed = true;
        IsFollowButtonEnabled = true;
    }

    public async Task PrivateFollow()
    {
        IsFollowButtonEnabled = false;
        await App.AppViewModel.MakoClient.PostFollowUserAsync(Id.ToString(), PrivacyPolicy.Private);
        IsFollowed = true;
        IsFollowButtonEnabled = true;
    }

    public async Task Unfollow()
    {
        IsFollowButtonEnabled = false;
        await App.AppViewModel.MakoClient.RemoveFollowUserAsync(Id.ToString());
        IsFollowed = false;
        IsFollowButtonEnabled = true;
    }

    public void DisposeCurrent()
    {
        Illustrations.Clear();
    }

    public void AddIllustrationViewModel(IllustrationViewModel viewModel)
    {
        Illustrations.Add(viewModel);
    }

    public Visibility IsNotFollowed(bool followed) => followed ? Visibility.Collapsed : Visibility.Visible;
}