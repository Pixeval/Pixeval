// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using Imouto.BooruParser;
using Mako.Model;
using Misaki;
using Pixeval.Utilities;

namespace Pixeval.Models.Database;

public static class ArtworkSerializerTable
{
    public static FrozenDictionary<string, Func<string, ISerializable>> ArtworkTypeMethodsTable { get; } =
        new Dictionary<string, Func<string, ISerializable>>
        {
            // 本地记录中IsFavorite已过时，反序列化时直接设为默认值false
            [typeof(Illustration).FullName!] = s => Illustration.Deserialize(s).Apply(t => t.IsFavorite = false),
            [typeof(Novel).FullName!] = s => Novel.Deserialize(s).Apply(t => t.IsFavorite = false),
            [typeof(Post).FullName!] = Post.Deserialize
        }.ToFrozenDictionary();
}
