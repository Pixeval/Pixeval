#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/ActivationRegistrar.cs
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
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Activation;
using Microsoft.Windows.AppLifecycle;
using System.Web;
using Pixeval.CoreApi.Preference;

namespace Pixeval.Activation;

public static class ActivationRegistrar
{
    public static readonly List<IAppActivationHandler> FeatureHandlers =
    [
        new IllustrationAppActivationHandler(),
        new IllustratorAppActivationHandler(),
        new NovelAppActivationHandler()
    ];

    public static void Dispatch(AppActivationArguments args)
    {
        if (args is
            {
                Kind: ExtendedActivationKind.Protocol, Data: IProtocolActivatedEventArgs { Uri: var activationUri }
            })
        {
            if (activationUri.Scheme == "pixeval")
            {
                if (FeatureHandlers.FirstOrDefault(f => f.ActivationFragment == activationUri.Host) is { } handler)
                {
                    {
                        _ = handler.Execute(activationUri.PathAndQuery[1..]);
                    }
                }
            }
            else if (activationUri.Scheme == "pixiv")
            {
                var code = HttpUtility.ParseQueryString(activationUri.Query)["code"]!;
                Session session;
                try
                {
                    session = await 
                }
            }
        }
    }
}
