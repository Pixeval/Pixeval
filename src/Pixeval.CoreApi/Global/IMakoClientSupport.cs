#region Copyright (c) Pixeval/Pixeval.CoreApi

// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2021 Pixeval.CoreApi/IMakoClientSupport.cs
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

using JetBrains.Annotations;

namespace Pixeval.CoreApi.Global
{
    /// <summary>
    ///     Indicates that the each of its implementation contains a <see cref="MakoClient" />
    ///     that is to be used as a context provider, whereby "context provider" mostly refers to
    ///     the properties that are required when performing some context-aware tasks, such as the
    ///     access token while sending a request to app-api.pixiv.net
    /// </summary>
    [PublicAPI]
    public interface IMakoClientSupport
    {
        /// <summary>
        ///     The <see cref="MakoClient" /> that tends to be used as a context provider
        /// </summary>
        MakoClient MakoClient { get; }
    }
}