#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/DownloadStartingDeferral.cs
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

using Pixeval.Utilities.Threading;

namespace Pixeval.Download;

public class DownloadStartingDeferral
{
    /// <summary>
    ///     Set its result to <see langword="true" /> if you want the download to proceed, otherwise, <see langword="false" />
    /// </summary>
    public ReenterableAwaiter<bool> Signal { get; } = new ReenterableAwaiter<bool>(true, true);

    public void Set()
    {
        Signal.Reset();
    }

    /// <summary>
    ///     Sets the value of <see cref="Signal" /> <br></br>
    ///     Pass <see langword="true" /> if you want the download to proceed, otherwise, <see langword="false" />
    /// </summary>
    /// <param name="value"></param>
    public void Complete(bool value)
    {
        Signal.SetResult(value);
    }
}
