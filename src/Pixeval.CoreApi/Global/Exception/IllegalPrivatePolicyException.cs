#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2023 Pixeval.CoreApi/IllegalPrivatePolicyException.cs
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

using Pixeval.CoreApi.Global.Enum;

namespace Pixeval.CoreApi.Global.Exception;

/// <summary>
/// When a <see cref="PrivacyPolicy" /> is set to <see cref="PrivacyPolicy.Private" /> while the uid is not equivalent
/// to the <see cref="MakoClient.Me" />
/// </summary>
public class IllegalPrivatePolicyException : MakoException
{
    public IllegalPrivatePolicyException(long uid)
    {
        Uid = uid;
    }

    public IllegalPrivatePolicyException(string? message, long uid) : base(message)
    {
        Uid = uid;
    }

    public IllegalPrivatePolicyException(string? message, System.Exception? innerException, long uid) : base(message, innerException)
    {
        Uid = uid;
    }

    public long Uid { get; }
}
