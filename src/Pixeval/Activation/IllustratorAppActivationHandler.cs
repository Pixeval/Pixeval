// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Threading.Tasks;
using Pixeval.Pages;
using Pixeval.Pages.IllustratorViewer;
using Pixeval.Util.Threading;
using Pixeval.Util.UI;
using Pixeval.Utilities;

namespace Pixeval.Activation;

public class IllustratorAppActivationHandler : IAppActivationHandler
{
    public string ActivationFragment => "user";

    public Task Execute(string param)
    {
        if (!long.TryParse(param, out var id))
        {
            return Task.CompletedTask;
        }

        return ThreadingHelper.DispatchTaskAsync(async () =>
        {
            try
            {
                await MainPage.Current.TabViewParameter.CreateIllustratorPageAsync(id);
            }
            catch (Exception e)
            {
                AppNotificationHelper.ShowTextAppNotification(
                    ActivationsResources.ActivationFailedTitle,
                    ActivationsResources.ActivationFailedContentFormatted.Format(e.Message));
            }
        });
    }
}
