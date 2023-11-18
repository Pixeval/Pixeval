#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/RecommendIllustratorProfile.xaml.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Pixeval.Util.Threading;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls.RecommendIllustratorProfile;

[DependencyProperty<RecommendIllustratorProfileViewModel>("ViewModel")]
public sealed partial class RecommendIllustratorProfile
{
    private const string ButtonPointerOverBackgroundKey = "ButtonBackgroundPointerOver";

    public RecommendIllustratorProfile()
    {
        InitializeComponent();
    }

    private void RecommendIllustratorProfile_OnLoaded(object sender, RoutedEventArgs e)
    {
        _ = ThreadingHelper.Fork(async () =>
        {
            if (ViewModel.AvatarTask is { } task)
            {
                var result = await task;
                AvatarPersonPicture.ProfilePicture = result;
            }
        });
        _ = ThreadingHelper.Fork(async () =>
        {
            if (ViewModel.DisplayImagesTask is { } task)
            {
                var results = await task;
                var averageLength = 300 / results.Length;
                var images = results.Select(s => new Image
                {
                    Width = averageLength,
                    Height = 100,
                    Stretch = Stretch.UniformToFill,
                    Source = s
                });
                IllustratorDisplayImages.Children.AddRange(images);
            }
        });
    }

    private void FollowButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        if (ViewModel.Followed)
        {
            ViewModel.Unfollow();
        }
        else
        {
            ViewModel.Follow();
        }

        Resources[ButtonPointerOverBackgroundKey] = new SolidColorBrush(ViewModel.GetButtonBackground(ViewModel.Followed).Color.Brighten(25));
    }

    private void FollowButton_OnLoaded(object sender, RoutedEventArgs e)
    {
        Resources[ButtonPointerOverBackgroundKey] = new SolidColorBrush(ViewModel.GetButtonBackground(ViewModel.Followed).Color.Brighten(25));
    }
}