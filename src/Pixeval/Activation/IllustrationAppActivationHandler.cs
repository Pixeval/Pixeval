#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IllustrationAppActivationHandler.cs
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
                ToastNotificationHelper.ShowTextToastNotification(
                    ActivationsResources.IllustrateActivationFailedTitle,
                    ActivationsResources.IllustrateActivationFailedContentFormatted.Format(e.Message));
            }
        });
    }
}
