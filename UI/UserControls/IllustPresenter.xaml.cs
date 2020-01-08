// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019 Dylech30th
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
using System.Windows;
using System.Windows.Media;
using Pixeval.Data.ViewModel;
using Pixeval.Objects;
using PropertyChanged;

namespace Pixeval.UI.UserControls
{
    /// <summary>
    ///     Interaction logic for IllustPresenter.xaml
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public partial class IllustPresenter
    {
        public IllustPresenter(ImageSource imgSource, Illustration illustration)
        {
            ImgSource = imgSource;
            Illust = illustration;
            InitializeComponent();
        }

        public ImageSource ImgSource { get; set; }

        public Illustration Illust { get; set; }

        private async void CopyImageItem_OnClick(object sender, RoutedEventArgs e)
        {
            Clipboard.SetImage(await PixivEx.GetAndCreateOrLoadFromCache(false, Illust.Origin, Illust.Id));
        }
    }
}