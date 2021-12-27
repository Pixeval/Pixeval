#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2021 Pixeval.CoreApi/IllegalSortOptionException.cs
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

using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Pixeval.CoreApi.Global.Exception;

/// <summary>
///     Raised if you're trying to set the sort option to popular_desc without a premium access
/// </summary>
[PublicAPI]
public class IllegalSortOptionException : MakoException
{
    public IllegalSortOptionException()
    {
    }

    protected IllegalSortOptionException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public IllegalSortOptionException(string? message) : base(message)
    {
    }

    public IllegalSortOptionException(string? message, System.Exception? innerException) : base(message, innerException)
    {
    }
}