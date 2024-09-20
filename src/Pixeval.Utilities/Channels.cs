#region Copyright (c) Pixeval/Pixeval.Utilities
// GPL v3 License
// 
// Pixeval/Pixeval.Utilities
// Copyright (c) 2024 Pixeval.Utilities/Channels.cs
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

using System.Threading.Channels;
using System;

namespace Pixeval.Utilities;

public static class Channels
{
    public static async void OnReceive<T>(this ChannelReader<T> reader, Func<ChannelReader<T>, bool> condition, Action<T> action)
    {
        await foreach (var item in reader.ReadAllAsync())
        {
            if (!condition(reader))
                break;
            action(item);
        }
    }
}
