// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Threading.Tasks;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.Util.Threading;
using Pixeval.Util.UI;
using Pixeval.Utilities;

namespace Pixeval.Activation;

public class IllustrationAppActivationHandler : IAppActivationHandler
{
    public string ActivationFragment => "illust";

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
                await IllustrationViewerHelper.CreateWindowWithPageAsync(id);
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
