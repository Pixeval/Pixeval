#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/IllustrationAppActivationHandler.cs
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

using System;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Messages;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.UserControls;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using WinUI3Utilities;

namespace Pixeval.Activation;

public class IllustrationAppActivationHandler : IAppActivationHandler
{
    public string ActivationFragment => "illust";

    public Task Execute(string id)
    {
        WeakReferenceMessenger.Default.Send(new MainPageFrameSetConnectedAnimationTargetMessage(CurrentContext.Frame));

        return App.AppViewModel.DispatchTaskAsync(async () =>
        {
            App.AppViewModel.PrepareForActivation();

            try
            {
                var viewModels = new IllustrationViewModel(await App.AppViewModel.MakoClient.GetIllustrationFromIdAsync(id))
                    .GetMangaIllustrationViewModels()
                    .ToArray();

                ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation", CurrentContext.Frame);
                App.AppViewModel.RootFrameNavigate(typeof(IllustrationViewerPage), new IllustrationViewerPageViewModel(viewModels), new SuppressNavigationTransitionInfo());
            }
            catch (Exception e)
            {
                ToastNotificationHelper.ShowTextToastNotification(
                    ActivationsResources.IllustrationActivationFailedTitle,
                    ActivationsResources.IllustrationActivationFailedContentFormatted.Format(e.Message));
            }
            finally
            {
                App.AppViewModel.ActivationProcessed();
            }
        });
    }
}
