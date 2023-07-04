#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/ActivationRegistrar.cs
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

using System.Collections.Generic;
using System.Linq;
using Microsoft.Windows.AppLifecycle;
using Windows.ApplicationModel.Activation;

namespace Pixeval.Activation;

public static class ActivationRegistrar
{
    public static readonly List<IAppActivationHandler> FeatureHandlers = new();

    static ActivationRegistrar()
    {
        FeatureHandlers.Add(new IllustrationAppActivationHandler());
    }

    public static void Dispatch(AppActivationArguments args)
    {
        if (args is { Kind: ExtendedActivationKind.Protocol, Data: IProtocolActivatedEventArgs { Uri: var activationUri } } &&
            FeatureHandlers.FirstOrDefault(f => f.ActivationFragment == activationUri.Host) is { } handler)
        {
            handler.Execute(activationUri.PathAndQuery[1..]);
        }
    }
}
