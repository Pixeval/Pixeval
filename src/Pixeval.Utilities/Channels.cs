// Copyright (c) Pixeval.Utilities.
// Licensed under the GPL v3 License.

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
