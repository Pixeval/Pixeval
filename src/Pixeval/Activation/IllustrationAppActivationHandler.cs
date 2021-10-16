using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Messages;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using Pixeval.ViewModel;

namespace Pixeval.Activation
{
    public class IllustrationAppActivationHandler : IAppActivationHandler
    {
        public Task Execute(string id)
        {
            WeakReferenceMessenger.Default.Send(new MainPageFrameSetConnectedAnimationTargetMessage(App.AppViewModel.AppWindowRootFrame));

            return App.AppViewModel.DispatchTaskAsync(async () =>
            {
                App.AppViewModel.PrepareForActivation();

                try
                {
                    var viewModels = new IllustrationViewModel(await App.AppViewModel.MakoClient.GetIllustrationFromIdAsync(id))
                        .GetMangaIllustrationViewModels()
                        .ToArray();

                    ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation", App.AppViewModel.AppWindowRootFrame);
                    App.AppViewModel.RootFrameNavigate(typeof(IllustrationViewerPage), new IllustrationViewerPageViewModel(viewModels), new SuppressNavigationTransitionInfo());
                }
                catch (Exception e)
                {
                    UIHelper.ShowTextToastNotification(
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
}