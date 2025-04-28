// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Windows.AppLifecycle;
using Pixeval.Pages;
using Pixeval.Pages.Login;
using Pixeval.Util.Threading;
using Pixeval.Util.UI;
using Windows.ApplicationModel.Activation;
using Misaki;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.Pages.IllustratorViewer;
using Pixeval.Pages.NovelViewer;
using Pixeval.Utilities;

namespace Pixeval.Activation;

public static class ActivationRegistrar
{
    public static async void Dispatch(AppActivationArguments args)
    {
        if (args is
            {
                Kind: ExtendedActivationKind.Protocol, Data: IProtocolActivatedEventArgs { Uri: var activationUri }
            })
        {
            switch (activationUri.Scheme)
            {
                case "pixeval":
                {
                    var seg1 = activationUri.Segments[1].TrimEnd('/');
                    try
                    {
                        _ = ThreadingHelper.DispatchTaskAsync(() =>
                            activationUri.Host switch
                            {
                                "novel" => MainPage.Current.TabViewParameter.CreateNovelPageAsync(long.Parse(seg1)),
                                "user" => MainPage.Current.TabViewParameter.CreateIllustratorPageAsync(long.Parse(seg1)),
                                "illust" => MainPage.Current.TabViewParameter.CreateIllustrationPageAsync(seg1, IPlatformInfo.Pixiv),
                                IPlatformInfo.Sankaku => MainPage.Current.TabViewParameter.CreateIllustrationPageAsync(seg1, IPlatformInfo.Sankaku),
                                IPlatformInfo.Danbooru => MainPage.Current.TabViewParameter.CreateIllustrationPageAsync(seg1, IPlatformInfo.Danbooru),
                                IPlatformInfo.Gelbooru => MainPage.Current.TabViewParameter.CreateIllustrationPageAsync(seg1, IPlatformInfo.Gelbooru),
                                IPlatformInfo.Yandere => MainPage.Current.TabViewParameter.CreateIllustrationPageAsync(seg1, IPlatformInfo.Yandere),
                                IPlatformInfo.Rule34 => MainPage.Current.TabViewParameter.CreateIllustrationPageAsync(seg1, IPlatformInfo.Rule34),
                                _ => Task.CompletedTask
                            });
                    }
                    catch (Exception e)
                    {
                        AppNotificationHelper.ShowTextAppNotification(
                            ActivationsResources.ActivationFailedTitle,
                            ActivationsResources.ActivationFailedContentFormatted.Format(e.Message));
                    }

                    break;
                }
                case "pixiv":
                {
                    if (LoginPage.Current is null || LoginPage.CurrentVerifier is null || !App.AppViewModel.MakoClient.IsBuilt)
                        return;
                    var code = HttpUtility.ParseQueryString(activationUri.Query)["code"]!;
                    var tokenResponse = await PixivAuth.AuthCodeToTokenResponseAsync(code, LoginPage.CurrentVerifier);
                    if (tokenResponse is null)
                        return;
                    App.AppViewModel.MakoClient.Build(tokenResponse);
                    _ = ThreadingHelper.DispatchAsync(LoginPage.SuccessNavigating);
                    break;
                }
            }
        }
    }
}
