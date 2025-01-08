// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Activation;
using Microsoft.Windows.AppLifecycle;
using System.Web;
using Pixeval.Pages.Login;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.CoreApi;
using Pixeval.Logging;

namespace Pixeval.Activation;

public static class ActivationRegistrar
{
    public static readonly List<IAppActivationHandler> FeatureHandlers =
    [
        new IllustrationAppActivationHandler(),
        new IllustratorAppActivationHandler(),
        new NovelAppActivationHandler()
    ];

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
                    if (FeatureHandlers.FirstOrDefault(f => f.ActivationFragment == activationUri.Host) is { } handler)
                    {
                        _ = handler.Execute(activationUri.PathAndQuery[1..]);
                    }

                    break;
                }
                case "pixiv":
                {
                    if (LoginPage.Current is null || App.AppViewModel.MakoClient != null!)
                        return;
                    var code = HttpUtility.ParseQueryString(activationUri.Query)["code"]!;
                    var tokenResponse = await PixivAuth.AuthCodeToTokenResponseAsync(code, PixivAuth.GetCodeVerify());
                    var logger = App.AppViewModel.AppServiceProvider.GetRequiredService<FileLogger>();
                    App.AppViewModel.MakoClient = new MakoClient(tokenResponse, App.AppViewModel.AppSettings.ToMakoClientConfiguration(), logger);
                    LoginPage.SuccessNavigating();
                    break;
                }
            }
        }
    }
}
