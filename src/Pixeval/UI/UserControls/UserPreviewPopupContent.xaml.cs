using System.Windows;
using System.Windows.Media.Imaging;
using Pixeval.Core;
using Pixeval.Data.ViewModel;
using Pixeval.Data.Web.Delegation;
using Pixeval.Objects.Primitive;

namespace Pixeval.UI.UserControls
{
    public partial class UserPreviewPopupContent
    {
        public UserPreviewPopupContent()
        {
            InitializeComponent();
        }

        public async void SetImages(BitmapImage left, BitmapImage center, BitmapImage right)
        {
            UiHelper.SetImageSource(ImgLeft, left);
            UiHelper.SetImageSource(ImgCenter, center);
            UiHelper.SetImageSource(ImgRight, right);
            UiHelper.SetImageSource(UserAvatar, await PixivIO.FromUrl(this.GetDataContext<User>().Avatar));
            try
            {
                UiHelper.SetImageSource(Banner, await PixivIO.FromUrl((await HttpClientFactory.WebApiService().GetWebApiUserDetail(this.GetDataContext<User>().Id)).ResponseBody.UserDetails.CoverImage.ProfileCoverImage.The720X360));
            }
            catch
            {
                /* ignore */
            }
        }

        private async void FollowButton_OnClick(object sender, RoutedEventArgs e)
        {
            var usr = sender.GetDataContext<User>();
            await PixivClient.Instance.FollowArtist(usr, RestrictPolicy.Public);
        }

        private async void UnFollowButton_OnClick(object sender, RoutedEventArgs e)
        {
            var usr = sender.GetDataContext<User>();
            await PixivClient.Instance.UnFollowArtist(usr);
        }
    }
}
