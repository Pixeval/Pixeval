#region Copyright
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/NovelAppActivationHandler.cs
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
using Pixeval.Util.Threading;
using System.Threading.Tasks;
using Pixeval.Pages.NovelViewer;
using Pixeval.Util.UI;
using Pixeval.Utilities;

namespace Pixeval.Activation;

public class NovelAppActivationHandler : IAppActivationHandler
{
    public string ActivationFragment => "novel";

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
                await NovelViewerHelper.CreateWindowWithPageAsync(id);
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

